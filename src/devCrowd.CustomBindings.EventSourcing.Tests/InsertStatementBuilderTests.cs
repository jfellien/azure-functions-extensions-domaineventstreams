using System.Text;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using FluentAssertions;
using Xunit;

namespace devCrowd.CustomBindings.EventSourcing.Tests;

public class InsertStatementBuilderTests
{
    [Fact]
    public void WhenHaveTableNameOnly_ItShouldNotContainEntityAndEntityId()
    {
        var statement = InsertStatementBuilder.GetStatement("sampleTable");

        statement.Should().Contain("INSERT");
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.Entity);
        statement.Should().NotContain("@entity");
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.EntityId);
        statement.Should().NotContain("@entityId");
    }
    
    [Fact]
    public void WhenHaveTableNameAndEntityOnly_ItShouldNotContainEntityId()
    {
        var columnList = new StringBuilder(
            $"{SqlServerDomainEventStreamStorageColumnNames.EventId}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Context}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventFullName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.SequenceNumber}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.PayLoad}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Entity}");
        
        var parameterList = new StringBuilder("@eventId, @context, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload, @entity");
        
        var statement = InsertStatementBuilder.GetStatement("sampleTable","sampleEntity");

        statement.Should().Contain("INSERT");
        statement.Should().Contain(columnList.ToString());
        statement.Should().Contain(parameterList.ToString());
        
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.EntityId);
        statement.Should().NotContain("@entityId");
    }
    
    [Fact]
    public void WhenHaveTableNameEntityAndEntityId_ItShouldContainEntityAndEntityId()
    {
        var columnList = new StringBuilder(
            $"{SqlServerDomainEventStreamStorageColumnNames.EventId}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Context}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventFullName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.SequenceNumber}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.PayLoad}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Entity}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EntityId}");
        
        var parameterList = new StringBuilder("@eventId, @context, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload, @entity, @entityId");

        
        var statement = InsertStatementBuilder.GetStatement("sampleTable", "sampleEntity", "sampleEntityId");

        statement.Should().Contain("INSERT");
        statement.Should().Contain(columnList.ToString());
        statement.Should().Contain(parameterList.ToString());
    }
}