// FileStorage.Infrastructure/Validation/MagicByteValidator.cs
namespace FileStorage.Infrastructure.Validation;

public class MagicByteValidator
{
    private static readonly Dictionary<string, (byte[] Signature, int Offset, string Name)> MagicBytes = new()
    {
        // Images
        { ".jpg", (new byte[] { 0xFF, 0xD8, 0xFF }, 0, "JPEG") },
        { ".jpeg", (new byte[] { 0xFF, 0xD8, 0xFF }, 0, "JPEG") },
        { ".png", (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, "PNG") },
        { ".gif", (new byte[] { 0x47, 0x49, 0x46, 0x38 }, 0, "GIF") },
        { ".bmp", (new byte[] { 0x42, 0x4D }, 0, "BMP") },
        { ".webp", (new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0, "WebP") }, // RIFF header
        { ".tiff", (new byte[] { 0x49, 0x49, 0x2A, 0x00 }, 0, "TIFF (Little Endian)") },
        { ".tif", (new byte[] { 0x49, 0x49, 0x2A, 0x00 }, 0, "TIFF (Little Endian)") },

        // Documents
        { ".pdf", (new byte[] { 0x25, 0x50, 0x44, 0x46 }, 0, "PDF") },
        { ".doc", (new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, 0, "Microsoft Office (Legacy)") },
        { ".docx", (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "ZIP-based Office (DOCX, XLSX, PPTX)") },

        // Archives
        { ".zip", (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "ZIP") },
        { ".rar", (new byte[] { 0x52, 0x61, 0x72, 0x21 }, 0, "RAR") },
        { ".7z", (new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, 0, "7-Zip") },

        // Text
        { ".txt", (new byte[] { 0xEF, 0xBB, 0xBF }, 0, "UTF-8 Text") }, // BOM
        { ".csv", (new byte[] { 0xEF, 0xBB, 0xBF }, 0, "UTF-8 CSV") }, // BOM

        // XML/JSON
        { ".xml", (new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, 0, "XML") },
    };

    public async Task<MagicByteValidationResult> ValidateAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken = default)
    {
        if (!MagicBytes.TryGetValue(extension.ToLowerInvariant(), out var magicInfo))
        {
            // No magic bytes defined for this extension - skip validation
            return MagicByteValidationResult.Valid;
        }

        var buffer = new byte[Math.Max(magicInfo.Signature.Length + magicInfo.Offset, 16)];
        stream.Position = 0;
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead < magicInfo.Signature.Length + magicInfo.Offset)
        {
            return MagicByteValidationResult.Invalid("File too small to validate");
        }

        var isMatch = true;
        for (int i = 0; i < magicInfo.Signature.Length; i++)
        {
            if (buffer[magicInfo.Offset + i] != magicInfo.Signature[i])
            {
                isMatch = false;
                break;
            }
        }

        if (isMatch)
        {
            return MagicByteValidationResult.Valid;
        }

        // Special case for WebP (RIFF header)
        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46) // RIFF
            {
                // Check for WEBP marker at offset 8
                if (buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
                {
                    return MagicByteValidationResult.Valid;
                }
            }
        }

        // Special case for DOCX/XLSX/PPTX (ZIP format)
        if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
        {
            if (buffer[0] == 0x50 && buffer[1] == 0x4B) // ZIP
            {
                return MagicByteValidationResult.Valid;
            }
        }

        return MagicByteValidationResult.Invalid(
            $"Expected {magicInfo.Name} magic bytes",
            magicInfo.Name,
            GetActualType(buffer));
    }

    private static string? GetActualType(byte[] bytes)
    {
        if (bytes.Length < 4)
            return null;

        // Check for common signatures
        if (bytes[0] == 0x4D && bytes[1] == 0x5A)
            return "Windows Executable (MZ)";
        if (bytes[0] == 0x7F && bytes[1] == 0x45 && bytes[2] == 0x4C && bytes[3] == 0x46)
            return "Linux Executable (ELF)";
        if (bytes[0] == 0xCA && bytes[1] == 0xFE && bytes[2] == 0xBA && bytes[3] == 0xBE)
            return "Java Class";
        if (bytes[0] == 0x50 && bytes[1] == 0x4B)
            return "ZIP Archive";
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)
            return "RIFF Container";

        return "Unknown";
    }
}

public class MagicByteValidationResult
{
    public bool IsValid { get; private set; }
    public string? Error { get; private set; }
    public string? ExpectedType { get; private set; }
    public string? ActualType { get; private set; }

    public static MagicByteValidationResult Valid => new() { IsValid = true };
    public static MagicByteValidationResult Invalid(
        string error,
        string? expectedType = null,
        string? actualType = null) => new()
        {
            IsValid = false,
            Error = error,
            ExpectedType = expectedType,
            ActualType = actualType
        };
}
