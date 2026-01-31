public static class CosmosContainerExtensions
{
    public static string GetName(this CosmosContainer container) =>
        container switch
        {
            CosmosContainer.Records => "records",
            CosmosContainer.Tags => "tags",
            // CosmosContainer.ServiceOptions => "service-options",
            _ => throw new ArgumentOutOfRangeException(nameof(container))
        };
}
