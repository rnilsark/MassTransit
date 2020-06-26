namespace MassTransit.TestFramework
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Testing;
    using Util;


    public class InMemoryTestFixture :
        BusTestFixture
    {
        readonly IBusCreationScope _busCreationScope;

        public InMemoryTestFixture(bool busPerTest = false)
            : this(new InMemoryTestHarness(), busPerTest)
        {
        }

        public InMemoryTestFixture(InMemoryTestHarness harness, bool busPerTest = false)
            : base(harness)
        {
            InMemoryTestHarness = harness;

            if (busPerTest)
                _busCreationScope = new PerTestBusCreationScope(SetupBus, TeardownBus);
            else
                _busCreationScope = new PerTestFixtureBusCreationScope(SetupBus, TeardownBus);

            InMemoryTestHarness.OnConfigureInMemoryBus += ConfigureInMemoryBus;
            InMemoryTestHarness.OnConfigureInMemoryReceiveEndpoint += ConfigureInMemoryReceiveEndpoint;
        }

        protected InMemoryTestHarness InMemoryTestHarness { get; }

        protected string InputQueueName => InMemoryTestHarness.InputQueueName;

        protected Uri BaseAddress => InMemoryTestHarness.BaseAddress;

        /// <summary>
        /// The sending endpoint for the InputQueue
        /// </summary>
        protected ISendEndpoint InputQueueSendEndpoint => InMemoryTestHarness.InputQueueSendEndpoint;

        /// <summary>
        /// The sending endpoint for the Bus
        /// </summary>
        protected ISendEndpoint BusSendEndpoint => InMemoryTestHarness.BusSendEndpoint;

        protected Uri BusAddress => InMemoryTestHarness.BusAddress;

        protected Uri InputQueueAddress => InMemoryTestHarness.InputQueueAddress;

        [SetUp]
        public Task SetupInMemoryTest()
        {
            return _busCreationScope.TestSetup();
        }

        [TearDown]
        public Task TearDownInMemoryTest()
        {
            return _busCreationScope.TestTeardown();
        }

        protected IRequestClient<TRequest> CreateRequestClient<TRequest>()
            where TRequest : class
        {
            return InMemoryTestHarness.CreateRequestClient<TRequest>();
        }

        protected IRequestClient<TRequest> CreateRequestClient<TRequest>(Uri destinationAddress)
            where TRequest : class
        {
            return InMemoryTestHarness.CreateRequestClient<TRequest>(destinationAddress);
        }

        protected Task<IRequestClient<TRequest>> ConnectRequestClient<TRequest>()
            where TRequest : class
        {
            return InMemoryTestHarness.ConnectRequestClient<TRequest>();
        }

        [OneTimeSetUp]
        public Task SetupInMemoryTestFixture()
        {
            return _busCreationScope.TestFixtureSetup();
        }

        Task SetupBus()
        {
            return InMemoryTestHarness.Start();
        }

        protected Task<ISendEndpoint> GetSendEndpoint(Uri address)
        {
            return InMemoryTestHarness.GetSendEndpoint(address);
        }

        [OneTimeTearDown]
        public async Task TearDownInMemoryTestFixture()
        {
            await _busCreationScope.TestFixtureTeardown().ConfigureAwait(false);

            InMemoryTestHarness.Dispose();
        }

        Task TeardownBus()
        {
            return InMemoryTestHarness.Stop();
        }

        protected virtual void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
        }

        protected virtual void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
        }


        interface IBusCreationScope
        {
            Task TestFixtureSetup();
            Task TestSetup();
            Task TestTeardown();
            Task TestFixtureTeardown();
        }


        class PerTestFixtureBusCreationScope :
            IBusCreationScope
        {
            readonly Func<Task> _setupBus;
            readonly Func<Task> _teardownBus;

            public PerTestFixtureBusCreationScope(Func<Task> setupBus, Func<Task> teardownBus)
            {
                _setupBus = setupBus;
                _teardownBus = teardownBus;
            }

            public Task TestFixtureSetup()
            {
                return _setupBus();
            }

            public Task TestSetup()
            {
                return TaskUtil.Completed;
            }

            public Task TestTeardown()
            {
                return TaskUtil.Completed;
            }

            public Task TestFixtureTeardown()
            {
                return _teardownBus();
            }
        }


        class PerTestBusCreationScope :
            IBusCreationScope
        {
            readonly Func<Task> _setupBus;
            readonly Func<Task> _teardownBus;

            public PerTestBusCreationScope(Func<Task> setupBus, Func<Task> teardownBus)
            {
                _setupBus = setupBus;
                _teardownBus = teardownBus;
            }

            public Task TestFixtureSetup()
            {
                return TaskUtil.Completed;
            }

            public Task TestSetup()
            {
                return _setupBus();
            }

            public Task TestTeardown()
            {
                return _teardownBus();
            }

            public Task TestFixtureTeardown()
            {
                return TaskUtil.Completed;
            }
        }
    }
}
