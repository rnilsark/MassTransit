namespace MassTransit.EventHubIntegration
{
    using System;
    using System.Collections.Generic;


    public interface EventDataContext
    {
        DateTimeOffset EnqueuedTime { get; }
        long Offset { get; }
        string PartitionId { get; }
        string PartitionKey { get; }
        long SequenceNumber { get; }
        IReadOnlyDictionary<string, object> SystemProperties { get; }
        IDictionary<string, object> Properties { get; }
    }
}
