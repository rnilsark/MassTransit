namespace MassTransit.Containers.Tests.StructureMap_Tests
{
    using Common_Tests;
    using NUnit.Framework;
    using StructureMap;


    public class StructureMap_Conductor :
        Common_Conductor
    {
        readonly IContainer _container;

        public StructureMap_Conductor(bool instanceEndpoint)
            : base(instanceEndpoint)
        {
            _container = new Container(expression => expression.AddMassTransit(ConfigureRegistration));
        }

        [OneTimeTearDown]
        public void Close_container()
        {
            _container.Dispose();
        }

        protected override void ConfigureServiceEndpoints(IBusFactoryConfigurator<IInMemoryReceiveEndpointConfigurator> configurator)
        {
            configurator.ConfigureServiceEndpoints(_container.GetInstance<IBusRegistrationContext>(), Options);
        }

        protected override IClientFactory GetClientFactory()
        {
            return _container.GetInstance<IClientFactory>();
        }
    }
}
