namespace MassTransit.Azure.ServiceBus.Core.Topology.Topologies
{
    using System;
    using MassTransit.Topology;
        
    public class ServiceBusNonPolymorphicPublishTopology : ServiceBusPublishTopology
    {
        private readonly IMessageTopology _messageTopology;

        public ServiceBusNonPolymorphicPublishTopology(IMessageTopology messageTopology) : base(messageTopology)
        {
            _messageTopology = messageTopology;
        }

        protected override IMessagePublishTopologyConfigurator CreateMessageTopology<T>(Type type)
        {
            var messageTopology = new ServiceBusMessagePublishTopology<T>(_messageTopology.GetMessageTopology<T>(), this);

            OnMessageTopologyCreated(messageTopology);

            return messageTopology;
        }
    }
}
