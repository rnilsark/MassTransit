﻿namespace MassTransit.Clients
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;


    public interface HandlerConnectHandle<T> :
        HandlerConnectHandle
        where T : class
    {
        Task<Response<T>> Task { get; }
    }


    public interface HandlerConnectHandle :
        ConnectHandle
    {
        void TrySetException(Exception exception);

        void TrySetCanceled();
    }
}
