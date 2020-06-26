namespace MassTransit.Containers.Tests.Autofac_Tests
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Common_Tests;
    using NUnit.Framework;


    [TestFixture]
    public class Autofac_ScopePublish :
        Common_ScopePublish<ILifetimeScope>
    {
        readonly IContainer _container;
        readonly ILifetimeScope _childContainer;

        public Autofac_ScopePublish()
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransit(x =>
            {
                x.AddBus(provider => BusControl);
            });

            _container = builder.Build();
            _childContainer = _container.BeginLifetimeScope();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _childContainer.DisposeAsync();
            await _container.DisposeAsync();
        }

        protected override IPublishEndpoint GetPublishEndpoint()
        {
            return _childContainer.Resolve<IPublishEndpoint>();
        }

        protected override void AssertScopesAreEqual(ILifetimeScope actual)
        {
            Assert.AreEqual(_childContainer, actual);
        }
    }


    [TestFixture]
    public class Autofac_Publish_Filter :
        Common_Publish_Filter
    {
        readonly IContainer _container;
        readonly ILifetimeScope _scope;

        public Autofac_Publish_Filter()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new MyId(Guid.NewGuid())).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(ScopedFilter<>)).InstancePerLifetimeScope();
            builder.RegisterInstance(TaskCompletionSource);

            builder.AddMassTransit(ConfigureRegistration);

            _container = builder.Build();
            _scope = _container.BeginLifetimeScope();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _scope.DisposeAsync();
            await _container.DisposeAsync();
        }

        protected override void ConfigureFilter(IPublishPipelineConfigurator configurator)
        {
            AutofacFilterExtensions.UsePublishFilter(configurator, typeof(ScopedFilter<>), Registration);
        }

        protected override IBusRegistrationContext Registration => _container.Resolve<IBusRegistrationContext>();
        protected override MyId MyId => _scope.Resolve<MyId>();
        protected override IPublishEndpoint PublishEndpoint => _scope.Resolve<IPublishEndpoint>();
    }
}
