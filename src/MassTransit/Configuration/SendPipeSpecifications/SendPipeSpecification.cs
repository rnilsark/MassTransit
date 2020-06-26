namespace MassTransit.SendPipeSpecifications
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GreenPipes;
    using Metadata;


    public class SendPipeSpecification :
        ISendPipeConfigurator,
        ISendPipeSpecification
    {
        readonly object _lock = new object();
        readonly ConcurrentDictionary<Type, IMessageSendPipeSpecification> _messageSpecifications;
        readonly SendPipeSpecificationObservable _observers;
        readonly IList<IPipeSpecification<SendContext>> _specifications;

        public SendPipeSpecification()
        {
            _specifications = new List<IPipeSpecification<SendContext>>();
            _messageSpecifications = new ConcurrentDictionary<Type, IMessageSendPipeSpecification>();
            _observers = new SendPipeSpecificationObservable();
        }

        public void AddPipeSpecification(IPipeSpecification<SendContext> specification)
        {
            lock (_lock)
            {
                _specifications.Add(specification);

                foreach (var messageSpecification in _messageSpecifications.Values)
                    messageSpecification.AddPipeSpecification(specification);
            }
        }

        void ISendPipeConfigurator.AddPipeSpecification<T>(IPipeSpecification<SendContext<T>> specification)
        {
            IMessageSendPipeSpecification<T> messageSpecification = GetMessageSpecification<T>();

            messageSpecification.AddPipeSpecification(specification);
        }

        public ConnectHandle ConnectSendPipeSpecificationObserver(ISendPipeSpecificationObserver observer)
        {
            return _observers.Connect(observer);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _specifications.SelectMany(x => x.Validate())
                .Concat(_messageSpecifications.Values.SelectMany(x => x.Validate()));
        }

        public IMessageSendPipeSpecification<T> GetMessageSpecification<T>()
            where T : class
        {
            var specification = _messageSpecifications.GetOrAdd(typeof(T), CreateMessageSpecification<T>);

            return specification.GetMessageSpecification<T>();
        }

        IMessageSendPipeSpecification CreateMessageSpecification<T>(Type type)
            where T : class
        {
            var specification = new MessageSendPipeSpecification<T>();

            lock (_lock)
            {
                foreach (IPipeSpecification<SendContext> pipeSpecification in _specifications)
                    specification.AddPipeSpecification(pipeSpecification);
            }

            _observers.MessageSpecificationCreated(specification);

            var connector = new ImplementedMessageTypeConnector<T>(this, specification);

            ImplementedMessageTypeCache<T>.EnumerateImplementedTypes(connector);

            return specification;
        }


        class ImplementedMessageTypeConnector<TMessage> :
            IImplementedMessageType
            where TMessage : class
        {
            readonly MessageSendPipeSpecification<TMessage> _messageSpecification;
            readonly ISendPipeSpecification _specification;

            public ImplementedMessageTypeConnector(ISendPipeSpecification specification, MessageSendPipeSpecification<TMessage> messageSpecification)
            {
                _specification = specification;
                _messageSpecification = messageSpecification;
            }

            public void ImplementsMessageType<T>(bool direct)
                where T : class
            {
                IMessageSendPipeSpecification<T> implementedTypeSpecification = _specification.GetMessageSpecification<T>();

                _messageSpecification.AddImplementedMessageSpecification(implementedTypeSpecification);
            }
        }
    }
}
