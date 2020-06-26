namespace MassTransit.Courier.InternalMessages
{
    using System;
    using System.Collections.Generic;
    using Contracts;


    public class CompensationFailed :
        RoutingSlipCompensationFailed
    {
        readonly DateTime _failureTimestamp;
        readonly TimeSpan _routingSlipDuration;

        public CompensationFailed(HostInfo host, Guid trackingNumber, DateTime failureTimestamp, TimeSpan routingSlipDuration, ExceptionInfo exceptionInfo,
            IDictionary<string, object> variables)
        {
            _failureTimestamp = failureTimestamp;
            _routingSlipDuration = routingSlipDuration;
            Host = host;

            TrackingNumber = trackingNumber;
            Variables = variables;
            ExceptionInfo = exceptionInfo;
        }

        public Guid TrackingNumber { get; }

        public ExceptionInfo ExceptionInfo { get; }
        public IDictionary<string, object> Variables { get; }

        public HostInfo Host { get; }

        DateTime RoutingSlipCompensationFailed.Timestamp => _failureTimestamp;

        TimeSpan RoutingSlipCompensationFailed.Duration => _routingSlipDuration;
    }
}
