# RabbitMQ

With tens of thousands of users, RabbitMQ is one of the most popular open source message brokers. RabbitMQ is lightweight and easy to deploy on premises and in the cloud. RabbitMQ can be deployed in distributed and federated configurations to meet high-scale, high-availability requirements.

MassTransit fully supports RabbitMQ, including many of the advanced features and capabilities. 

::: tip Getting Started
To get started with RabbitMQ, refer to the [configuration](/usage/configuration) section which uses RabbitMQ in the examples.
:::

## Broker Topology

With RabbitMQ, which supports exchanges and queues, messages are _sent_ or _published_ to exchanges and RabbitMQ routes those messages through exchanges to the appropriate queues.

In the example below, which configures a receive endpoint, consumer, and message type, the bus is configured to use RabbitMQ.

<<< @/docs/code/transports/RabbitMqConsoleListener.cs

The configuration includes:

* The RabbitMQ host
  - Host name: _localhost_
  - Virtual host: /
  - Username and password used to connect to the virtual host (credentials are virtual-host specific)
* The receive endpoint
  - Queue name: _order-events-listener_
  - Consumer: _OrderSubmittedEventConsumer_
    - Message type: _OrderSystem.Events.OrderSubmitted_

When the bus is started, MassTransit will create exchanges and queues on the virtual host for the receive endpoint. MassTransit creates durable, _fanout_ exchanges by default, and queues are also durable by default.

| Name | Description |
|:---|:---|
| order-events-listener | Queue for the receive endpoint
| order-events-listener | An exchange, bound to the queue, used to _send_ messages
| OrderSystem.Events:OrderSubmitted | An exchange, named by the message-type, bound to the _order-events-listener_ exchange, used to _publish_ messages

When a message is sent, the endpoint address can be one of two values:

`exchange:order-events-listener`

Send the message to the _order-events-listener_ exchange. If the exchange does not exist, it will be created. _MassTransit translates topic: to exchange: when using RabbitMQ, so that topic: addresses can be resolved – since RabbitMQ is the only supported transport that doesn't have topics._

`queue:order-events-listener`

Send the message to the _order-events-listener_ exchange. If the exchange or queue does not exist, they will created and the exchange will be bound to the queue.

With either address, RabbitMQ will route the message from the _order-events-listener_ exchange to the _order-events-listener_ queue.

When a message is published, the message is sent to the _OrderSystem.Events:OrderSubmitted_ exchange. If the exchange does not exist, it will created. RabbitMQ will route the message from the _OrderSystem.Events:OrderSubmitted_ exchange to the _order-events-listener_ exchange, and subsequently to the _order-events-listener_ queue. If other receive endpoints connected to the same virtual host include consumers that consume the _OrderSubmitted_ message, a copy of the message would be routed to each of those endpoints as well.

::: warning
If a message is published before starting the bus, so that MassTransit can create the exchanges and queues, the exchange _OrderSystem.Events:OrderSubmitted_ will be created. However, until the bus has been started at least once, there won't be a queue bound to the exchange and any published messages will be lost. Once the bus has been started, the queue will remain bound to the exchange even when the bus is stopped.
:::

Durable exchanges and queues remain configured on the virtual host, so even if the bus is stopped messages will continue to be routed to the queue. When the bus is restarted, queued messages will be consumed.

## Configuration

MassTransit includes several host-level configuration options that control the behavior for the entire bus.

|  Property                      | Type   | Description 
|-------|------------------------|--------|---
| PublisherConfirmation        | bool | MassTransit will wait until RabbitMQ confirms messages when publishing or sending messages (default: true)
| Heartbeat                    | TimeSpan |The heartbeat interval used by the RabbitMQ client to keep the connection alive
| RequestedChannelMax          | ushort | The maximum number of channels allowed on the connection
| RequestedConnectionTimeout   | TimeSpan | The connection timeout

#### UseCluster

MassTransit can connect to a cluster of RabbitMQ virtual hosts and treat them as a single virtual host. To configure a cluster, call the `UseCluster` methods, and add the cluster nodes, each of which becomes part of the virtual host identified by the host name. Each cluster node can specify either a `host` or a `host:port` combination.

#### ConfigureBatch

MassTransit will briefly buffer messages before sending them to RabbitMQ, to increase message throughput. While use of the default values is recommended, the batch options can be configured.

|  Property               | Type   | Default |Description 
|-------|------------------------|-----|--------|---
| Enabled        | bool | true | Enable or disable batch sends to RabbitMQ
| MessageLimit        | int | 100 | Limit the number of messages per batch
| SizeLimit        | int | 64K | A rough limit of the total message size
| Timeout        | TimeSpan | 4ms | The time to wait for additional messages before sending

MassTransit includes several receive endpoint level configuration options that control receive endpoint behavior.

| Property                | Type   | Description 
|-------------------------|--------|------------------
| PrefetchCount         | ushort | The number of unacknowledged messages that can be processed concurrently (default based on CPU count)
| PurgeOnStartup        | bool   | Removes all messages from the queue when the bus is started (default: false)
| AutoDelete         | bool | If true, the queue will be automatically deleted when the bus is stopped (default: false)
| Durable        | bool   | If true, messages are persisted to disk before being acknowledged (default: true)


## CloudAMQP

MassTransit can be used with CloudAMQP, which is a great SaaS-based solution to host your RabbitMQ broker. To configure MassTransit, the host and virtual host must be specified, and _UseSsl_ must be configured. 

<<< @/docs/code/transports/CloudAmqpConsoleListener.cs

## Guidance

The following recommendations should be considered _best practices_ for building applications using MassTransit, specifically with RabbitMQ.

- Published messages are routed to a receive endpoint queue by message type, using exchanges and exchange bindings. A service's receive endpoints do not affect other services or their receive endpoints, as long as they do not share the same queue. 
- Consumers and sagas should have their own receive endpoint, with a unique queue name
  - Each receive endpoint maps to one queue
  - A queue may contain more than one message type, the message type is used to deliver the message to the appropriate consumer configured on the receive endpoint.
  - If a received message is not handled by a consumer, the skipped message will be moved to a skipped queue, which is named with a \_skipped suffix.
- When running multiple instances of the same service
  - Use the same queue name for each instance
  - Messages from the queue will be load balanced across all instances (the _competing consumer_ pattern)
- If a consumer exception is thrown, the faulted message will be moved to an error queue, which is named with the \_error suffix.
- The number of concurrently processed messages can be up to the _PrefetchCount_, depending upon the number of cores available.
- For temporary receive endpoints, set _AutoDelete = true_ and _Durable = false_
- To configure _PrefetchCount_ higher than the desired concurrent message count, add _UseConcurrencyLimit(n)_ to the configuration. _This must be added before any consumers are configured._ Depending upon your consumer duration, higher values may greatly improve overall message throughput.
