namespace MassTransit.Containers.Tests.Autofac_Tests
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Common_Tests;
    using NUnit.Framework;
    using Scenarios;


    [TestFixture]
    public class Autofac_Consumer :
        Common_Consumer
    {
        readonly IContainer _container;

        public Autofac_Consumer()
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransit(x =>
            {
                x.AddConsumer<SimpleConsumer>();

                x.AddBus(provider => BusControl);
            });

            builder.RegisterType<SimpleConsumerDependency>()
                .As<ISimpleConsumerDependency>();

            builder.RegisterType<AnotherMessageConsumerImpl>()
                .As<AnotherMessageConsumer>();

            _container = builder.Build();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _container.DisposeAsync();
        }

        protected override IBusRegistrationContext Registration => _container.Resolve<IBusRegistrationContext>();
    }


    [TestFixture]
    public class Autofac_Consumer_Endpoint :
        Common_Consumer_Endpoint
    {
        readonly IContainer _container;

        public Autofac_Consumer_Endpoint()
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransit(x =>
            {
                x.AddConsumer<SimplerConsumer>()
                    .Endpoint(e => e.Name = "custom-endpoint-name");

                x.AddBus(provider => BusControl);
            });

            _container = builder.Build();
        }

        [OneTimeTearDown]
        public void Close_container()
        {
            _container.Dispose();
        }

        protected override IBusRegistrationContext Registration => _container.Resolve<IBusRegistrationContext>();
    }


    [TestFixture]
    public class Autofac_Consumer_ServiceEndpoint :
        Common_Consumer_ServiceEndpoint
    {
        readonly IContainer _container;

        public Autofac_Consumer_ServiceEndpoint()
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransit(x =>
            {
                x.AddConsumer<PingRequestConsumer>();

                x.AddBus(provider => BusControl);
            });

            _container = builder.Build();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _container.DisposeAsync();
        }

        protected override IBusRegistrationContext Registration => _container.Resolve<IBusRegistrationContext>();
    }


    [TestFixture]
    public class Autofac_Consume_Filter :
        Common_Consume_Filter
    {
        readonly IContainer _container;

        public Autofac_Consume_Filter()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new MyId(Guid.NewGuid())).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(ScopedFilter<>)).InstancePerLifetimeScope();
            builder.RegisterInstance(TaskCompletionSource);
            builder.AddMassTransit(ConfigureRegistration);
            _container = builder.Build();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _container.DisposeAsync();
        }

        protected override void ConfigureFilter(IConsumePipeConfigurator configurator)
        {
            AutofacFilterExtensions.UseConsumeFilter(configurator, typeof(ScopedFilter<>), Registration);
        }

        protected override IBusRegistrationContext Registration => _container.Resolve<IBusRegistrationContext>();
    }
}
