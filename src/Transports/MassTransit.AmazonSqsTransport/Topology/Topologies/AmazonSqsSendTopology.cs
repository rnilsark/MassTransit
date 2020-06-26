﻿namespace MassTransit.AmazonSqsTransport.Topology.Topologies
{
    using System;
    using System.Globalization;
    using MassTransit.Topology;
    using MassTransit.Topology.Topologies;
    using Settings;


    public class AmazonSqsSendTopology :
        SendTopology,
        IAmazonSqsSendTopologyConfigurator
    {
        public AmazonSqsSendTopology(IEntityNameValidator validator)
        {
            EntityNameValidator = validator;
        }

        public IEntityNameValidator EntityNameValidator { get; }

        public Action<IQueueConfigurator> ConfigureErrorSettings { get; set; }
        public Action<IQueueConfigurator> ConfigureDeadLetterSettings { get; set; }

        IAmazonSqsMessageSendTopologyConfigurator<T> IAmazonSqsSendTopology.GetMessageTopology<T>()
        {
            IMessageSendTopologyConfigurator<T> configurator = base.GetMessageTopology<T>();

            return configurator as IAmazonSqsMessageSendTopologyConfigurator<T>;
        }

        public SendSettings GetSendSettings(AmazonSqsEndpointAddress address)
        {
            return new QueueSendSettings(address);
        }

        public ErrorSettings GetErrorSettings(EntitySettings settings)
        {
            var errorSettings = new QueueErrorSettings(settings, BuildEntityName(settings.EntityName, "_error"));

            ConfigureErrorSettings?.Invoke(errorSettings);

            return errorSettings;
        }

        public DeadLetterSettings GetDeadLetterSettings(EntitySettings settings)
        {
            var deadLetterSetting = new QueueDeadLetterSettings(settings, BuildEntityName(settings.EntityName, "_skipped"));

            ConfigureDeadLetterSettings?.Invoke(deadLetterSetting);

            return deadLetterSetting;
        }

        protected override IMessageSendTopologyConfigurator CreateMessageTopology<T>(Type type)
        {
            var messageTopology = new AmazonSqsMessageSendTopology<T>();

            OnMessageTopologyCreated(messageTopology);

            return messageTopology;
        }

        static string BuildEntityName(string entityName, string suffix)
        {
            const string fifoSuffix = ".fifo";

            if (!entityName.EndsWith(fifoSuffix, true, CultureInfo.InvariantCulture))
                return entityName + suffix;

            return entityName.Substring(0, entityName.Length - fifoSuffix.Length) + suffix + fifoSuffix;
        }
    }
}
