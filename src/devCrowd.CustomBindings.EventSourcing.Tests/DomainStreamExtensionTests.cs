using System;
using System.Collections.Generic;
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
    
    private readonly DomainEventStream _domainEventStream;
    
    public DomainStreamExtensionTests()
    {
        IReadAndWriteDomainEvents? eventStore = SetupEventStoreMock(CONTEXT, ENTITY, ENTITY_ID);
        
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
        Mock<IReadAndWriteDomainEvents>? eventStoreMock = new Mock<IReadAndWriteDomainEvents>();

        eventStoreMock.Setup(x => x.ReadBy(context, entity, entityId, default)).ReturnsAsync(() =>
        {
            DomainEventSequence? domainEventSequence = new DomainEventSequence
            {
                new(1, new MySampleEvent("003")),
                new(2, new MySampleEvent("004")),
                new(3, new MySampleEvent("005")),
                new(4, new MySingleEvent("011"))
            };

            return domainEventSequence;
        });

        return eventStoreMock.Object;
    }
    
    [Fact]
    public async Task WhenGetAllEventOfOneType_ItShouldReturnAllEvents()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();
        
        MySampleEvent[]? allSampleEvents = events.Get<MySampleEvent>().All().ToArray();

        allSampleEvents.Count().Should().Be(5);

        allSampleEvents[0].Header.RequesterId.Should().Be("003");
        allSampleEvents[1].Header.RequesterId.Should().Be("004");
        allSampleEvents[2].Header.RequesterId.Should().Be("005");
        allSampleEvents[3].Header.RequesterId.Should().Be("001");
        allSampleEvents[4].Header.RequesterId.Should().Be("002");

    }
    
    [Fact]
    public async Task WhenRequestAnyEventOfOneType_ItShouldReturnAListOfEvents()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();
        
        IEnumerable<MySampleEvent>? anySampleEvents = events.Get<MySampleEvent>().Any().Where(e => e.Header.RequesterId is "003" or "002");

        anySampleEvents.Count().Should().Be(2);
    }
    
    [Fact]
    public async Task WhenRequestForAnyNotExistingEventOfOneType_ItShouldReturnAnEmptyList()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();
        
        IEnumerable<MyNotExistingEvent>? anySampleEvents = events.Get<MyNotExistingEvent>().Any().Where(e => e.Header.RequesterId is "003" or "002");
        
        anySampleEvents.Should().NotBeNull();
        anySampleEvents.Count().Should().Be(0);
    }

    [Fact]
    public async Task WhenRequestFirstEventOfType_ItShouldReturnTheFirst()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        MyFilterableSampleEvent? firstEvent = events.Get<MyFilterableSampleEvent>().First().Where(x => x.FilterableValue == FILTER_VALUE);

        firstEvent.Header.RequesterId.Should().Be("1111");
    }
    
    [Fact]
    public async Task WhenRequestLastEventOfType_ItShouldReturnTheLast()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        MyFilterableSampleEvent? firstEvent = events.Get<MyFilterableSampleEvent>().Last().Where(x => x.FilterableValue == FILTER_VALUE);

        firstEvent.Header.RequesterId.Should().Be("1112");
    }

    [Fact]
    public async Task WhenRequestForEarlierEventThanAnOther_ItShouldReturnTrue()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events.Event<MySampleEvent>().HappenedEarlierThan<MyFilterableSampleEvent>();

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForEarlierEventThanAnOther_ItShouldReturnFalse()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events.Event<MyFilterableSampleEvent>().HappenedEarlierThan<MySampleEvent>();

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForLaterEventThanAnOther_ItShouldReturnTrue()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events.Event<MyFilterableSampleEvent>().HappenedLaterThan<MySampleEvent>();

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForLaterEventThanAnOther_ItShouldReturnFalse()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events.Event<MySampleEvent>().HappenedLaterThan<MyFilterableSampleEvent>();

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanAnOther_ItShouldReturnFalse()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003")
            .HappenedLaterThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004");

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanAnOther_ItShouldReturnTrue()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004")
            .HappenedLaterThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003");

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificLaterEventThanAnOther_ItShouldThrowException()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
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
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
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
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003")
            .HappenedEarlierThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004");

        compareResult.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificEarlierEventThanAnOther_ItShouldReturnFalse()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        bool compareResult = events
            .Event<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "004")
            .HappenedEarlierThan<MySampleEvent>()
            .Where(x => x.Header.RequesterId == "003");

        compareResult.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenRequestForSpecificEarlierEventThanAnOther_ItShouldThrowException()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
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
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();
        
        events.Should().NotBeNull();

        FluentActions.Invoking(() => events
                .Event<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "010")
                .HappenedLaterThan<MySampleEvent>()
                .Where(x => x.Header.RequesterId == "003"))
            .Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task WhenRequestForExistingSingleEvent_ItShouldReturnTheEvent()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        MySingleEvent? singleEvent = events.Get<MySingleEvent>().TheOnlyOne();

        singleEvent.Should().NotBeNull();
        singleEvent.Header.RequesterId.Should().Be("011");
    }
    
    [Fact]
    public async Task WhenRequestForNotExistingSingleEvent_ItShouldReturnNull()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        MyNotExistingEvent? singleEvent = events.Get<MyNotExistingEvent>().TheOnlyOne();

        singleEvent.Should().BeNull();
    }

    [Fact]
    public async Task WhenRequestForOneSpecificEventByExpression_ItShouldReturnTheEvent()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        MySampleEvent? singleEvent = events.Get<MySampleEvent>().Where(e => e.Header.RequesterId is "003");

        singleEvent.Should().NotBeNull();

        singleEvent.Header.RequesterId.Should().Be("003");
    }
    
    [Fact]
    public async Task WhenRequestForOneSpecificNotExistingEventByExpression_ItShouldReturnNull()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        MySampleEvent? singleEvent = events.Get<MySampleEvent>().Where(e => e.Header.RequesterId is "030");

        singleEvent.Should().BeNull();
    }

    [Fact]
    public async Task WhenCheckForAnyExistingEvent_ItShouldReturnTrue()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        bool exists = events.Event<MySampleEvent>().Exists();

        exists.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenCheckForAnyNotExistingEvent_ItShouldReturnFalse()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        bool exists = events.Event<MyNotExistingEvent>().Exists();

        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenCheckForAnyExistingEventByExpression_ItShouldReturnTrue()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        bool exists = events.Event<MySampleEvent>().Where(e => e.Header.RequesterId is "003").Exists();

        exists.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenCheckForAnyNotExistingEventByExpression_ItShouldReturnFalse()
    {
        List<IDomainEvent>? events = (await _domainEventStream.Events()).ToList();

        bool exists = events.Event<MySampleEvent>().Where(e => e.Header.RequesterId is "999").Exists();

        exists.Should().BeFalse();
    }
}