using LogMate.Domain.Enums;

namespace LogMate.Infrastructure.Extensions;

public static class CosmosContainerExtensions
{
    public static string GetName(this CosmosContainer container) =>
        container switch
        {
            CosmosContainer.Records => "records",
            CosmosContainer.Tags => "tags",
            _ => throw new ArgumentOutOfRangeException(nameof(container))
        };
}
