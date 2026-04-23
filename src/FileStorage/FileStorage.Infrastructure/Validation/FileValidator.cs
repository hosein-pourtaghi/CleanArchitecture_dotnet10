// FileStorage.Infrastructure/Validation/FileValidator.cs
using System.ComponentModel.DataAnnotations;
using FileStorage.Application.Common.Extensions;
using FileStorage.Application.Options;
using FileStorage.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace FileStorage.Infrastructure.Validation;

public class FileValidator : IFileValidator
{
    private readonly ValidationOptions _options;
    private readonly MagicByteValidator _magicByteValidator;
    private readonly ExecutableSignatureDetector _executableDetector;

    private readonly HashSet<string> _allowedExtensions;
    private readonly HashSet<string> _blockedExtensions;

    public FileValidator(
        IOptions<ValidationOptions> options,
        MagicByteValidator magicByteValidator,
        ExecutableSignatureDetector executableDetector)
    {
        _options = options.Value;
        _magicByteValidator = magicByteValidator;
        _executableDetector = executableDetector;

        _allowedExtensions = new HashSet<string>(
            _options.AllowedExtensions.Select(e => e.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

        _blockedExtensions = new HashSet<string>(
            _options.BlockedExtensions.Select(e => e.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);
    }
     
    public async Task<ValidationResultLib> ValidateAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        // 1. Size check
        if (fileSize > _options.MaxFileSizeBytes)
        {
            errors.Add(ValidationError.FileTooLarge(_options.MaxFileSizeBytes, fileSize));
            return ValidationResultLib.Failure(errors);
        }

        // 2. Extension check
        var extension = FileExtensions.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            errors.Add(ValidationError.ExtensionNotAllowed(extension));
        }
        else if (IsExtensionBlocked(extension))
        {
            errors.Add(ValidationError.ExtensionBlocked(extension));
            return ValidationResultLib.Failure(errors);
        }
        else if (!IsExtensionAllowed(extension))
        {
            errors.Add(ValidationError.ExtensionNotAllowed(extension));
            return ValidationResultLib.Failure(errors);
        }

        // 3. Filename sanitization check
        var sanitized = FileExtensions.SanitizeFileName(fileName);
        if (sanitized != fileName)
        {
            errors.Add(ValidationError.InvalidFileName(fileName));
            return ValidationResultLib.Failure(errors);
        }

        // 4. MIME type verification (if enabled)
        if (_options.EnableMimeTypeVerification && !string.IsNullOrEmpty(extension))
        {
            var expectedMimeType = FileExtensions.GetMimeType(extension);
            if (!string.Equals(contentType, expectedMimeType, StringComparison.OrdinalIgnoreCase))
            {
                // Allow some flexibility - only warn if significantly different
                if (!contentType.StartsWith(expectedMimeType.Split('/')[0], StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(ValidationError.InvalidMimeType(expectedMimeType, contentType));
                }
            }
        }

        // 5. Magic bytes validation (if enabled)
        if (_options.EnableMagicByteValidation && stream.Length >= _options.SkipMagicBytesForSize)
        {
            stream.Position = 0;
            var magicBytesResult = await _magicByteValidator.ValidateAsync(stream, extension, cancellationToken);
            if (!magicBytesResult.IsValid)
            {
                errors.Add(ValidationError.MagicBytesMismatch(
                    magicBytesResult.ExpectedType ?? "unknown",
                    magicBytesResult.ActualType ?? "unknown"));
                return ValidationResultLib.Failure(errors);
            }
        }

        // 6. Executable signature detection (if enabled)
        if (_options.EnableExecutableScan && stream.Length >= _options.SkipMagicBytesForSize)
        {
            stream.Position = 0;
            var executableResult = await _executableDetector.DetectAsync(stream, cancellationToken);
            if (executableResult.IsExecutable)
            {
                errors.Add(ValidationError.ExecutableDetected(executableResult.ExecutableType ?? "unknown"));
                return ValidationResultLib.Failure(errors);
            }
        }

        return errors.Count == 0
            ? ValidationResultLib.Success()
            : ValidationResultLib.Failure(errors);
    }

    public bool IsExtensionAllowed(string extension)
    {
        return _allowedExtensions.Contains(extension.ToLowerInvariant());
    }

    public bool IsExtensionBlocked(string extension)
    {
        return _blockedExtensions.Contains(extension.ToLowerInvariant());
    }

    public IReadOnlyList<string> GetAllowedExtensions() => _allowedExtensions.ToList();

    public IReadOnlyList<string> GetBlockedExtensions() => _blockedExtensions.ToList();

}
