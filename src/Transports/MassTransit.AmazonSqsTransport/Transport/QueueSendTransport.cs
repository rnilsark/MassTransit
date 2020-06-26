﻿namespace MassTransit.AmazonSqsTransport.Transport
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.SQS.Model;
    using Context;
    using Contexts;
    using GreenPipes;
    using GreenPipes.Agents;
    using Logging;
    using Transports;


    public class QueueSendTransport :
        Supervisor,
        ISendTransport
    {
        readonly SqsSendTransportContext _context;

        public QueueSendTransport(SqsSendTransportContext context)
        {
            _context = context;

            Add(context.ClientContextSupervisor);
        }

        Task ISendTransport.Send<T>(T message, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken)
        {
            if (IsStopped)
                throw new TransportUnavailableException($"The send transport is stopped: {_context.EntityName}");

            var sendPipe = new SendPipe<T>(_context, message, pipe, cancellationToken);

            return _context.ClientContextSupervisor.Send(sendPipe, cancellationToken);
        }

        public ConnectHandle ConnectSendObserver(ISendObserver observer)
        {
            return _context.ConnectSendObserver(observer);
        }


        class SendPipe<T> :
            IPipe<ClientContext>
            where T : class
        {
            readonly CancellationToken _cancellationToken;
            readonly SqsSendTransportContext _context;
            readonly T _message;
            readonly IPipe<SendContext<T>> _pipe;

            public SendPipe(SqsSendTransportContext context, T message, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken)
            {
                _context = context;
                _message = message;
                _pipe = pipe;
                _cancellationToken = cancellationToken;
            }

            public async Task Send(ClientContext context)
            {
                LogContext.SetCurrentIfNull(_context.LogContext);

                await _context.ConfigureTopologyPipe.Send(context).ConfigureAwait(false);

                var sendContext = new TransportAmazonSqsSendContext<T>(_message, _cancellationToken);

                await _pipe.Send(sendContext).ConfigureAwait(false);

                StartedActivity? activity = LogContext.IfEnabled(OperationName.Transport.Send)?.StartSendActivity(sendContext);
                try
                {
                    if (_context.SendObservers.Count > 0)
                        await _context.SendObservers.PreSend(sendContext).ConfigureAwait(false);

                    var message = new SendMessageBatchRequestEntry("", Encoding.UTF8.GetString(sendContext.Body));

                    _context.SqsSetHeaderAdapter.Set(message.MessageAttributes, sendContext.Headers);

                    _context.SqsSetHeaderAdapter.Set(message.MessageAttributes, "Content-Type", sendContext.ContentType.MediaType);
                    _context.SqsSetHeaderAdapter.Set(message.MessageAttributes, nameof(sendContext.CorrelationId), sendContext.CorrelationId);

                    if (!string.IsNullOrEmpty(sendContext.DeduplicationId))
                        message.MessageDeduplicationId = sendContext.DeduplicationId;

                    if (!string.IsNullOrEmpty(sendContext.GroupId))
                        message.MessageGroupId = sendContext.GroupId;

                    if (sendContext.DelaySeconds.HasValue)
                        message.DelaySeconds = sendContext.DelaySeconds.Value;

                    await context.SendMessage(_context.EntityName, message, sendContext.CancellationToken).ConfigureAwait(false);

                    sendContext.LogSent();

                    if (_context.SendObservers.Count > 0)
                        await _context.SendObservers.PostSend(sendContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    sendContext.LogFaulted(ex);

                    if (_context.SendObservers.Count > 0)
                        await _context.SendObservers.SendFault(sendContext, ex).ConfigureAwait(false);

                    throw;
                }
                finally
                {
                    activity?.Stop();
                }
            }

            public void Probe(ProbeContext context)
            {
            }
        }
    }
}
