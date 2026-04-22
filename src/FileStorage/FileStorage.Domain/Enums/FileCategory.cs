// FileStorage.Domain/Enums/FileCategory.cs
namespace FileStorage.Domain.Enums;

/// <summary>
/// Categories for organizing files by their business context.
/// </summary>
public enum FileCategory
{
    // Product related
    ProductImage = 10,
    ProductDocument = 11,

    // Customer related
    CustomerAvatar = 20,
    CustomerDocument = 21,

    // Maintenance related
    MaintenanceAttachment = 30,
    MaintenanceImage = 31,

    // General purpose
    GeneralDocument = 40,
    GeneralImage = 41,

    // Fallback
    Other = 99
}
