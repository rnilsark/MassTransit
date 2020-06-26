﻿namespace Automatonymous
{
    using System;
    using Events;
    using MassTransit;


    /// <summary>
    /// A request is a state-machine based request configuration that includes
    /// the events and states related to the execution of a request.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <typeparam name="TInstance"></typeparam>
    public interface Request<in TInstance, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {
        /// <summary>
        /// The name of the request
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The settings that are used for the request, including the timeout
        /// </summary>
        RequestSettings Settings { get; }

        /// <summary>
        /// The event that is raised when the request completes and the response is received
        /// </summary>
        Event<TResponse> Completed { get; set; }

        /// <summary>
        /// The event raised when the request faults
        /// </summary>
        Event<Fault<TRequest>> Faulted { get; set; }

        /// <summary>
        /// The event raised when the request times out with no response received
        /// </summary>
        Event<RequestTimeoutExpired<TRequest>> TimeoutExpired { get; set; }

        /// <summary>
        /// The state that is transitioned to once the request is pending
        /// </summary>
        State Pending { get; set; }

        /// <summary>
        /// Sets the requestId on the instance using the configured property
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="requestId"></param>
        void SetRequestId(TInstance instance, Guid? requestId);

        /// <summary>
        /// Gets the requestId on the instance using the configured property
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Guid? GetRequestId(TInstance instance);
    }


    /// <summary>
    /// A request is a state-machine based request configuration that includes
    /// the events and states related to the execution of a request.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TResponse2"></typeparam>
    public interface Request<in TInstance, TRequest, TResponse, TResponse2> :
        Request<TInstance, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
        where TResponse2 : class
    {
        /// <summary>
        /// The event that is raised when the request completes and the response is received
        /// </summary>
        Event<TResponse2> Completed2 { get; set; }
    }
}
