﻿namespace MassTransit.AmazonSqsTransport.Configuration
{
    using System;
    using Configurators;
    using Definition;
    using Exceptions;
    using GreenPipes;
    using MassTransit.Configuration;
    using MassTransit.Configurators;
    using MassTransit.Topology;
    using MassTransit.Topology.EntityNameFormatters;
    using Topology;
    using Topology.Settings;
    using Topology.Topologies;
    using Transport;


    public class AmazonSqsHostConfiguration :
        BaseHostConfiguration<IAmazonSqsReceiveEndpointConfiguration>,
        IAmazonSqsHostConfiguration
    {
        readonly IAmazonSqsBusConfiguration _busConfiguration;
        readonly IAmazonSqsTopologyConfiguration _topologyConfiguration;
        readonly IAmazonSqsHostTopology _hostTopology;
        AmazonSqsHostSettings _hostSettings;

        public AmazonSqsHostConfiguration(IAmazonSqsBusConfiguration busConfiguration, IAmazonSqsTopologyConfiguration
            topologyConfiguration)
            : base(busConfiguration)
        {
            _busConfiguration = busConfiguration;
            _topologyConfiguration = topologyConfiguration;

            _hostSettings = new ConfigurationHostSettings();

            var messageNameFormatter = new AmazonSqsMessageNameFormatter();

            _hostTopology = new AmazonSqsHostTopology(this, messageNameFormatter, topologyConfiguration);

            ConnectionContextSupervisor = new ConnectionContextSupervisor(this, topologyConfiguration);
        }

        public override Uri HostAddress => _hostSettings.HostAddress;

        public AmazonSqsHostSettings Settings
        {
            get => _hostSettings;
            set
            {
                _hostSettings = value ?? throw new ArgumentNullException(nameof(value));

                var hostAddress = new AmazonSqsHostAddress(value.HostAddress);

                if (value.ScopeTopics && hostAddress.Scope != "/")
                {
                    var formatter = new PrefixEntityNameFormatter(_topologyConfiguration.Message.EntityNameFormatter, hostAddress.Scope.Trim('/') + "_");

                    _topologyConfiguration.Message.SetEntityNameFormatter(formatter);
                }
            }
        }

        public IRetryPolicy ConnectionRetryPolicy
        {
            get
            {
                return Retry.CreatePolicy(x =>
                {
                    x.Handle<AmazonSqsTransportException>();

                    x.Exponential(1000, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
                });
            }
        }

        public IConnectionContextSupervisor ConnectionContextSupervisor { get; }

        public void ApplyEndpointDefinition(IAmazonSqsReceiveEndpointConfigurator configurator, IEndpointDefinition definition)
        {
            configurator.ConfigureConsumeTopology = definition.ConfigureConsumeTopology;

            if (definition.IsTemporary)
            {
                configurator.AutoDelete = true;
                configurator.Durable = false;
            }

            if (definition.PrefetchCount.HasValue)
                configurator.PrefetchCount = (ushort)definition.PrefetchCount.Value;

            if (definition.ConcurrentMessageLimit.HasValue)
            {
                var concurrentMessageLimit = definition.ConcurrentMessageLimit.Value;

                // if there is a prefetchCount, and it is greater than the concurrent message limit, we need a filter
                if (!definition.PrefetchCount.HasValue || definition.PrefetchCount.Value > concurrentMessageLimit)
                {
                    configurator.UseConcurrencyLimit(concurrentMessageLimit);

                    // we should determine a good value to use based upon the concurrent message limit
                    if (definition.PrefetchCount.HasValue == false)
                    {
                        var calculatedPrefetchCount = concurrentMessageLimit * 12 / 10;

                        configurator.PrefetchCount = (ushort)calculatedPrefetchCount;
                    }
                }
            }

            definition.Configure(configurator);
        }

        public IAmazonSqsReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(string queueName,
            Action<IAmazonSqsReceiveEndpointConfigurator> configure)
        {
            var settings = new QueueReceiveSettings(queueName, true, false);
            var endpointConfiguration = _busConfiguration.CreateEndpointConfiguration();

            return CreateReceiveEndpointConfiguration(settings, endpointConfiguration, configure);
        }

        public IAmazonSqsReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(QueueReceiveSettings settings,
            IAmazonSqsEndpointConfiguration endpointConfiguration, Action<IAmazonSqsReceiveEndpointConfigurator> configure)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (endpointConfiguration == null)
                throw new ArgumentNullException(nameof(endpointConfiguration));

            var configuration = new AmazonSqsReceiveEndpointConfiguration(this, settings, endpointConfiguration);

            configure?.Invoke(configuration);

            Observers.EndpointConfigured(configuration);

            Add(configuration);

            return configuration;
        }

        IAmazonSqsHostTopology IAmazonSqsHostConfiguration.HostTopology => _hostTopology;

        void IReceiveConfigurator.ReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configureEndpoint)
        {
            ReceiveEndpoint(queueName, configureEndpoint);
        }

        void IReceiveConfigurator.ReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IReceiveEndpointConfigurator> configureEndpoint)
        {
            ReceiveEndpoint(definition, endpointNameFormatter, configureEndpoint);
        }

        public void ReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IAmazonSqsReceiveEndpointConfigurator> configureEndpoint = null)
        {
            var queueName = definition.GetEndpointName(endpointNameFormatter ?? DefaultEndpointNameFormatter.Instance);

            ReceiveEndpoint(queueName, configurator =>
            {
                ApplyEndpointDefinition(configurator, definition);
                configureEndpoint?.Invoke(configurator);
            });
        }

        public void ReceiveEndpoint(string queueName, Action<IAmazonSqsReceiveEndpointConfigurator> configureEndpoint)
        {
            CreateReceiveEndpointConfiguration(queueName, configureEndpoint);
        }

        public override IHostTopology HostTopology => _hostTopology;

        public override IReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(string queueName,
            Action<IReceiveEndpointConfigurator> configure = null)
        {
            return CreateReceiveEndpointConfiguration(queueName, configure);
        }

        public override IHost Build()
        {
            var host = new AmazonSqsHost(this, _hostTopology);

            foreach (var endpointConfiguration in Endpoints)
                endpointConfiguration.Build(host);

            return host;
        }
    }
}
