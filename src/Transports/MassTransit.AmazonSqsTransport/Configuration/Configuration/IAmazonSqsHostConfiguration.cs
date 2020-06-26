﻿namespace MassTransit.AmazonSqsTransport.Configuration
{
    using System;
    using GreenPipes;
    using MassTransit.Configuration;
    using Topology;
    using Topology.Settings;
    using Transport;


    public interface IAmazonSqsHostConfiguration :
        IHostConfiguration,
        IReceiveConfigurator<IAmazonSqsReceiveEndpointConfigurator>
    {
        AmazonSqsHostSettings Settings { get; set; }

        IRetryPolicy ConnectionRetryPolicy { get; }

        IConnectionContextSupervisor ConnectionContextSupervisor { get; }

        new IAmazonSqsHostTopology HostTopology { get; }

        /// <summary>
        /// Apply the endpoint definition to the receive endpoint configurator
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="definition"></param>
        void ApplyEndpointDefinition(IAmazonSqsReceiveEndpointConfigurator configurator, IEndpointDefinition definition);

        /// <summary>
        /// Create a receive endpoint configuration using the specified host
        /// </summary>
        /// <returns></returns>
        IAmazonSqsReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(string queueName,
            Action<IAmazonSqsReceiveEndpointConfigurator> configure = null);

        /// <summary>
        /// Create a receive endpoint configuration for the default host
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="endpointConfiguration"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        IAmazonSqsReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(QueueReceiveSettings settings,
            IAmazonSqsEndpointConfiguration endpointConfiguration, Action<IAmazonSqsReceiveEndpointConfigurator> configure = null);
    }
}
