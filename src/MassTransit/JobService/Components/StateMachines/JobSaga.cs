namespace MassTransit.JobService.Components.StateMachines
{
    using System;
    using System.Collections.Generic;
    using Automatonymous;


    /// <summary>
    /// Individual turnout jobs are tracked by this state
    /// </summary>
    public class JobSaga :
        SagaStateMachineInstance
    {
        public int CurrentState { get; set; }

        public DateTime? Submitted { get; set; }
        public Uri ServiceAddress { get; set; }
        public TimeSpan? JobTimeout { get; set; }
        public IDictionary<string, object> Job { get; set; }
        public Guid JobTypeId { get; set; }

        public Guid AttemptId { get; set; }
        public int RetryAttempt { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? Completed { get; set; }
        public TimeSpan? Duration { get; set; }

        public DateTime? Faulted { get; set; }
        public string Reason { get; set; }

        public Guid? StartJobRequestId { get; set; }
        public Guid? JobSlotRequestId { get; set; }
        public Guid? JobSlotWaitToken { get; set; }
        public Guid? JobRetryDelayToken { get; set; }

        public Guid CorrelationId { get; set; }

    }
}
