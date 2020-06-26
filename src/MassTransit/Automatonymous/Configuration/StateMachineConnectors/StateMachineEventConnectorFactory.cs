﻿namespace Automatonymous.StateMachineConnectors
{
    using System;
    using MassTransit.Saga;
    using MassTransit.Saga.Connectors;
    using Pipeline;


    public class StateMachineEventConnectorFactory<TInstance, TMessage> :
        ISagaConnectorFactory
        where TInstance : class, ISaga, SagaStateMachineInstance
        where TMessage : class
    {
        readonly ISagaMessageConnector<TInstance> _connector;

        public StateMachineEventConnectorFactory(SagaStateMachine<TInstance> stateMachine, EventCorrelation<TInstance, TMessage> correlation)
        {
            var consumeFilter = new StateMachineSagaMessageFilter<TInstance, TMessage>(stateMachine, correlation.Event);

            _connector = new StateMachineSagaMessageConnector<TInstance, TMessage>(consumeFilter, correlation.Policy, correlation.FilterFactory,
                correlation.MessageFilter);
        }

        ISagaMessageConnector<T> ISagaConnectorFactory.CreateMessageConnector<T>()
        {
            if (_connector is ISagaMessageConnector<T> connector)
                return connector;

            throw new ArgumentException("The saga type did not match the connector type");
        }
    }
}
