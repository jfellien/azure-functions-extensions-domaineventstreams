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
        string? statement = InsertStatementBuilder.GetStatement("sampleTable");

        statement.Should().Contain("INSERT");
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.Entity);
        statement.Should().NotContain("@entity");
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.EntityId);
        statement.Should().NotContain("@entityId");
    }
    
    [Fact]
    public void WhenHaveTableNameAndEntityOnly_ItShouldNotContainEntityId()
    {
        StringBuilder? columnList = new StringBuilder(
            $"{SqlServerDomainEventStreamStorageColumnNames.EventId}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Context}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventFullName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.SequenceNumber}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.PayLoad}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Entity}");
        
        StringBuilder? parameterList = new StringBuilder("@eventId, @context, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload, @entity");
        
        string? statement = InsertStatementBuilder.GetStatement("sampleTable","sampleEntity");

        statement.Should().Contain("INSERT");
        statement.Should().Contain(columnList.ToString());
        statement.Should().Contain(parameterList.ToString());
        
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.EntityId);
        statement.Should().NotContain("@entityId");
    }
    
    [Fact]
    public void WhenHaveTableNameEntityAndEntityId_ItShouldContainEntityAndEntityId()
    {
        StringBuilder? columnList = new StringBuilder(
            $"{SqlServerDomainEventStreamStorageColumnNames.EventId}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Context}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventFullName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.SequenceNumber}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.PayLoad}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Entity}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EntityId}");
        
        StringBuilder? parameterList = new StringBuilder("@eventId, @context, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload, @entity, @entityId");

        
        string? statement = InsertStatementBuilder.GetStatement("sampleTable", "sampleEntity", "sampleEntityId");

        statement.Should().Contain("INSERT");
        statement.Should().Contain(columnList.ToString());
        statement.Should().Contain(parameterList.ToString());
    }
}