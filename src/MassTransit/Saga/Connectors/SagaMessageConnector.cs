namespace MassTransit.Saga.Connectors
{
    using System;
    using GreenPipes;
    using MassTransit.Pipeline;
    using SagaSpecifications;


    public abstract class SagaMessageConnector<TSaga, TMessage> :
        ISagaMessageConnector<TSaga>
        where TSaga : class, ISaga
        where TMessage : class
    {
        readonly IFilter<SagaConsumeContext<TSaga, TMessage>> _consumeFilter;

        protected SagaMessageConnector(IFilter<SagaConsumeContext<TSaga, TMessage>> consumeFilter)
        {
            _consumeFilter = consumeFilter;
        }

        public Type MessageType => typeof(TMessage);

        public ISagaMessageSpecification<TSaga> CreateSagaMessageSpecification()
        {
            return new SagaMessageSpecification<TSaga, TMessage>();
        }

        public ConnectHandle ConnectSaga(IConsumePipeConnector consumePipe, ISagaRepository<TSaga> repository, ISagaSpecification<TSaga> specification)
        {
            ISagaMessageSpecification<TSaga, TMessage> messageSpecification = specification.GetMessageSpecification<TMessage>();

            IPipe<SagaConsumeContext<TSaga, TMessage>> consumerPipe = messageSpecification.BuildConsumerPipe(_consumeFilter);

            IPipe<ConsumeContext<TMessage>> messagePipe = messageSpecification.BuildMessagePipe(x =>
            {
                ConfigureMessagePipe(x, repository, consumerPipe);
            });

            return consumePipe.ConnectConsumePipe(messagePipe);
        }

        /// <summary>
        /// Configure the message pipe that is prior to the saga repository
        /// </summary>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="repository"></param>
        /// <param name="sagaPipe"></param>
        protected abstract void ConfigureMessagePipe(IPipeConfigurator<ConsumeContext<TMessage>> configurator, ISagaRepository<TSaga> repository,
            IPipe<SagaConsumeContext<TSaga, TMessage>> sagaPipe);
    }
}
