namespace MassTransit.Azure.ServiceBus.Core.Topology
{
    using Builders;
    using MassTransit.Topology;


    public interface IServiceBusPublishTopologyConfigurator :
        IPublishTopologyConfigurator,
        IServiceBusPublishTopology
    {
        
        /// <summary>
        /// Determines how type hierarchy is configured on the broker
        /// </summary>
        new PublishEndpointBrokerTopologyBuilder.Options BrokerTopologyOptions { set; }

        new IServiceBusMessagePublishTopologyConfigurator<T> GetMessageTopology<T>()
            where T : class;
    }
}
