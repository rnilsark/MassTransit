﻿namespace MassTransit.Clients.Contexts
{
    using System;
    using GreenPipes;


    public class BusClientFactoryContext :
        ClientFactoryContext
    {
        readonly IBus _bus;

        public BusClientFactoryContext(IBus bus, RequestTimeout defaultTimeout = default)
        {
            _bus = bus;

            DefaultTimeout = defaultTimeout.HasValue ? defaultTimeout : RequestTimeout.Default;
        }

        public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe)
            where T : class
        {
            return _bus.ConnectConsumePipe(pipe);
        }

        public ConnectHandle ConnectRequestPipe<T>(Guid requestId, IPipe<ConsumeContext<T>> pipe)
            where T : class
        {
            return _bus.ConnectRequestPipe(requestId, pipe);
        }

        public Uri ResponseAddress => _bus.Address;

        public IRequestSendEndpoint<T> GetRequestEndpoint<T>(ConsumeContext consumeContext = default)
            where T : class
        {
            return new PublishRequestSendEndpoint<T>(consumeContext != null
                ? consumeContext.ReceiveContext.PublishEndpointProvider
                : _bus, consumeContext);
        }

        public IRequestSendEndpoint<T> GetRequestEndpoint<T>(Uri destinationAddress, ConsumeContext consumeContext = default)
            where T : class
        {
            return new SendRequestSendEndpoint<T>((ISendEndpointProvider)consumeContext ?? _bus, destinationAddress, consumeContext);
        }

        public RequestTimeout DefaultTimeout { get; }
    }
}
