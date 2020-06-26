﻿namespace MassTransit.AmazonSqsTransport.Builders
{
    using Amazon.SQS.Model;
    using Configuration;
    using Contexts;
    using GreenPipes;
    using MassTransit.Builders;
    using Pipeline;
    using Topology;
    using Topology.Builders;
    using Transport;
    using Transports;


    public class AmazonSqsReceiveEndpointBuilder :
        ReceiveEndpointBuilder
    {
        readonly IAmazonSqsReceiveEndpointConfiguration _configuration;
        readonly IAmazonSqsHostConfiguration _hostConfiguration;

        public AmazonSqsReceiveEndpointBuilder(IAmazonSqsHostConfiguration hostConfiguration, IAmazonSqsReceiveEndpointConfiguration configuration)
            : base(configuration)
        {
            _hostConfiguration = hostConfiguration;
            _configuration = configuration;
        }

        public override ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe)
        {
            if (_configuration.ConfigureConsumeTopology)
            {
                _configuration.Topology.Consume
                    .GetMessageTopology<T>()
                    .Subscribe();
            }

            return base.ConnectConsumePipe(pipe);
        }

        public SqsReceiveEndpointContext CreateReceiveEndpointContext()
        {
            var brokerTopology = BuildTopology(_configuration.Settings);

            var headerAdapter = new TransportSetHeaderAdapter<MessageAttributeValue>(
                new SqsHeaderValueConverter(_hostConfiguration.Settings.AllowTransportHeader),
                TransportHeaderOptions.IncludeFaultMessage);

            var deadLetterTransport = CreateDeadLetterTransport(headerAdapter);

            var errorTransport = CreateErrorTransport(headerAdapter);

            var context = new SqsQueueReceiveEndpointContext(_hostConfiguration, _configuration, brokerTopology);

            context.GetOrAddPayload(() => deadLetterTransport);
            context.GetOrAddPayload(() => errorTransport);
            context.GetOrAddPayload(() => _hostConfiguration.HostTopology);

            return context;
        }

        BrokerTopology BuildTopology(ReceiveSettings settings)
        {
            var builder = new ReceiveEndpointBrokerTopologyBuilder();

            builder.Queue = builder.CreateQueue(settings.EntityName, settings.Durable, settings.AutoDelete, settings.QueueAttributes,
                settings.QueueSubscriptionAttributes, settings.Tags);

            _configuration.Topology.Consume.Apply(builder);

            return builder.BuildTopologyLayout();
        }

        IErrorTransport CreateErrorTransport(TransportSetHeaderAdapter<MessageAttributeValue> headerAdapter)
        {
            var errorSettings = _configuration.Topology.Send.GetErrorSettings(_configuration.Settings);
            var filter = new ConfigureTopologyFilter<ErrorSettings>(errorSettings, errorSettings.GetBrokerTopology());

            return new SqsErrorTransport(errorSettings.EntityName, headerAdapter, filter);
        }

        IDeadLetterTransport CreateDeadLetterTransport(TransportSetHeaderAdapter<MessageAttributeValue> headerAdapter)
        {
            var deadLetterSettings = _configuration.Topology.Send.GetDeadLetterSettings(_configuration.Settings);
            var filter = new ConfigureTopologyFilter<DeadLetterSettings>(deadLetterSettings, deadLetterSettings.GetBrokerTopology());

            return new SqsDeadLetterTransport(deadLetterSettings.EntityName, headerAdapter, filter);
        }
    }
}
