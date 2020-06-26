﻿namespace Automatonymous.StateMachineConnectors
{
    using GreenPipes;
    using MassTransit;
    using MassTransit.Saga;
    using MassTransit.Saga.Connectors;


    public class StateMachineSagaMessageConnector<TInstance, TMessage> :
        SagaMessageConnector<TInstance, TMessage>
        where TInstance : class, ISaga, SagaStateMachineInstance
        where TMessage : class
    {
        readonly IFilter<ConsumeContext<TMessage>> _messageFilter;
        readonly ISagaPolicy<TInstance, TMessage> _policy;
        readonly SagaFilterFactory<TInstance, TMessage> _sagaFilterFactory;

        public StateMachineSagaMessageConnector(IFilter<SagaConsumeContext<TInstance, TMessage>> consumeFilter, ISagaPolicy<TInstance, TMessage> policy,
            SagaFilterFactory<TInstance, TMessage> sagaFilterFactory, IFilter<ConsumeContext<TMessage>> messageFilter)
            : base(consumeFilter)
        {
            _policy = policy;
            _sagaFilterFactory = sagaFilterFactory;
            _messageFilter = messageFilter;
        }

        protected override void ConfigureMessagePipe(IPipeConfigurator<ConsumeContext<TMessage>> configurator, ISagaRepository<TInstance> repository,
            IPipe<SagaConsumeContext<TInstance, TMessage>> sagaPipe)
        {
            if (_messageFilter != null)
                configurator.UseFilter(_messageFilter);

            configurator.UseFilter(_sagaFilterFactory(repository, _policy, sagaPipe));
        }
    }
}
