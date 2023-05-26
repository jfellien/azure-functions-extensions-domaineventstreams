using System;
using System.Data.Common;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

internal static class DomainEventStreamStorageLibrary
{
    public static IReadAndWriteDomainEvents GetInstanceBy(string connectionString, string databaseName, string collectionName)
    {
        if (IsSqlServerConnectionString(connectionString))
        {
            return new SqlServerDomainEventStreamStorage(connectionString, databaseName, collectionName);
        }
        
        if (IsCosmosDbConnectionString(connectionString))
        {
            return CosmosDbDomainEventStreamStorage
                .CreateFromConnectionString(connectionString, databaseName, collectionName);
        } 
        
        if (IsCosmosDbServiceEndpoint(connectionString))
        {
            return CosmosDbDomainEventStreamStorage
                .CreateFromServiceEndpoint(connectionString, databaseName, collectionName);
        }

        throw new ApplicationException("Wrong Setup for EventStore ConnectionString");
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
    
    private static bool IsCosmosDbServiceEndpoint(string connectionString)
    {
        return connectionString != null
               && Uri.TryCreate(connectionString, UriKind.Absolute, out Uri _) 
               && connectionString.Contains("documents.azure.com");
    }
}