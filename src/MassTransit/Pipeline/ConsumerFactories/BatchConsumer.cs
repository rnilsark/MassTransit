﻿namespace MassTransit.Pipeline.ConsumerFactories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Context;
    using GreenPipes;
    using Metadata;
    using Util;


    public class BatchConsumer<TConsumer, TMessage> :
        IConsumer<TMessage>
        where TMessage : class
        where TConsumer : class
    {
        readonly TaskCompletionSource<DateTime> _completed;
        readonly IConsumerFactory<TConsumer> _consumerFactory;
        readonly IPipe<ConsumerConsumeContext<TConsumer, Batch<TMessage>>> _consumerPipe;
        readonly DateTime _firstMessage;
        readonly int _messageLimit;
        readonly SortedDictionary<Guid, ConsumeContext<TMessage>> _messages;
        readonly ChannelExecutor _executor;
        readonly ChannelExecutor _dispatcher;
        readonly Timer _timer;
        DateTime _lastMessage;

        public BatchConsumer(int messageLimit, TimeSpan timeLimit, ChannelExecutor executor, ChannelExecutor dispatcher,
            IConsumerFactory<TConsumer> consumerFactory, IPipe<ConsumerConsumeContext<TConsumer, Batch<TMessage>>> consumerPipe)
        {
            _messageLimit = messageLimit;
            _executor = executor;
            _consumerFactory = consumerFactory;
            _consumerPipe = consumerPipe;
            _dispatcher = dispatcher;
            _messages = new SortedDictionary<Guid, ConsumeContext<TMessage>>();
            _completed = TaskUtil.GetTask<DateTime>();
            _firstMessage = DateTime.UtcNow;

            _timer = new Timer(TimeLimitExpired, null, timeLimit, TimeSpan.FromMilliseconds(-1));
        }

        public bool IsCompleted { get; private set; }

        async Task IConsumer<TMessage>.Consume(ConsumeContext<TMessage> context)
        {
            try
            {
                await _completed.Task.ConfigureAwait(false);
            }
            catch
            {
                // if this message was marked as successfully delivered, do not fault it
                if (context.ReceiveContext.IsDelivered)
                    return;

                // again, if it's already faulted, we don't want to fault it again
                if (context.ReceiveContext.IsFaulted)
                    return;

                throw;
            }
        }

        void TimeLimitExpired(object state)
        {
            Task.Run(() => _executor.Push(() =>
            {
                if (IsCompleted)
                    return TaskUtil.Completed;

                IsCompleted = true;

                if (_messages.Count <= 0)
                    return TaskUtil.Completed;

                List<ConsumeContext<TMessage>> messages = _messages.Values.ToList();

                return _dispatcher.Push(() => Deliver(messages[0], messages, BatchCompletionMode.Time));
            }));
        }

        public Task Add(ConsumeContext<TMessage> context)
        {
            var messageId = context.MessageId ?? NewId.NextGuid();
            _messages.Add(messageId, context);

            _lastMessage = DateTime.UtcNow;

            if (IsReadyToDeliver(context))
            {
                IsCompleted = true;

                return _dispatcher.Push(() => Deliver(context, _messages.Values.ToList(), BatchCompletionMode.Size));
            }

            return TaskUtil.Completed;
        }

        bool IsReadyToDeliver(ConsumeContext context)
        {
            if (context.GetRetryAttempt() > 0)
                return true;

            return _messages.Count == _messageLimit;
        }

        public Task ForceComplete()
        {
            IsCompleted = true;

            List<ConsumeContext<TMessage>> consumeContexts = _messages.Values.ToList();
            return consumeContexts.Count == 0
                ? TaskUtil.Completed
                : _dispatcher.Push(() => Deliver(consumeContexts.Last(), consumeContexts, BatchCompletionMode.Forced));
        }

        async Task Deliver(ConsumeContext context, IReadOnlyList<ConsumeContext<TMessage>> messages, BatchCompletionMode batchCompletionMode)
        {
            _timer.Dispose();

            Batch<TMessage> batch = new Batch(_firstMessage, _lastMessage, batchCompletionMode, messages);

            try
            {
                var proxy = new MessageConsumeContext<Batch<TMessage>>(context, batch);

                await _consumerFactory.Send(proxy, _consumerPipe).ConfigureAwait(false);

                _completed.TrySetResult(DateTime.UtcNow);
            }
            catch (OperationCanceledException exception) when (exception.CancellationToken == context.CancellationToken)
            {
                _completed.TrySetCanceled();
            }
            catch (Exception exception)
            {
                _completed.TrySetException(exception);
            }
        }


        class Batch :
            Batch<TMessage>
        {
            readonly IReadOnlyList<ConsumeContext<TMessage>> _messages;

            public Batch(DateTime firstMessageReceived, DateTime lastMessageReceived, BatchCompletionMode mode,
                IReadOnlyList<ConsumeContext<TMessage>> messages)
            {
                FirstMessageReceived = firstMessageReceived;
                LastMessageReceived = lastMessageReceived;
                Mode = mode;
                _messages = messages;
            }

            public BatchCompletionMode Mode { get; }
            public DateTime FirstMessageReceived { get; }
            public DateTime LastMessageReceived { get; }

            public ConsumeContext<TMessage> this[int index] => _messages[index];

            public int Length => _messages.Count;

            public IEnumerator<ConsumeContext<TMessage>> GetEnumerator()
            {
                return _messages.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
