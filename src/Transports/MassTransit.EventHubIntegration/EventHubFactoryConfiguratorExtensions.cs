namespace MassTransit
{
    using System;
    using Azure.Messaging.EventHubs.Consumer;
    using EventHubIntegration;


    public static class EventHubFactoryConfiguratorExtensions
    {
        /// <summary>
        /// Subscribe to EventHub messages using default consumer group <see cref="EventHubConsumerClient.DefaultConsumerGroupName" />
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="eventHubName">Event Hub name</param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Endpoint(this IEventHubFactoryConfigurator configurator, string eventHubName, Action<IEventHubReceiveEndpointConfigurator> configure)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.Endpoint(eventHubName, EventHubConsumerClient.DefaultConsumerGroupName, configure);
        }
    }
}
