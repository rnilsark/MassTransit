﻿namespace MassTransit.Azure.ServiceBus.Core.Builders
{
    using System;
    using System.Linq;
    using Configuration;
    using Contexts;
    using GreenPipes;
    using MassTransit.Builders;
    using Topology;
    using Topology.Builders;
    using Transport;


    public class ServiceBusReceiveEndpointBuilder :
        ReceiveEndpointBuilder
    {
        static readonly char[] Separator = {'/'};
        readonly IServiceBusReceiveEndpointConfiguration _configuration;
        readonly IServiceBusHostConfiguration _hostConfiguration;

        public ServiceBusReceiveEndpointBuilder(IServiceBusHostConfiguration hostConfiguration, IServiceBusReceiveEndpointConfiguration configuration)
            : base(configuration)
        {
            _hostConfiguration = hostConfiguration;
            _configuration = configuration;
        }

        public override ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe)
        {
            if (_configuration.ConfigureConsumeTopology)
            {
                var subscriptionName = GenerateSubscriptionName();

                _configuration.Topology.Consume
                    .GetMessageTopology<T>()
                    .Subscribe(subscriptionName);
            }

            return base.ConnectConsumePipe(pipe);
        }

        public ServiceBusReceiveEndpointContext CreateReceiveEndpointContext()
        {
            var topologyLayout = BuildTopology(_configuration.Settings);

            var context = new ServiceBusEntityReceiveEndpointContext(_hostConfiguration, _configuration, topologyLayout);

            context.GetOrAddPayload(() => _hostConfiguration.HostTopology);

            return context;
        }

        string GenerateSubscriptionName()
        {
            var subscriptionName = _configuration.Settings.Name.Split(Separator, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            var hostScope = _configuration.HostAddress.AbsolutePath.Split(Separator, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

            return _configuration.Topology.Publish.GenerateSubscriptionName(subscriptionName, hostScope);
        }

        BrokerTopology BuildTopology(ReceiveSettings settings)
        {
            var topologyBuilder = new ReceiveEndpointBrokerTopologyBuilder();

            topologyBuilder.Queue = topologyBuilder.CreateQueue(settings.GetQueueDescription());

            _configuration.Topology.Consume.Apply(topologyBuilder);

            return topologyBuilder.BuildBrokerTopology();
        }
    }
}
