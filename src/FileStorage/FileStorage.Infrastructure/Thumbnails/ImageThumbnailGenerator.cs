// FileStorage.Infrastructure/Thumbnails/ImageThumbnailGenerator.cs
using FileStorage.Application.Options;
using FileStorage.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing; 

using static System.Net.Mime.MediaTypeNames;

namespace FileStorage.Infrastructure.Thumbnails;

public class ImageThumbnailGenerator : IThumbnailGenerator
{
    private readonly ThumbnailOptions _options;
    private readonly ILogger<ImageThumbnailGenerator> _logger;

    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp",
        "image/tiff"
    };

    public IReadOnlyList<string> SupportedContentTypes => SupportedTypes.ToList();

    public ImageThumbnailGenerator(
        IOptions<ThumbnailOptions> options,
        ILogger<ImageThumbnailGenerator>? logger = null)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool CanGenerate(string contentType, long fileSize)
    {
        if (!_options.Enabled)
            return false;

        if (!SupportedTypes.Contains(contentType))
            return false;

        if (fileSize > _options.SkipForImagesLargerThan)
        {
            _logger.LogWarning("Skipping thumbnail generation for large image: {Size} bytes", fileSize);
            return false;
        }

        return true;
    }

    public async Task<ThumbnailResult> GenerateAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            imageStream.Position = 0;
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream, cancellationToken);

            var result = new ThumbnailResult
            {
                Success = true,
                Width = image.Width,
                Height = image.Height,
                Thumbnails = new Dictionary<string, Stream>()
            };

            // Generate thumbnails for each size
            var sizes = new[]
            {
                ("small", _options.Sizes.Small.Width, _options.Sizes.Small.Height),
                ("medium", _options.Sizes.Medium.Width, _options.Sizes.Medium.Height),
                ("large", _options.Sizes.Large.Width, _options.Sizes.Large.Height)
            };

            foreach (var (name, targetWidth, targetHeight) in sizes)
            {
                using var thumbnail = image.Clone(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = ResizeMode.Max
                }));

                var stream = new MemoryStream();

                if (_options.OutputFormat.Equals("webp", StringComparison.OrdinalIgnoreCase))
                {
                    await thumbnail.SaveAsWebpAsync(stream, new WebpEncoder
                    {
                        Quality = _options.Quality
                    }, cancellationToken);
                }
                else
                { 
                    await thumbnail.SaveAsJpegAsync(stream, new JpegEncoder
                    {
                        Quality = _options.Quality
                    }, cancellationToken);
                }

                stream.Position = 0;
                result.Thumbnails[name] = stream;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnails for {FileName}", originalFileName);
            return ThumbnailResult.Failed(ex.Message);
        }
    }
}
