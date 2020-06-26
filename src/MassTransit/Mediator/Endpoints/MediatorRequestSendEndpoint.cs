namespace MassTransit.Mediator.Endpoints
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Clients;
    using Context;
    using GreenPipes;
    using Initializers;


    public class MediatorRequestSendEndpoint<TRequest> :
        IRequestSendEndpoint<TRequest>
        where TRequest : class
    {
        readonly ConsumeContext _consumeContext;
        readonly ISendEndpoint _endpoint;

        public MediatorRequestSendEndpoint(ISendEndpoint endpoint, ConsumeContext consumeContext)
        {
            _endpoint = endpoint;
            _consumeContext = consumeContext;
        }

        public Task<TRequest> Send(Guid requestId, object values, IPipe<SendContext<TRequest>> pipe, CancellationToken cancellationToken)
        {
            IMessageInitializer<TRequest> initializer = MessageInitializerCache<TRequest>.GetInitializer(values.GetType());

            if (_consumeContext != null)
            {
                InitializeContext<TRequest> initializeContext = initializer.Create(_consumeContext);

                return initializer.Send(_endpoint, initializeContext, values, new ConsumeSendPipeAdapter<TRequest>(_consumeContext, pipe, requestId));
            }

            return initializer.Send(_endpoint, values, pipe, cancellationToken);
        }

        public Task Send(Guid requestId, TRequest message, IPipe<SendContext<TRequest>> pipe, CancellationToken cancellationToken)
        {
            IPipe<SendContext<TRequest>> consumePipe = _consumeContext != null
                ? new ConsumeSendPipeAdapter<TRequest>(_consumeContext, pipe, requestId)
                : pipe;

            return _endpoint.Send(message, consumePipe, cancellationToken);
        }
    }
}
