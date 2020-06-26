# Mediator

MassTransit includes a mediator implementation, with full support for consumers, handlers, and sagas (including saga state machines). MassTransit Mediator runs in-process and in-memory, no transport is required. For maximum performance, messages are passed by reference, instead than being serialized, and control flows directly from the _Publish_/_Send_ caller to the consumer. If a consumer throws an exception, the _Publish_/_Send_ method throws and the exception should be handled by the caller.

::: tip Mediator
Mediator is a [behavioral design pattern](https://en.wikipedia.org/wiki/Mediator_pattern) in which a _mediator_ encapsulates communication between objects to reduce coupling.
:::

### Configuration

Creating and configuring a mediator is similar to a bus, but uses the _CreateMediator_ factory method. Consumers and sagas are configured the same way they would on a receive endpoint. The example below configures the mediator with a single consumer.

<<< @/docs/code/usage/UsageMediator.cs

Once created, the mediator doesn't need to be started or stopped and can be used immediately. _IMediator_ combines several other interfaces into a single interface, including _IPublishEndpoint_, _ISendEndpoint_, and _IClientFactory_.

<<< @/src/MassTransit/Mediator/IMediator.cs

MassTransit dispatches the command to the consumer asynchronously. Once the _Consume_ method completes, the _Send_ method will complete. If the consumer throws an exception, it will be propagated back to the caller.

::: tip Send vs Publish
_Send_ expects the message to be consumed. If there is no consumer configured for the message type, an exception will be thrown.

_Publish_, on the other hand, does not require the message to be consumed and does not throw an exception if the message isn't consumed. To throw an exception when a published message is not consumed, set the _Mandatory_ property to _true_ on _PublishContext_.
:::

### Connect

Consumers can be connected and disconnected from mediator at run-time, allowing components and services to temporarily consume messages. Use the _ConnectConsumer_ method to connect a consumer. The handle can be used to disconnect the consumer.

<<< @/docs/code/usage/UsageMediatorConnect.cs

### Requests

To send a request using the mediator, a request client can be created from _IMediator_. The example below configures two consumers and then sends the _SubmitOrder_ command, followed by the _GetOrderStatus_ request.

<<< @/docs/code/usage/UsageMediatorRequest.cs

The _OrderStatusConsumer_, along with the message contracts, is shown below.

<<< @/docs/code/usage/UsageMediatorConsumer.cs

Just like _Send_, the request is executed asynchronously. If an exception occurs, the exception will be propagated back to the caller. If the request times out, or if the request is canceled, the _GetResponse_ method will throw an exception (either a _RequestTimeoutException_ or an _OperationCanceledException_).

### Containers

To configure mediator using a container, use the _AddMediator_ method.

<<< @/docs/code/usage/UsageMediatorContainer.cs

Consumers and sagas (including saga repositories) can be added, routing slip activities are not supported using mediator. Consumer and saga definitions are supported as well, but certain properties like _EndpointName_ are ignored. Middleware components, including _UseMessageRetry_ and _UseInMemoryOutbox_, are fully supported.
