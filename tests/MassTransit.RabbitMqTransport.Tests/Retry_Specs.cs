namespace MassTransit.RabbitMqTransport.Tests
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit.Testing;
    using MassTransit.Testing.Indicators;
    using NUnit.Framework;
    using TestFramework;
    using TestFramework.Messages;


    [TestFixture]
    public class When_specifying_retry_limit :
        RabbitMqTestFixture
    {
        [Test]
        public async Task Should_stop_after_limit_exceeded()
        {
            await Bus.Publish(new PingMessage());

            ConsumeContext<Fault<PingMessage>> handled = await _handled;

            Assert.That(handled.Headers.Get<int>(MessageHeaders.FaultRetryCount), Is.GreaterThan(0));

            await _activityMonitor.AwaitBusInactivity(TestCancellationToken);

            Assert.LessOrEqual(_attempts, _limit + 1);
        }

        readonly int _limit;
        int _attempts;
        IBusActivityMonitor _activityMonitor;
        Task<ConsumeContext<Fault<PingMessage>>> _handled;

        public When_specifying_retry_limit()
        {
            _limit = 2;
            _attempts = 0;
        }

        protected override void ConfigureRabbitMqBus(IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.ReceiveEndpoint(e =>
            {
                _handled = Handled<Fault<PingMessage>>(e);
            });
        }

        protected override void ConfigureRabbitMqReceiveEndpoint(IRabbitMqReceiveEndpointConfigurator configurator)
        {
            var sec2 = TimeSpan.FromSeconds(2);
            configurator.UseRetry(x => x.Exponential(_limit, sec2, sec2, sec2));

            configurator.Consumer(() => new Consumer(ref _attempts));
        }

        protected override void ConnectObservers(IBus bus)
        {
            base.ConnectObservers(bus);

            _activityMonitor = bus.CreateBusActivityMonitor(TimeSpan.FromMilliseconds(500));
        }


        class Consumer :
            IConsumer<PingMessage>
        {
            public Consumer(ref int attempts)
            {
                ++attempts;
            }

            public Task Consume(ConsumeContext<PingMessage> context)
            {
                throw new IntentionalTestException();
            }
        }
    }


    [TestFixture]
    public class When_specifying_redelivery_limit :
        RabbitMqTestFixture
    {
        [Test]
        public async Task Should_stop_after_limit_exceeded()
        {
            await Bus.Publish(new PingMessage());

            ConsumeContext<Fault<PingMessage>> handled = await _handled;

            Assert.That(handled.Headers.Get<int>(MessageHeaders.FaultRetryCount), Is.GreaterThan(0));

            await _activityMonitor.AwaitBusInactivity(TestCancellationToken);

            Assert.LessOrEqual(_attempts, _limit + 1);
        }

        readonly int _limit;
        int _attempts;
        IBusActivityMonitor _activityMonitor;
        Task<ConsumeContext<Fault<PingMessage>>> _handled;

        public When_specifying_redelivery_limit()
        {
            _limit = 3;
            _attempts = 0;
        }

        protected override void ConfigureRabbitMqBus(IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.UseDelayedExchangeMessageScheduler();

            configurator.ReceiveEndpoint(e =>
            {
                _handled = Handled<Fault<PingMessage>>(e);
            });
        }

        protected override void ConfigureRabbitMqReceiveEndpoint(IRabbitMqReceiveEndpointConfigurator configurator)
        {
            var two = TimeSpan.FromSeconds(2);
            configurator.UseScheduledRedelivery(x => x.Intervals(two, two, two));

            configurator.Consumer(() => new Consumer(ref _attempts));
        }

        protected override void ConnectObservers(IBus bus)
        {
            base.ConnectObservers(bus);

            _activityMonitor = bus.CreateBusActivityMonitor(TimeSpan.FromMilliseconds(500));
        }


        class Consumer :
            IConsumer<PingMessage>
        {
            public Consumer(ref int attempts)
            {
                ++attempts;
            }

            public Task Consume(ConsumeContext<PingMessage> context)
            {
                throw new IntentionalTestException();
            }
        }
    }


    [TestFixture]
    public class When_specifying_redelivery_limit_with_message_ttl :
        RabbitMqTestFixture
    {
        [Test]
        public async Task Should_stop_after_limit_exceeded()
        {
            await Bus.Publish(new PingMessage(), x => x.TimeToLive = TimeSpan.FromSeconds(2));

            ConsumeContext<Fault<PingMessage>> handled = await _handled;

            Assert.That(handled.Headers.Get<int>(MessageHeaders.FaultRetryCount), Is.GreaterThan(0));

            await _activityMonitor.AwaitBusInactivity(TestCancellationToken);

            Assert.LessOrEqual(_attempts, _limit + 1);
        }

        readonly int _limit;
        int _attempts;
        IBusActivityMonitor _activityMonitor;
        Task<ConsumeContext<Fault<PingMessage>>> _handled;

        public When_specifying_redelivery_limit_with_message_ttl()
        {
            _limit = 3;
            _attempts = 0;
        }

        protected override void ConfigureRabbitMqBus(IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.UseDelayedExchangeMessageScheduler();

            configurator.ReceiveEndpoint(e =>
            {
                _handled = Handled<Fault<PingMessage>>(e);
            });
        }

        protected override void ConfigureRabbitMqReceiveEndpoint(IRabbitMqReceiveEndpointConfigurator configurator)
        {
            var two = TimeSpan.FromSeconds(2);
            configurator.UseScheduledRedelivery(x => x.Intervals(two, two, two));

            configurator.Consumer(() => new Consumer(ref _attempts));
        }

        protected override void ConnectObservers(IBus bus)
        {
            base.ConnectObservers(bus);

            _activityMonitor = bus.CreateBusActivityMonitor(TimeSpan.FromMilliseconds(500));
        }


        class Consumer :
            IConsumer<PingMessage>
        {
            public Consumer(ref int attempts)
            {
                ++attempts;
            }

            public Task Consume(ConsumeContext<PingMessage> context)
            {
                throw new IntentionalTestException();
            }
        }
    }
}
