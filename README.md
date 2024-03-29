# Azure Functions Extensions DomainEventStream

A simple to use Custom Binding for Azure Functions which represents a Domain Event Stream.
The Stream can be stored in a CosmosDB or MS SQL Server, only by set up the connection string the storage will use.

Since version 3.0.0 you can use the CosmosDB with an managed identity. So the connection string is only the endpoint of your CosmosDB Account.

## How to use

Add the Binding to your Function like this:

```
public static async <your retun type> Run(
    <your trigger>,
    [DomainEventStream("<context name>", "<entity name>")]
    DomainEventStream eventStream,
    <more bindings>) {

       ...

    }
```

Well, it should be explained more. `<context name>` means its the name of the context (bounded context in DDD) 
your Domain Events are related to and `<entity name>` have a similar function. Its a name of a domain entity. Both parameters
are used to filter the Domain Event Stream if you need to read from it.

*An example*
Imagine you've developed a shop application. One context could be the order process and entities could be products.
So the DomainEventStream Bindung would be: `[DomainEventStream("orderProcess", "products")]`. This will filter from the whole Stream 
only the events of orderProcess and products.

The DomainEventStream Binding allowes two more filter settings.

Get all events relatet to a specific context:

```
[DomainEventStream("<context name>")]
```

Get all events relatet to a specific context, an entity type and id of entity:

```
[DomainEventStream("<context name>", "<entity name", "<entityId>")]
```

Because of the binding is an In/Out Binding you can push events to the storage by append it. You bind the Stream to a parameter of type
`DomainEventStream`. This type supports an `Append` method.

Push one Domain Event:
```
eventStream.Append(<your Domain Event Instance inherited from DomainEvent type>);
```

Push one Domain Event with a relation to an entity:
```
eventStream.Append(<your Domain Event Instance inherited from DomainEvent type>, <entityId>);
```

The same is possible to push a list of events.

In any case the events will related to the Binding configuration of type DomainEventStream. If you have configured an
entity name in top of your function all the appended events will related to this specifiv entity. In case 
you did not specifiy a name all the appended events are not related. Here comes one sample how to place an order
in the orderPrcess context. The event `OrderPlaced` will assigned to the `order` entities

```
public static async <your retun type> Run(
    <your trigger>,
    [DomainEventStream("orderProcess", "order")]
    DomainEventStream eventStream,
    <more bindings>) {

       ...
       eventStream.Append(new OrderPlaced(){...});
       ...

    }
```

When one or many Events are stored the events will published to a ServiceBus Topic. Any system which is
interested in the events can subscribe this topic.

## Configuration

To get the `DomainEventStream` Attribute clean and simple the infrastructure configuration will happen in
the Application Settings (or for local development in the local.settings.json file). The Binding expects a few settings.

* EVENT_STORE_CONNECTION_STRING: The connection string, where the events will stored (only CosmosDB and SQL Server available)
* EVENT_STORE_DB_NAME: Name of the dastabase where the events will stored
* DOMAIN_EVENTS_COLLECTION_NAME: Name of the collection where the events will stored (equal to table name if a SQL Server is the data sink)
* EVENT_HANDLER_CONNECTION_STRING: Connection String to the ServiceBus which will publish the events after storing

Any Domain Context needs its own topic in the ServiceBus. This topic will not created if not exists already. It should be part of automatic deployment.

## Sql Server as sink
If you use a Sql Server as Data Source/EventSource you need a table to store the events. Here comes the Create script:

```
CREATE TABLE [<your table name>] (
    [EventId]        VARCHAR (100) NOT NULL,
    [EventName]      VARCHAR (200) NOT NULL,
    [EventFullName]  VARCHAR (500) NOT NULL,
    [IsoTimeStamp]   VARCHAR (30)  NOT NULL,
    [SequenceNumber] BIGINT    NOT NULL,
    [Context]        VARCHAR (200) NULL,
    [Entity]         VARCHAR (200) NULL,
    [EntityId]       VARCHAR (100) NULL,
    [Payload]        VARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([EventId] ASC)
);
```

## Use the DomainEventStream

In many cases you will use the Event Stream to get information about the history. What is happen in the past? Do we have stored an specific event already? And so on.
That's why this library provides a bunch of DomainEventStream Extension methods. [Here](README-EXTENSIONS.md) you'll find the documentation.  
