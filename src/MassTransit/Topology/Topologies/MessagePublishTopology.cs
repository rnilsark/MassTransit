namespace MassTransit.Topology.Topologies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GreenPipes;


    public class MessagePublishTopology<TMessage> :
        IMessagePublishTopologyConfigurator<TMessage>
        where TMessage : class
    {
        readonly IList<IMessagePublishTopologyConvention<TMessage>> _conventions;
        readonly IList<IMessagePublishTopology<TMessage>> _delegateTopologies;
        readonly IList<IImplementedMessagePublishTopologyConfigurator<TMessage>> _implementedMessageTypes;
        readonly IList<IMessagePublishTopology<TMessage>> _topologies;

        public MessagePublishTopology()
        {
            _conventions = new List<IMessagePublishTopologyConvention<TMessage>>();
            _topologies = new List<IMessagePublishTopology<TMessage>>();
            _delegateTopologies = new List<IMessagePublishTopology<TMessage>>();
            _implementedMessageTypes = new List<IImplementedMessagePublishTopologyConfigurator<TMessage>>();
        }

        public void Add(IMessagePublishTopology<TMessage> publishTopology)
        {
            _topologies.Add(publishTopology);
        }

        public void AddDelegate(IMessagePublishTopology<TMessage> configuration)
        {
            _delegateTopologies.Add(configuration);
        }

        public void Apply(ITopologyPipeBuilder<PublishContext<TMessage>> builder)
        {
            ITopologyPipeBuilder<PublishContext<TMessage>> delegatedBuilder = builder.CreateDelegatedBuilder();

            foreach (IMessagePublishTopology<TMessage> topology in _delegateTopologies)
                topology.Apply(delegatedBuilder);

            foreach (IMessagePublishTopologyConvention<TMessage> convention in _conventions)
            {
                if (convention.TryGetMessagePublishTopology(out IMessagePublishTopology<TMessage> topology))
                    topology.Apply(builder);
            }

            foreach (IMessagePublishTopology<TMessage> topology in _topologies)
                topology.Apply(builder);
        }

        public virtual bool TryGetPublishAddress(Uri baseAddress, out Uri publishAddress)
        {
            publishAddress = null;
            return false;
        }

        public bool TryAddConvention(IMessagePublishTopologyConvention<TMessage> convention)
        {
            if (_conventions.Any(x => x.GetType() == convention.GetType()))
                return false;
            _conventions.Add(convention);
            return true;
        }

        public void AddOrUpdateConvention<TConvention>(Func<TConvention> add, Func<TConvention, TConvention> update)
            where TConvention : class, IMessagePublishTopologyConvention<TMessage>
        {
            for (var i = 0; i < _conventions.Count; i++)
            {
                if (_conventions[i] is TConvention convention)
                {
                    var updatedConvention = update(convention);
                    _conventions[i] = updatedConvention;
                    return;
                }
            }

            var addedConvention = add();
            if (addedConvention != null)
                _conventions.Add(addedConvention);
        }

        public virtual IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void AddImplementedMessageConfigurator<T>(IMessagePublishTopologyConfigurator<T> configurator)
            where T : class
        {
            var adapter = new ImplementedTypeAdapter<T>(configurator);

            _implementedMessageTypes.Add(adapter);
        }


        class ImplementedTypeAdapter<T> :
            IImplementedMessagePublishTopologyConfigurator<TMessage>
            where T : class
        {
            readonly IMessagePublishTopologyConfigurator<T> _configurator;

            public ImplementedTypeAdapter(IMessagePublishTopologyConfigurator<T> configurator)
            {
                _configurator = configurator;
            }
        }
    }
}
