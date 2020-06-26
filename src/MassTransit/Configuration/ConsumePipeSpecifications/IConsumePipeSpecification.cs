﻿namespace MassTransit.ConsumePipeSpecifications
{
    using GreenPipes;
    using Pipeline;


    public interface IConsumePipeSpecification :
        IConsumePipeSpecificationObserverConnector,
        ISpecification
    {
        /// <summary>
        /// Returns the specification for the message type
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <returns></returns>
        IMessageConsumePipeSpecification<T> GetMessageSpecification<T>()
            where T : class;

        /// <summary>
        /// Build the consume pipe for the specification
        /// </summary>
        /// <returns></returns>
        IConsumePipe BuildConsumePipe();
    }
}
