﻿namespace MassTransit.AmazonSqsTransport.Exceptions
{
    using System;
    using System.Runtime.Serialization;


    [Serializable]
    public class AmazonSqsConnectException :
        AmazonSqsTransportException
    {
        public AmazonSqsConnectException()
        {
        }

        public AmazonSqsConnectException(string message)
            : base(message)
        {
        }

        public AmazonSqsConnectException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AmazonSqsConnectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
