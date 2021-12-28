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
        var statement = InsertStatementBuilder.GetStatement("sampleTable","sampleEntity");

        statement.Should().Contain("INSERT");
        statement.Should().Contain(SqlServerDomainEventStreamStorageColumnNames.Entity);
        statement.Should().Contain("@entity");
        statement.Should().NotContain(SqlServerDomainEventStreamStorageColumnNames.EntityId);
        statement.Should().NotContain("@entityId");
    }
    
    [Fact]
    public void WhenHaveTableNameEntityAndEntityId_ItShouldContainEntityAndEntityId()
    {
        var statement = InsertStatementBuilder.GetStatement("sampleTable", "sampleEntity", "sampleEntityId");

        statement.Should().Contain("INSERT");
        statement.Should().Contain(SqlServerDomainEventStreamStorageColumnNames.Entity);
        statement.Should().Contain("@entity");
        statement.Should().Contain(SqlServerDomainEventStreamStorageColumnNames.EntityId);
        statement.Should().Contain("@entityId");
    }
}