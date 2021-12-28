using System;
using System.Linq;
using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing.EventsPublisher;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using devCrowd.CustomBindings.EventSourcing.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace devCrowd.CustomBindings.EventSourcing.Tests;

public class DomainStreamExtensionTests
{
    private const string FILTER_VALUE = "my value";
    private const string CONTEXT = "context";
    private const string ENTITY = "entity";
    private const string ENTITY_ID = "entityID";
    
    private DomainEventStream _domainEventStream;
    
    public DomainStreamExtensionTests()
    {
        var eventStore = SetupEventStoreMock(CONTEXT, ENTITY, ENTITY_ID);
        
        _domainEventStream = new DomainEventStream(
            CONTEXT, ENTITY, ENTITY_ID, 
            eventStore,
            new Mock<IPublishDomainEvents>().Object);

        _domainEventStream.Append(new MySampleEvent("001"));
        _domainEventStream.Append(new MySampleEvent("002"));
        _domainEventStream.Append(new MyFilterableSampleEvent("1111") {FilterableValue = FILTER_VALUE});
        _domainEventStream.Append(new MyFilterableSampleEvent("1112") {FilterableValue = FILTER_VALUE});
    }

    private IReadAndWriteDomainEvents SetupEventStoreMock(string context, string entity, string entityId)
    {
        var eventStoreMock = new Mock<IReadAndWriteDomainEvents>();

        eventStoreMock.Setup(x => x.ReadBy(context, entity, entityId, default)).ReturnsAsync(() =>
        {
            var domainEventSequence = new DomainEventSequence();
            
            domainEventSequence.Add(new SequencedDomainEvent(1, new MySampleEvent("003")));
            domainEventSequence.Add(new SequencedDomainEvent(2, new MySampleEvent("004")));
            domainEventSequence.Add(new SequencedDomainEvent(3, new MySampleEvent("005")));

            return domainEventSequence;
        });

        return eventStoreMock.Object;
    }
    
    [Fact]
    public async Task WhenGetAllEventOfOneType_ItShouldReturnAllEvents()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();
        
        var allSampleEvents = events.GetAll<MySampleEvent>();

        allSampleEvents.Count().Should().Be(5);

        var samplesAsArray = allSampleEvents.ToArray();

        samplesAsArray[0].Header.RequesterId.Should().Be("003");
        samplesAsArray[1].Header.RequesterId.Should().Be("004");
        samplesAsArray[2].Header.RequesterId.Should().Be("005");
        samplesAsArray[3].Header.RequesterId.Should().Be("001");
        samplesAsArray[4].Header.RequesterId.Should().Be("002");

    }

    [Fact]
    public async Task WhenRequestFirstEventOfType_ItShouldReturnTheFirst()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var firstEvent = events.GetFirst<MyFilterableSampleEvent>().Where(x => x.FilterableValue == FILTER_VALUE);

        firstEvent.Header.RequesterId.Should().Be("1111");
    }
    
    [Fact]
    public async Task WhenRequestLastEventOfType_ItShouldReturnTheLast()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var firstEvent = events.GetLast<MyFilterableSampleEvent>().Where(x => x.FilterableValue == FILTER_VALUE);

        firstEvent.Header.RequesterId.Should().Be("1112");
    }

    [Fact]
    public async Task WhenRequestForEarlierEventThanAnOther_ItShouldReturnTrue()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events.Event<MySampleEvent>().HappenedEarlierThan<MyFilterableSampleEvent>();

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForEarlierEventThanAnOther_ItShouldReturnFalse()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events.Event<MyFilterableSampleEvent>().HappenedEarlierThan<MySampleEvent>();

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForLaterEventThanAnOther_ItShouldReturnTrue()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events.Event<MyFilterableSampleEvent>().HappenedLaterThan<MySampleEvent>();

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForLaterEventThanAnOther_ItShouldReturnFalse()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events.Event<MySampleEvent>().HappenedLaterThan<MyFilterableSampleEvent>();

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanAnOther_ItShouldReturnFalse()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003")
            .HappenedLaterThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004");

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanAnOther_ItShouldReturnTrue()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004")
            .HappenedLaterThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003");

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanAnOther_ItShouldThrowException()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        FluentActions.Invoking(() => events
                .Event<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "003")
                .HappenedLaterThan<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "010"))
            .Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanNotExistingOther_ItShouldThrowException()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        FluentActions.Invoking(() => events
                .Event<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "010")
                .HappenedLaterThan<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "003"))
            .Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificEarlierEventThanAnOther_ItShouldReturnTrue()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003")
            .HappenedEarlierThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004");

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificEarlierEventThanAnOther_ItShouldReturnFalse()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        var compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004")
            .HappenedEarlierThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003");

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificEarlierEventThanAnOther_ItShouldThrowException()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        FluentActions.Invoking(() => events
                .Event<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "003")
                .HappenedLaterThan<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "010"))
            .Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificEarlierEventThanNotExistingOther_ItShouldThrowException()
    {
        var events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        FluentActions.Invoking(() => events
                .Event<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "010")
                .HappenedLaterThan<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "003"))
            .Should().Throw<ArgumentException>();
    }
}