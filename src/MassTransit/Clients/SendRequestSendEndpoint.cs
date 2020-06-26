﻿namespace MassTransit.Clients
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Context;
    using GreenPipes;
    using Initializers;


    public class SendRequestSendEndpoint<TRequest> :
        IRequestSendEndpoint<TRequest>
        where TRequest : class
    {
        readonly ConsumeContext _consumeContext;
        readonly Uri _destinationAddress;
        readonly ISendEndpointProvider _provider;

        public SendRequestSendEndpoint(ISendEndpointProvider provider, Uri destinationAddress, ConsumeContext consumeContext)
        {
            _provider = provider;
            _destinationAddress = destinationAddress;
            _consumeContext = consumeContext;
        }

        public async Task<TRequest> Send(Guid requestId, object values, IPipe<SendContext<TRequest>> pipe, CancellationToken cancellationToken)
        {
            var endpoint = await _provider.GetSendEndpoint(_destinationAddress).ConfigureAwait(false);

            IMessageInitializer<TRequest> initializer = MessageInitializerCache<TRequest>.GetInitializer(values.GetType());

            if (_consumeContext != null)
            {
                InitializeContext<TRequest> initializeContext = initializer.Create(_consumeContext);

                return await initializer.Send(endpoint, initializeContext, values, new ConsumeSendPipeAdapter<TRequest>(_consumeContext, pipe, requestId))
                    .ConfigureAwait(false);
            }

            return await initializer.Send(endpoint, values, pipe, cancellationToken).ConfigureAwait(false);
        }

        public async Task Send(Guid requestId, TRequest message, IPipe<SendContext<TRequest>> pipe, CancellationToken cancellationToken)
        {
            var endpoint = await _provider.GetSendEndpoint(_destinationAddress).ConfigureAwait(false);

            IPipe<SendContext<TRequest>> consumePipe = _consumeContext != null
                ? new ConsumeSendPipeAdapter<TRequest>(_consumeContext, pipe, requestId)
                : pipe;

            await endpoint.Send(message, consumePipe, cancellationToken).ConfigureAwait(false);
        }
    }
}
