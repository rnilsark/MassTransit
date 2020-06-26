namespace MassTransit.Azure.ServiceBus.Core.Transport
{
    using System;


    public interface ClientSettings
    {
        /// <summary>
        /// The number of concurrent messages to process
        /// </summary>
        int MaxConcurrentCalls { get; }

        /// <summary>
        /// The number of messages to push from the server to the client
        /// </summary>
        int PrefetchCount { get; }

        /// <summary>
        /// The timeout before the session state is renewed
        /// </summary>
        TimeSpan MaxAutoRenewDuration { get; }

        /// <summary>
        /// The timeout before a message session is abandoned
        /// </summary>
        TimeSpan MessageWaitTimeout { get; }

        /// <summary>
        /// The lock duration for messages read from the client
        /// </summary>
        TimeSpan LockDuration { get; }

        /// <summary>
        /// True if a session is required/desired
        /// </summary>
        bool RequiresSession { get; }

        /// <summary>
        /// True if the basic tier was selected
        /// </summary>
        bool UsingBasicTier { get; }

        /// <summary>
        /// The path of the message entity
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The name of the message entity
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the input address for the client on the specified host
        /// </summary>
        Uri GetInputAddress(Uri serviceUri, string path);
    }
}
