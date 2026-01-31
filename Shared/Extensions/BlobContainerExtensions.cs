public static class BlobContainerExtensions
{
    public static string GetName(this BlobContainer container) =>
        container switch
        {
            BlobContainer.Receipts => "receipts",
            // CosmosContainer.Users => "users",
            // CosmosContainer.ServiceOptions => "service-options",
            _ => throw new ArgumentOutOfRangeException(nameof(container))
        };
}
