namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

internal class DomainEventStreamStorageLibrary
{
    public static IReadAndWriteDomainEvents GetInstanceBy(string connectionString, string databaseName, string collectionName)
    {
        if (IsSqlServerConnectionString(connectionString))
        {
            return new SqlServerDomainEventStreamStorage(connectionString, databaseName, collectionName);
        }

        if (IsCosmosDbConnectionString(connectionString))
        {
            return new CosmosDbDomainEventStreamStorage(connectionString, databaseName, collectionName);
        }

        return null;
    }

    private static bool IsSqlServerConnectionString(string connectionString)
    {
        string lowerVersion = connectionString.ToLowerInvariant();

        return lowerVersion.StartsWith("server") || lowerVersion.StartsWith("data source");
    }
        
    private static bool IsCosmosDbConnectionString(string connectionString)
    {
        string lowerVersion = connectionString.ToLowerInvariant();

        return lowerVersion.StartsWith("accountendpoint=https://");
    }
}