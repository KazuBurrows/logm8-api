using LogMate.Domain.Enums;

namespace LogMate.Infrastructure.Extensions;

public static class BlobContainerExtensions
{
    public static string GetName(this BlobContainer container) =>
        container switch
        {
            BlobContainer.Receipts => "receipts",
            _ => throw new ArgumentOutOfRangeException(nameof(container))
        };
}
