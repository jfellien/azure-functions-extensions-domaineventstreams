# A few words to DomainEventStreamExtensions

Hi folks, I know its quite hard to deal with an EventStream. 
Its not that best to detect if a specific event is part of the stream or if one event happened earlier than an other. 
So that's the reason I've created an Extension for the DomainEventStream. It helps you to get the right information 
about your stream.

## Get the events

`DomainEventStream` it self allows only to append events...of course. But if you want to get the events use the function `Events()`

```c#
var events = await domainEventStream.Events();
```
The variable `domainEventStream` is set up by the Function Binding, btw.

Now you have all Events in your hand as list of `DomainEvent`. This list is the basement for the Extension.
Of course you can use all the fantastic LinQ functions, but I added some special functions to the list.
Ist not because of I don't like LinQ. No no, ist because of readability

> All Events have to implement the Interface `IDomainEvent` or could inherit from `DomainEvent`.

### Get all Events of one Type

```c#
var allEvents = events.Get<T>().All();
``` 
Gets all Events of Type `T`.

### Get all Events of one Type by expression
```c#
var someEvents = events.Get<T>().Any().Where([expression]);
```
Gets all Events of Type `T` which fits to expression or gets an empty list.

### Get single Event by expression
```c#
var singleEvent = events.Get<T>().Where([expression]);
```
Gets a single Event of Type `T` which fits to the expression or returns null if not.

### Get first Event of a Type

```c#
var firstEvent = events.Get<T>().First().Where([expression]);
```
Gets the first Event of Type `T` which fits to expression or returns null if no Event will be found.

### Get last Event of a Type

```c#
var lastEvent = events.Get<T>().Last().Where([expression]);
```
Gets the last event of Type `T` which fits to expression or returns null if no Event will be found.

### Get the only one Event of stream
```c#
var theOnlyOne = events.Get<T>().TheOnlyOne();
```
Get the only one Event of Type `T` or if not exists it returns null.

### Check by order

Check by order means how to find out if an event is happen earlier or later than an other event.

#### Happened earlier

```c#
var happendEarlier = events.Event<T1>().HappenedEarlierThan<T2>();
```
Checks if any Event of Type `T1` happened earlier in the history than an Event of Type `T2`.

```c#
var happenedEarlier = events.Event<T1>()
                            .Where([expression])
                            .HappenedEarlierThan<T2>()
                            .Where([expression]);
```

It checks if Event of Type `T1` happened earlier in the history than Event of Type `T2`. 
To get true both events have to exist. The comparison is between the first of Type `T1` and the last of Type `T2`.
If any of the types exists many times maybe the result becomes a wrong value.



#### Happened later

```c#
var happendLater = events.Event<T1>().HappenedLaterThan<T2>();
```
Checks if any Event of Type `T1` happened later in the history than an Event of Type `T2`.

```c#
var happenedLater = events.Event<T1>()
                          .Where([expression])
                          .HappenedLaterThan<T2>()
                          .Where([expression]);
```

It checks if Event of Type `T1` happened later in the history than Event of Type `T2`.
To get true both events have to exist. The comparison is between the last of Type `T1` and the first of Type `T2`.
If any of the types exists many times maybe the result becomes a wrong value.

### Check by Existence

```c#
var exists = events.Event<T>().Exists();
```
Checks if an Event of Type `T` exists in the stream.

```c#
var exists = events.Event<T>().Where([expression]).Exists();
```
Checks if an Event of Type T filtered by an expression exists in the stream.