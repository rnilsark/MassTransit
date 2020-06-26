﻿namespace MassTransit.Azure.ServiceBus.Core
{
    using System;


    public interface IServiceBusEndpointConfigurator
    {
        /// <summary>
        /// Specify the number of messages to prefetch from the queue to the service
        /// </summary>
        /// <value>The limit</value>
        int PrefetchCount { set; }

        /// <summary>
        /// Specify the number of concurrent consumers (separate from prefetch count)
        /// </summary>
        int MaxConcurrentCalls { set; }

        /// <summary>
        /// True if the queue should be deleted if idle
        /// </summary>
        TimeSpan AutoDeleteOnIdle { set; }

        /// <summary>
        /// Set the default message time to live in the queue
        /// </summary>
        TimeSpan DefaultMessageTimeToLive { set; }

        /// <summary>
        /// Sets a value that indicates whether server-side batched operations are enabled
        /// </summary>
        bool EnableBatchedOperations { set; }

        /// <summary>
        /// Move messages to the dead letter queue on expiration (time to live exceeded)
        /// </summary>
        bool EnableDeadLetteringOnMessageExpiration { set; }

        /// <summary>
        /// Sets the path to the recipient to which the dead lettered message is forwarded.
        /// </summary>
        string ForwardDeadLetteredMessagesTo { set; }

        /// <summary>
        /// Specify the lock duration for messages read from the queue
        /// </summary>
        /// <value></value>
        TimeSpan LockDuration { set; }

        /// <summary>
        /// Sets the maximum delivery count. A message is automatically deadlettered after this number of deliveries.
        /// </summary>
        int MaxDeliveryCount { set; }

        /// <summary>
        /// Sets the queue in session mode, requiring a session for inbound messages
        /// </summary>
        bool RequiresSession { set; }

        /// <summary>
        /// Sets the user metadata.
        /// </summary>
        string UserMetadata { set; }

        /// <summary>
        /// Sets the message session timeout period
        /// </summary>
        TimeSpan MessageWaitTimeout { set; }

        /// <summary>
        /// Sets the maximum time for locks/sessions to be automatically renewed
        /// </summary>
        TimeSpan MaxAutoRenewDuration { set; }

        /// <summary>
        /// IF using the Basic Tier of Service Bus, this resets some values to avoid failing
        /// </summary>
        void SelectBasicTier();
    }
}
