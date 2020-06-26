namespace MassTransit.Azure.ServiceBus.Core.Topology.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;


    public class QueueSubscriptionEntity :
        QueueSubscription,
        QueueSubscriptionHandle
    {
        readonly QueueEntity _queue;
        readonly SubscriptionEntity _subscription;
        readonly TopicEntity _topic;

        public QueueSubscriptionEntity(long id, long subscriptionId, TopicEntity topic, QueueEntity queue, SubscriptionDescription subscriptionDescription,
            RuleDescription rule = null, Filter filter = null)
        {
            Id = id;

            _topic = topic;
            _queue = queue;
            _subscription = new SubscriptionEntity(subscriptionId, topic, subscriptionDescription, rule, filter);
        }

        public static IEqualityComparer<QueueSubscriptionEntity> EntityComparer { get; } = new QueueSubscriptionEntityEqualityComparer();
        public static IEqualityComparer<QueueSubscriptionEntity> NameComparer { get; } = new NameEqualityComparer();

        public Topic Source => _topic.Topic;
        public Queue Destination => _queue.Queue;
        public Subscription Subscription => _subscription;

        public long Id { get; }
        public QueueSubscription QueueSubscription => this;

        public override string ToString()
        {
            return string.Join(", ",
                new[]
                {
                    $"topic: {_topic.TopicDescription.Path}",
                    $"queue: {_queue.QueueDescription.Path}",
                    $"subscription: {_subscription.SubscriptionDescription.SubscriptionName}"
                }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }


        sealed class QueueSubscriptionEntityEqualityComparer :
            IEqualityComparer<QueueSubscriptionEntity>
        {
            public bool Equals(QueueSubscriptionEntity x, QueueSubscriptionEntity y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                return TopicEntity.EntityComparer.Equals(x._topic, y._topic)
                    && QueueEntity.EntityComparer.Equals(x._queue, y._queue)
                    && SubscriptionEntity.EntityComparer.Equals(x._subscription, y._subscription);
            }

            public int GetHashCode(QueueSubscriptionEntity obj)
            {
                unchecked
                {
                    var hashCode = TopicEntity.EntityComparer.GetHashCode(obj._topic);
                    hashCode = (hashCode * 397) ^ QueueEntity.EntityComparer.GetHashCode(obj._queue);
                    hashCode = (hashCode * 397) ^ SubscriptionEntity.EntityComparer.GetHashCode(obj._subscription);

                    return hashCode;
                }
            }
        }


        sealed class NameEqualityComparer :
            IEqualityComparer<QueueSubscriptionEntity>
        {
            public bool Equals(QueueSubscriptionEntity x, QueueSubscriptionEntity y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                return string.Equals(x.Subscription.SubscriptionDescription.SubscriptionName, y.Subscription.SubscriptionDescription.SubscriptionName)
                    && string.Equals(x.Subscription.SubscriptionDescription.TopicPath, y.Subscription.SubscriptionDescription.TopicPath)
                    && string.Equals(x.Destination.QueueDescription.Path, y.Destination.QueueDescription.Path);
            }

            public int GetHashCode(QueueSubscriptionEntity obj)
            {
                var hashCode = obj.Subscription.SubscriptionDescription.SubscriptionName.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Subscription.SubscriptionDescription.TopicPath.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Destination.QueueDescription.Path.GetHashCode();

                return hashCode;
            }
        }
    }
}
