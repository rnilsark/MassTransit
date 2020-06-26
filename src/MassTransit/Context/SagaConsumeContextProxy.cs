namespace MassTransit.Context
{
    using System.Threading.Tasks;
    using Saga;


    /// <summary>
    /// A consumer instance merged with a message consume context
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public class SagaConsumeContextProxy<TSaga, TMessage> :
        ConsumeContextProxy<TMessage>,
        SagaConsumeContext<TSaga, TMessage>
        where TMessage : class
        where TSaga : class, ISaga
    {
        readonly SagaConsumeContext<TSaga, TMessage> _sagaContext;

        public SagaConsumeContextProxy(ConsumeContext<TMessage> context, SagaConsumeContext<TSaga, TMessage> sagaContext)
            : base(context)
        {
            _sagaContext = sagaContext;
        }

        public TSaga Saga => _sagaContext.Saga;

        public Task SetCompleted()
        {
            return _sagaContext.SetCompleted();
        }

        public bool IsCompleted => _sagaContext.IsCompleted;
    }
}
