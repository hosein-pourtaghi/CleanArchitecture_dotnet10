// FileStorage.Infrastructure/Validation/ExecutableSignatureDetector.cs
namespace FileStorage.Infrastructure.Validation;

public class ExecutableSignatureDetector
{
    private static readonly List<(byte[] Signature, int Offset, string Type)> ExecutableSignatures = new()
    {
        // Windows executables
        (new byte[] { 0x4D, 0x5A }, 0, "Windows Executable (MZ)"),
        
        // Linux executables
        (new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, 0, "Linux ELF Executable"),
        
        // macOS executables
        (new byte[] { 0xFE, 0xED, 0xFA, 0xCE }, 0, "macOS Mach-O 32-bit"),
        (new byte[] { 0xFE, 0xED, 0xFA, 0xCF }, 0, "macOS Mach-O 64-bit"),
        (new byte[] { 0xCF, 0xFA, 0xED, 0xFE }, 0, "macOS Mach-O 64-bit (reverse)"),
        
        // Java class files
        (new byte[] { 0xCA, 0xFE, 0xBA, 0xBE }, 0, "Java Class File"),
        
        // .NET assemblies
        (new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 }, 0, ".NET Assembly"),
        
        // Shell scripts
        (new byte[] { 0x23, 0x21 }, 0, "Shell Script (#!)"),
        (new byte[] { 0x23, 0x21, 0x2F, 0x62, 0x69, 0x6E }, 0, "Shell Script (#!/bin)"),
        
        // Python scripts
        (new byte[] { 0x23, 0x21, 0x2F, 0x75, 0x73, 0x72 }, 0, "Python Script"),
        
        // Perl scripts
        (new byte[] { 0x23, 0x21, 0x2F, 0x75, 0x73, 0x72, 0x2F, 0x62, 0x69, 0x6E, 0x2F, 0x70, 0x65, 0x72, 0x6C }, 0, "Perl Script"),
    };

    public async Task<ExecutableDetectionResult> DetectAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[64];
        stream.Position = 0;
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead < 2)
            return ExecutableDetectionResult.NotExecutable;

        foreach (var (signature, offset, type) in ExecutableSignatures)
        {
            if (bytesRead < offset + signature.Length)
                continue;

            var isMatch = true;
            for (int i = 0; i < signature.Length; i++)
            {
                if (buffer[offset + i] != signature[i])
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                return new ExecutableDetectionResult
                {
                    IsExecutable = true,
                    ExecutableType = type
                };
            }
        }

        return ExecutableDetectionResult.NotExecutable;
    }
}

public class ExecutableDetectionResult
{
    public bool IsExecutable { get; set; }
    public string? ExecutableType { get; set; }

    public static ExecutableDetectionResult NotExecutable => new() { IsExecutable = false };
}
