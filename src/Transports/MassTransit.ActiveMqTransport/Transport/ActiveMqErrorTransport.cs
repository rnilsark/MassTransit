﻿namespace MassTransit.ActiveMqTransport.Transport
{
    using System.Threading.Tasks;
    using Apache.NMS;
    using GreenPipes;
    using Transports;


    public class ActiveMqErrorTransport :
        ActiveMqMoveTransport,
        IErrorTransport
    {
        public ActiveMqErrorTransport(string destination, IFilter<SessionContext> topologyFilter)
            : base(destination, topologyFilter)
        {
        }

        public Task Send(ExceptionReceiveContext context)
        {
            void PreSend(IMessage message, SendHeaders headers)
            {
                headers.SetExceptionHeaders(context);
            }

            return Move(context, PreSend);
        }
    }
}
