---
sidebarDepth: 2
---

# Automatonymous

## Introduction

[Automatonymous][1] is a state machine library for .NET and provides a C# syntax to define a state machine, including states, events, and behaviors. MassTransit includes Automatonymous, and adds instance storage, event correlation, message binding, request and response support, and scheduling. Like MassTransit, Automatonymous is free, open-source, and Apache 2.0 licensed.

::: tip V6
Automatonymous is now included by default with _MassTransit_. In previous versions, an additional package reference was required. If _MassTransit.Automatonymous_ was previously used, it must be removed as it is no longer compatible.
:::

### State Machine

A state machine defines the states, events, and behavior of a finite state machine. Implemented as a class, which is derived from `MassTransitStateMachine<T>`, a state machine is created once, and then used to apply event triggered behavior to state machine _instances_.

```cs
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
}
```

### Instance

An instance contains the data for a state machine _instance_. A new instance is created for every consumed _initial_ event where an existing instance with the same _CorrelationId_ was not found. A saga repository is used to persist instances. Instances are classes, and must implement the `SagaStateMachineInstance` interface.

```cs
public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
    }
}
```

An instance must store the current state, which can be one of three types:

| Type | Description | 
|:-----|:------------|
|State | The interface `State` type. Can be difficult to serialize, typically only used for in-memory instances, but could be used if the repository storage engine supports mapping user types to a storage type. |
|string| Easy, stores the state name. However, it takes a lot of space as the state name is repeated for every instance.|
|int   | Small, fast, but requires that each possible state be specified, in order, to assign _int_ values to each state.|

The _CurrentState_ instance state property is automatically configured if it is a `State`. For `string` or `int` types, the `InstanceState` method must be used.

To specify the _int_ state values, configure the instance state as shown below.

```cs
public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState, Submitted, Accepted);
    }
}
```

This results in the following values: 0 - Initial, 1 - Final, 2 - Submitted, 3 - Accepted

### State

States represent previously consumed events resulting in an instance being in a current _state_. An instance can only be in one state at a given time. A new instance defaults to the _Initial_ state, which is automatically defined. The _Final_ state is also defined for all state machines and is used to signfify the instance has reached the final state.

In the example, two states are declared. States are automatically initialized by the _MassTransitStateMachine_ base class constructor.

```cs
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
}
```

### Event

An event is something that happened which may result in a state change. An event can add or update instance data, as well as changing an instance's current state. The `Event<T>` is generic, where `T` must be a valid message type.

In the example below, the _SubmitOrder_ message is declared as an event including how to correlate the event to an instance.

> Unless events implement `CorrelatedBy<Guid>`, they must be declared with a correlation expression.

```cs
public interface SubmitOrder
{
    Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId));
    }

    public Event<SubmitOrder> SubmitOrder { get; private set; }
}
```

### Behavior

Behavior is what happens when an _event_ occurs during a _state_. 

Below, the _Initially_ block is used to define the behavior of the _SubmitOrder_ event during the _Initial_ state. When a _SubmitOrder_ message is consumed and an instance with a _CorrelationId_ matching the _OrderId_ is not found, a new instance will be created in the _Initial_ state. The _TransitionTo_ activity transitions the instance to the _Submitted_ state, after which the instance is persisted using the saga repository.

```cs
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted));
    }
}
```

Subsequently, the _OrderAccepted_ event could be handled by the behavior shown below.

```cs
public interface OrderAccepted
{
    Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderAccepted, x => x.CorrelateById(context => context.Message.OrderId));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted));
    }

    public Event<OrderAccepted> OrderAccepted { get; private set; }
}
```

#### Message Order

Message brokers typically do not guarantee message order. Therefore, it is important to consider out-of-order messages in state machine design.

In the example above, receiving a _SubmitOrder_ message after an _OrderAccepted_ event could cause the _SubmitOrder_ message to end up in the *_error* queue. If the _OrderAccepted_ event is received first, it would be discarded since it isn't accepted in the _Initial_ state. Below is an updated state machine that handles both of these scenarios.


```cs
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted),
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Accepted,
            Ignore(SubmitOrder));
    }
}
```

In the updated example, receiving a _SubmitOrder_ message while in an _Accepted_ state ignores the event. However, data in the event may be useful. In that case, adding behavior to copy the data to the instance could be added. Below, data from the event is captured in both scenarios.

```cs
public interface SubmitOrder
{
    Guid OrderId { get; }

    DateTime OrderDate { get; }
}

public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public DateTime? OrderDate { get; set; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .Then(x => x.Instance.OrderDate = x.Data.OrderDate)
                .TransitionTo(Submitted),
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Accepted,
            When(SubmitOrder)
                .Then(x => x.Instance.OrderDate = x.Data.OrderDate));
    }
}
```

### Configuration

To add a state machine saga to a receive endpoint:

```cs
var machine = new OrderStateMachine();
var repository = new InMemorySagaRepository<OrderState>();

var busControl = Bus.Factory.CreateUsingInMemory(cfg =>
{    
    cfg.ReceiveEndpoint("order", e =>
    {
        e.StateMachineSaga(machine, repository);
    });
});
```

The example above uses the in-memory saga repository, but any saga repository could be used. The [persistence](persistence.md) section includes details on the supported saga repositories.

To test the state machine, see the [testing](/usage/testing.md#state-machine-saga) section.

## Event

As shown above, an event is a message that can be consumed by the state machine. Events can specify any valid message type, and each event may be configured. There are several event configuration methods available.

The built-in `CorrelatedBy<Guid>` interface can be used in a message contract to specify the event `CorrelationId`.

```cs
public interface OrderCanceled :
    CorrelatedBy<Guid>
{    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderCanceled); // not required, as it is the default convention
    }
}
```

While the event is declared explicitly above, it is not required. The default convention will automatically configure events that have a `CorrelatedBy<Guid>` interface.

> While convenient, some consider the interface an intrusion of infrastructure to the message contract.

An alternative is to declare the event correlation, as shown below.

```cs
public interface SubmitOrder
{    
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId));
    }

    public Event<SubmitOrder> SubmitOrder { get; private set; }
}
```

Since `OrderId` is a `Guid`, it can be used for event correlation. When `SubmitOrder` is accepted in the _Initial_ state, and because the _OrderId_ is a _Guid_, the `CorrelationId` on the new instance is automatically assigned the _OrderId_ value.

To correlate events using another type, additional configuration is required.

```cs
public interface ExternalOrderSubmitted
{    
    string OrderNumber { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => ExternalOrderSubmitted, e => e
            .CorrelateBy(i => i.OrderNumber, x => x.Message.OrderNumber)
            .SelectId(x => NewId.NextGuid()));
    }

    public Event<ExternalOrderSubmitted> ExternalOrderSubmitted { get; private set; }
}
```

When the event doesn't have a _Guid_ that uniquely correlates to an instance, the `.SelectId` expression must be configured. In the above example, [NewId](https://www.nuget.org/packages/NewId) is used to generate a sequential identifier which will be assigned to the instance _CorrelationId_. Any property on the event can be used to initialize the _CorrelationId_.

The message headers are also available, for example, instead of always generating a new identifier, the _CorrelationId_ header could be used if present.

```cs
            .SelectId(x => x.CorrelationId ?? NewId.NextGuid());
```

::: tip
Event correlation is critical, and should be consistently applied to all events on a state machine. Consider how events received in different orders may affect subsequent event correlations.
:::

### Ignore Event

It may be necessary to ignore an event in a given state, either to avoid fault generation, or to prevent messages from being moved to the *_skipped* queue. To ignore an event in a state, use the `Ignore` method.

```cs
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted),
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Accepted,
            Ignore(SubmitOrder));
    }
}
```

### Composite Event

A composite event is configured by specifying one or more events that must be consumed, after which the composite event will be raised. A composite event uses an instance property to keep track of the required events, which is specified during configuration.

To define a composite event, the required events must first be configured along with any event behaviors, after which the composite event can be configured.

```cs
public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public int ReadyEventStatus { get; set; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted),
            When(OrderAccepted)
                .TransitionTo(Accepted));

        During(Submitted,
            When(OrderAccepted)
                .TransitionTo(Accepted));

        CompositeEvent(() => OrderReady, x => x.ReadyEventStatus, SubmitOrder, OrderAccepted);

        DuringAny(
            When(OrderReady)
                .Then(context => Console.WriteLine("Order Ready: {0}", context.Instance.CorrelationId)));
    }

    public Event OrderReady { get; private set; }
}
```

Once the _SubmitOrder_ and _OrderAccepted_ events have been consumed, the _OrderReady_ event will be triggered.

::: warning
The order of events being declared can impact the order in which they execute. Therefore, it is best to declare composite events at the end of the state machine declaration, after all other events and behaviors are declared. That way, the composite events will be raised _after_ the dependent event behaviors.
:::

### Missing Instance

If an event is not matching to an instance, the missing instance behavior can be configured.

```cs
public interface RequestOrderCancellation
{    
    Guid OrderId { get; }
}

public interface OrderNotFound
{
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderCancellationRequested, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);

            e.OnMissingInstance(m =>
            {
                return m.ExecuteAsync(x => x.RespondAsync<OrderNotFound>(new { x.OrderId }));
            });
        });
    }

    public Event<RequestOrderCancellation> OrderCancellationRequested { get; private set; }
}

```

In this example, when a cancel order request is consumed without a matching instance, a response will be sent that the order was not found. Instead of generating a `Fault`, the response is more explicit. Other missing instance options include `Discard`, `Fault`, and `Execute` (a synchronous version of _ExecuteAsync_).

### Initial Insert

To increase new instance performance, configuring an event to directly insert into a saga repository may reduce lock contention. To configure an event to insert, it should be in the _Initially_ block, as well as have a saga factory specified.

```cs
public interface SubmitOrder
{    
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => SubmitOrder, e => 
        {
            e.CorrelateById(context => context.Message.OrderId));

            e.InsertOnInitial = true;
            e.SetSagaFactory(context => new OrderState
            {
                CorrelationId = context.Message.OrderId
            })
        });

        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted));
    }

    public Event<SubmitOrder> SubmitOrder { get; private set; }
}
```

When using _InsertOnInitial_, it is critical that the saga repository is able to detect duplicate keys (in this case, _CorrelationId_ - which is initialized using _OrderId_). In this case, having a clustered primary key on _CorrelationId_ would prevent duplicate instances from being inserted. If an event is correlated using a different property, make sure that the database enforces a unique constraint on the instance property and the saga factory initializes the instance property with the event property value.

```cs
public interface ExternalOrderSubmitted
{    
    string OrderNumber { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => ExternalOrderSubmitted, e => 
        {
            e.CorrelateBy(i => i.OrderNumber, x => x.Message.OrderNumber)
            e.SelectId(x => NewId.NextGuid());

            e.InsertOnInitial = true;
            e.SetSagaFactory(context => new OrderState
            {
                CorrelationId = context.CorrelationId ?? NewId.NextGuid(),
                OrderNumber = context.Message.OrderNumber,
            })
        });

        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted));
    }

    public Event<ExternalOrderSubmitted> ExternalOrderSubmitted { get; private set; }
}
```

The database would use a unique constraint on the _OrderNumber_ to prevent duplicates, which the saga repository would detect as an existing instance, which would then be loaded to consume the event.

### Completed Instance

By default, instances are not removed from the saga repository. To configure completed instance removal, specify the method used to determine if an instance has completed.

```cs
public interface OrderCompleted
{    
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));

        DuringAny(
            When(OrderCompleted)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public Event<OrderCompleted> OrderCompleted { get; private set; }
}
```

When the instance consumes the _OrderCompleted_ event, the instance is finalized (which transitions the instance to the _Final_ state). The `SetCompletedWhenFinalized` method defines an instance in the _Final_ state as completed – which is then used by the saga repository to remove the instance.

To use a different completed expression, such as one that checks if the instance is in a _Completed_ state, use the `SetCompleted` method as shown below.

```cs
public interface OrderCompleted
{    
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));

        DuringAny(
            When(OrderCompleted)
                .TransitionTo(Completed));

        SetCompleted(async instance => 
        {
            State<TInstance> currentState = await this.GetState(instance);

            return Completed.Equals(currentState);
        });
    }

    public State Completed { get; private set; }
    public Event<OrderCompleted> OrderCompleted { get; private set; }
}
```

## Activities

State machine behaviors are defined as a sequence of activities which are executed in response to an event. In addition to the activities included with Automatonymous, MassTransit includes activities to send, publish, and schedule messages, as well as initiate and respond to requests.

### Publish

To publish an event, add a `Publish` activity.

```cs
public interface OrderSubmitted
{
    Guid OrderId { get; }    
}

public class OrderSubmittedEvent :
    OrderSubmitted
{
    public OrderSubmittedEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .Publish(context => (OrderSubmitted)new OrderSubmittedEvent(context.Instance.CorrelationId))
                .TransitionTo(Submitted));
    }
}
```

Alternatively, a message initializer can be used to eliminate the _Event_ class.

```cs
public interface OrderSubmitted
{
    Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .PublishAsync(context => context.Init<OrderSubmitted>(new { OrderId = context.Instance.CorrelationId }))
                .TransitionTo(Submitted));
    }
}
```

### Send

To send a message, add a `Send` activity.

```cs
public interface UpdateAccountHistory
{
    Guid OrderId { get; }    
}

public class UpdateAccountHistoryCommand :
    UpdateAccountHistory
{
    public UpdateAccountHistoryCommand(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine(OrderStateMachineSettings settings)
    {
        Initially(
            When(SubmitOrder)
                .Send(settings.AccountServiceAddress, context => new UpdateAccountHistoryCommand(context.Instance.CorrelationId))
                .TransitionTo(Submitted));
    }
}
```

Alternatively, a message initializer can be used to eliminate the _Command_ class.

```cs
public interface UpdateAccountHistory
{
    Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine(OrderStateMachineSettings settings)
    {
        Initially(
            When(SubmitOrder)
                .SendAsync(settings.AccountServiceAddress, context => context.Init<UpdateAccountHistory>(new { OrderId = context.Instance.CorrelationId }))
                .TransitionTo(Submitted));
    }
}
```

### Respond

A state machine can respond to requests by configuring the request message type as an event, and using the `Respond` method. When configuring a request event, configuring a missing instance method is recommended, to provide a better response experience (either through a different response type, or a response that indicates an instance was not found).

```cs
public interface RequestOrderCancellation
{    
    Guid OrderId { get; }
}

public interface OrderCanceled
{
    Guid OrderId { get; }
}

public interface OrderNotFound
{
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderCancellationRequested, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);

            e.OnMissingInstance(m =>
            {
                return m.ExecuteAsync(x => x.RespondAsync<OrderNotFound>(new { x.OrderId }));
            });
        });

        DuringAny(
            When(OrderCancellationRequested)
                .RespondAsync(context => context.Init<OrderCanceled>(new { OrderId = context.Instance.CorrelationId }))
                .TransitionTo(Canceled));
    }

    public State Canceled { get; private set; }
    public Event<RequestOrderCancellation> OrderCancellationRequested { get; private set; }
}
```

### Schedule

::: tip NOTE
The bus must be configured to include a message scheduler to use the scheduling activities. See the [scheduling](/advanced/scheduling/) section to learn how to setup a message scheduler.
:::

A state machine can schedule events, which uses the message scheduler to schedule a message for delivery to the instance. First, the schedule must be declared.

```cs {1-4,12,20-25,28}
public interface OrderCompletionTimeoutExpired
{
    Guid OrderId { get; }
}

public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public Guid? OrderCompletionTimeoutTokenId { get; set; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Schedule(() => OrderCompletionTimeout, instance => instance.OrderCompletionTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromDays(30);

            s.Received = r => r.CorrelateById(context => context.Message.OrderId);
        });
    }

    public Schedule<OrderState, OrderCompletionTimeoutExpired> OrderCompletionTimeout { get; private set; }
}
```

The configuration specifies the _Delay_, which can be overridden by the schedule activity, and the correlation expression for the _Received_ event. The state machine can consume the _Received_ event as shown. The _OrderCompletionTimeoutTokenId_ is a `Guid?` instance property used to keep track of the scheduled message _tokenId_ which can later be used to unschedule the event.

```cs {12}
public interface OrderCompleted
{
    Guid OrderId { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        During(Accepted,
            When(OrderCompletionTimeout.Received)
                .PublishAsync(context => context.Init<OrderCompleted>(new { OrderId = context.Instance.CorrelationId }))
                .Finalize());
    }

    public Schedule<OrderState, OrderCompletionTimeoutExpired> OrderCompletionTimeout { get; private set; }
}
```

The event can be scheduled using the `Schedule` activity.

```cs {8}
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        During(Submitted,
            When(OrderAccepted)
                .Schedule(OrderCompletionTimeout, context => context.Init<OrderCompletionTimeoutExpired>(new { OrderId = context.Instance.CorrelationId }))
                .TransitionTo(Accepted));
    }
}
```

As stated above, the _delay_ can be overridden by the _Schedule_ activity. Both instance and message (_context.Data_) content can be used to calculate the delay.

```cs {14-15}
public interface OrderAccepted
{
    Guid OrderId { get; }    
    TimeSpan CompletionTime { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        During(Submitted,
            When(OrderAccepted)
                .Schedule(OrderCompletionTimeout, context => context.Init<OrderCompletionTimeoutExpired>(new { OrderId = context.Instance.CorrelationId }),
                    context => context.Data.CompletionTime)
                .TransitionTo(Accepted));
    }
}
```

Once the scheduled event is received, the `OrderCompletionTimeoutTokenId` property is cleared.

If the scheduled event is no longer needed, the _Unschedule_ activity can be used.

```cs {15}
public interface OrderAccepted
{
    Guid OrderId { get; }    
    TimeSpan CompletionTime { get; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        DuringAny(
            When(OrderCancellationRequested)
                .RespondAsync(context => context.Init<OrderCanceled>(new { OrderId = context.Instance.CorrelationId }))
                .Unschedule(OrderCompletionTimeout)
                .TransitionTo(Canceled));
    }
}
```

### Request

A request can be sent from a state machine using the _Request_ method. Defining a request includes specifying an instance property to store the _RequestId_ so that response event(s) can be correlated to the instance, as well as specifying a default request timeout.

::: tip NOTE
The bus must be configured to include a message scheduler to use the request activities. See the [scheduling](/advanced/scheduling/) section to learn how to setup a message scheduler.
:::

To declare a request, add a `Request` property and configure it using the `Request` method.

```cs
public interface ProcessOrder
{
    Guid OrderId { get; }    
}

public interface OrderProcessed
{
    Guid OrderId { get; }
    Guid ProcessingId { get; }
}

public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public Guid? ProcessOrderRequestId { get; set; }
    public Guid? ProcessingId { get; set; }
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine(OrderStateMachineSettings settings)
    {
        Request(() => ProcessOrder, x => x.ProcessOrderRequestId, r =>
        {
            r.ServiceAddress = settings.ProcessOrderServiceAddress;
            r.Timeout = settings.RequestTimeout;
        });
    }

    public Request<OrderState, ProcessOrder, OrderProcessed> ProcessOrder { get; private set; }
}
```

Once defined, the request activity can be added to a behavior.

```cs
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        During(Submitted,
            When(OrderAccepted)
                .Request(ProcessOrder, x => x.Init<ProcessOrder>(new { OrderId = x.Instance.CorrelationId}))
                .TransitionTo(ProcessOrder.Pending));

        During(ProcessOrder.Pending,
            When(ProcessOrder.Completed)
                .Then(context => context.Instance.ProcessingId = context.Data.ProcessingId)
                .TransitionTo(Processed),
            When(ProcessOrder.Faulted)
                .TransitionTo(ProcessFaulted),
            When(ProcessOrder.TimeoutExpired)
                .TransitionTo(ProcessTimeoutExpired));
    }

    public State Processed { get; private set; }
    public State ProcessFaulted { get; private set; }
    public State ProcessTimeoutExpired { get; private set; }
}
```

The _Request_ includes three events: _Completed, _Faulted_, and _TimeoutExpired_. These events can be consumed during any state, however, the _Request_ includes a _Pending_ state which can be used to avoid declaring a separate pending state. 

::: tip NOTE
The request timeout is scheduled using the message scheduler, and the scheduled message is canceled when a response or fault is received. Not all message schedulers support cancellation, so it may be necessary to _Ignore_ the `TimeoutExpired` event in subsequent states.
:::

### Custom

There are scenarios when an event behavior may have dependencies that need to be managed at a scope level, such as a database connection, or the complexity is best encapsulated in a separate class rather than being part of the state machine itself. Developers can create their own activities for state machine use, and optionally create their own extension methods to add them to a behavior.

To create an activity, create a class that implements `IActivity<TInstance, TData>` as shown.

```cs
public class PublishOrderSubmittedActivity :
    Activity<OrderState, SubmitOrder>
{
    readonly ConsumeContext _context;

    public PublishOrderSubmittedActivity(ConsumeContext context)
    {
        _context = context;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("publish-order-submitted");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<OrderState, SubmitOrder> context, Behavior<OrderState, SubmitOrder> next)
    {
        // do the activity thing
        await _context.Publish<OrderSubmitted>(new { OrderId = context.Instance.CorrelationId }).ConfigureAwait(false);

        // call the next activity in the behavior
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<OrderState, SubmitOrder, TException> context, Behavior<OrderState, SubmitOrder> next)
        where TException : Exception
    {
        return next.Faulted(context);
    }
}
```

Once created, configure the activity in a state machine as shown.

```cs
public interface OrderSubmitted
{
    Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .Activity(x => x.OfType<PublishOrderSubmittedActivity>())
                .TransitionTo(Submitted));
    }
}
```

When the `SubmitOrder` event is consumed, the state machine will resolve the activity from the container, and call the `Execute` method. The activity will be scoped, so any dependencies will be resolved within the message `ConsumeContext`.

In the above example, the event type was known in advance. If an activity for any event type is needed, it can be created without specifying the event type. 

```cs
public class PublishOrderSubmittedActivity :
    Activity<OrderState>
{
    readonly ConsumeContext _context;

    public PublishOrderSubmittedActivity(ConsumeContext context)
    {
        _context = context;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("publish-order-submitted");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<OrderState> context, Behavior<OrderState> next)
    {
        await _context.Publish<OrderSubmitted>(new { OrderId = context.Instance.CorrelationId }).ConfigureAwait(false);

        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Execute<T>(BehaviorContext<OrderState, T> context, Behavior<OrderState, T> next)
    {
        await _context.Publish<OrderSubmitted>(new { OrderId = context.Instance.CorrelationId }).ConfigureAwait(false);

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<OrderState, TException> context, Behavior<OrderState> next) 
        where TException : Exception
    {
        return next.Faulted(context);
    }

    public Task Faulted<T, TException>(BehaviorExceptionContext<OrderState, T, TException> context, Behavior<OrderState, T> next)
        where TException : Exception
    {
        return next.Faulted(context);
    }
}
```

To register an instance activity, use the following syntax.

```cs
public interface OrderSubmitted
{
    Guid OrderId { get; }    
}

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Initially(
            When(SubmitOrder)
                .Activity(x => x.OfInstanceType<PublishOrderSubmittedActivity>())
                .TransitionTo(Submitted));
    }
}
```



[1]: https://github.com/MassTransit/Automatonymous
[2]: https://github.com/MassTransit/Sample-ShoppingWeb
