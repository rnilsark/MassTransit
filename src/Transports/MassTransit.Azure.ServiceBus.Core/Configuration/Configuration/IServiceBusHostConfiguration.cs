﻿namespace MassTransit.Azure.ServiceBus.Core.Configuration
{
    using System;
    using GreenPipes;
    using MassTransit.Configuration;
    using Pipeline;
    using Settings;
    using Topology;
    using Transport;


    public interface IServiceBusHostConfiguration :
        IHostConfiguration,
        IReceiveConfigurator<IServiceBusReceiveEndpointConfigurator>
    {
        ServiceBusHostSettings Settings { get; set; }

        string BasePath { get; }

        IConnectionContextSupervisor ConnectionContextSupervisor { get; }

        IRetryPolicy RetryPolicy { get; }

        new IServiceBusHostTopology HostTopology { get; }

        ISendEndpointContextSupervisor CreateSendEndpointContextSupervisor(SendSettings settings);

        /// <summary>
        /// Apply the endpoint definition to the receive endpoint configurator
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="definition"></param>
        void ApplyEndpointDefinition(IServiceBusReceiveEndpointConfigurator configurator, IEndpointDefinition definition);

        IServiceBusReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(string queueName,
            Action<IServiceBusReceiveEndpointConfigurator> configure = null);

        IServiceBusReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(ReceiveEndpointSettings settings, IServiceBusEndpointConfiguration
            endpointConfiguration, Action<IServiceBusReceiveEndpointConfigurator> configure = null);

        IServiceBusSubscriptionEndpointConfiguration CreateSubscriptionEndpointConfiguration(SubscriptionEndpointSettings settings,
            Action<IServiceBusSubscriptionEndpointConfigurator> configure = null);

        void SubscriptionEndpoint<T>(string subscriptionName, Action<IServiceBusSubscriptionEndpointConfigurator> configure)
            where T : class;

        void SubscriptionEndpoint(string subscriptionName, string topicPath, Action<IServiceBusSubscriptionEndpointConfigurator> configure);

        void SetNamespaceSeparatorToTilde();

        void SetNamespaceSeparatorToUnderscore();

        void SetNamespaceSeparatorTo(string separator);
    }
}
