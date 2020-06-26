﻿namespace MassTransit.Azure.ServiceBus.Core.Topology
{
    using Builders;
    using MassTransit.Topology;


    public interface IServiceBusPublishTopology :
        IPublishTopology
    {
        
        /// <summary>
        /// Determines how type hierarchy is configured on the broker
        /// </summary>
        PublishEndpointBrokerTopologyBuilder.Options BrokerTopologyOptions { get; } 

        new IServiceBusMessagePublishTopology<T> GetMessageTopology<T>()
            where T : class;

        /// <summary>
        /// Formats a subscription name to be 50 characters if it is greater than 50 characters.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string FormatSubscriptionName(string name);

        /// <summary>
        /// Generate a subscription name that is less than 50 characters, using the entity name and host address
        /// </summary>
        /// <param name="entityName">The entity name of the destination queue or topic</param>
        /// <param name="hostScope">The absolute path of the host, which is usually the scope</param>
        /// <returns></returns>
        string GenerateSubscriptionName(string entityName, string hostScope = default);
    }
}
