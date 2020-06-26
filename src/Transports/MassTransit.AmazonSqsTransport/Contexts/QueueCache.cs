namespace MassTransit.AmazonSqsTransport.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.SQS;
    using Amazon.SQS.Model;
    using Context;
    using GreenPipes.Caching;
    using Topology.Entities;


    public class QueueCache :
        IAsyncDisposable
    {
        readonly ICache<QueueInfo> _cache;
        readonly CancellationToken _cancellationToken;
        readonly IAmazonSQS _client;
        readonly IDictionary<string, QueueInfo> _durableQueues;
        readonly IIndex<string, QueueInfo> _nameIndex;

        public QueueCache(IAmazonSQS client, CancellationToken cancellationToken)
        {
            _client = client;
            _cancellationToken = cancellationToken;
            _cache = new GreenCache<QueueInfo>(ClientContextCacheDefaults.GetCacheSettings());
            _nameIndex = _cache.AddIndex("entityName", x => x.EntityName);

            _durableQueues = new Dictionary<string, QueueInfo>();
        }

        public async ValueTask DisposeAsync()
        {
            QueueInfo[] queueInfos;
            lock (_durableQueues)
            {
                queueInfos = _durableQueues.Values.ToArray();

                _durableQueues.Clear();
            }

            foreach (var queueInfo in queueInfos)
                await queueInfo.DisposeAsync().ConfigureAwait(false);

            _cache.Clear();
        }

        public Task<QueueInfo> Get(Queue queue, CancellationToken cancellationToken)
        {
            lock (_durableQueues)
            {
                if (_durableQueues.TryGetValue(queue.EntityName, out var queueInfo))
                    return Task.FromResult(queueInfo);
            }

            return _nameIndex.Get(queue.EntityName, key => CreateMissingQueue(queue, cancellationToken));
        }

        public Task<QueueInfo> GetByName(string entityName)
        {
            lock (_durableQueues)
            {
                if (_durableQueues.TryGetValue(entityName, out var queueInfo))
                    return Task.FromResult(queueInfo);
            }

            return _nameIndex.Get(entityName);
        }

        public void RemoveByName(string entityName)
        {
            lock (_durableQueues)
                _durableQueues.Remove(entityName);

            _nameIndex.Remove(entityName);
        }

        async Task<QueueInfo> CreateMissingQueue(Queue queue, CancellationToken cancellationToken)
        {
            Dictionary<string, string> attributes = queue.QueueAttributes.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (queue.EntityName.EndsWith(".fifo", true, CultureInfo.InvariantCulture) && !attributes.ContainsKey(QueueAttributeName.FifoQueue))
            {
                LogContext.Warning?.Log("Using '.fifo' suffix without 'FifoQueue' attribute might cause unexpected behavior.");

                attributes[QueueAttributeName.FifoQueue] = "true";
            }

            var request = new CreateQueueRequest(queue.EntityName)
            {
                Attributes = attributes,
                Tags = queue.QueueTags.ToDictionary(x => x.Key, x => x.Value)
            };

            var response = await _client.CreateQueueAsync(request, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessfulResponse();

            var queueUrl = response.QueueUrl;

            Dictionary<string, string> queueAttributes = await _client.GetAttributesAsync(queueUrl).ConfigureAwait(false);

            var missingQueue = new QueueInfo(queue.EntityName, queueUrl, queueAttributes, _client, _cancellationToken);

            if (queue.Durable && queue.AutoDelete == false)
            {
                lock (_durableQueues)
                    _durableQueues[missingQueue.EntityName] = missingQueue;
            }

            return missingQueue;
        }
    }
}
