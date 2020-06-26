namespace MassTransit.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConsumeConfigurators;
    using Monitoring.Health;
    using Riders;
    using Saga;


    public class RiderRegistrationContext :
        IRiderRegistrationContext
    {
        readonly BusHealth _health;
        readonly IRegistration _registration;
        readonly IRegistrationCache<object> _registrations;

        public RiderRegistrationContext(IRegistration registration, BusHealth health, IRegistrationCache<object> registrations)
        {
            _registration = registration;
            _health = health;
            _registrations = registrations;
        }

        public IEnumerable<T> GetRegistrations<T>()
        {
            return _registrations.Values.OfType<T>();
        }

        public void UseHealthCheck(IRiderFactoryConfigurator configurator)
        {
            configurator.ConnectReceiveEndpointObserver(_health);
        }

        public object GetService(Type serviceType)
        {
            return _registration.GetService(serviceType);
        }

        public T GetRequiredService<T>()
            where T : class
        {
            return _registration.GetRequiredService<T>();
        }

        public T GetService<T>()
            where T : class
        {
            return _registration.GetService<T>();
        }

        public void ConfigureConsumer(Type consumerType, IReceiveEndpointConfigurator configurator)
        {
            _registration.ConfigureConsumer(consumerType, configurator);
        }

        public void ConfigureConsumer<T>(IReceiveEndpointConfigurator configurator, Action<IConsumerConfigurator<T>> configure = null)
            where T : class, IConsumer
        {
            _registration.ConfigureConsumer(configurator, configure);
        }

        public void ConfigureConsumers(IReceiveEndpointConfigurator configurator)
        {
            _registration.ConfigureConsumers(configurator);
        }

        public void ConfigureSaga(Type sagaType, IReceiveEndpointConfigurator configurator)
        {
            _registration.ConfigureSaga(sagaType, configurator);
        }

        public void ConfigureSaga<T>(IReceiveEndpointConfigurator configurator, Action<ISagaConfigurator<T>> configure = null)
            where T : class, ISaga
        {
            _registration.ConfigureSaga(configurator, configure);
        }

        public void ConfigureSagas(IReceiveEndpointConfigurator configurator)
        {
            _registration.ConfigureSagas(configurator);
        }

        public void ConfigureExecuteActivity(Type activityType, IReceiveEndpointConfigurator configurator)
        {
            _registration.ConfigureExecuteActivity(activityType, configurator);
        }

        public void ConfigureActivity(Type activityType, IReceiveEndpointConfigurator executeEndpointConfigurator,
            IReceiveEndpointConfigurator compensateEndpointConfigurator)
        {
            _registration.ConfigureActivity(activityType, executeEndpointConfigurator, compensateEndpointConfigurator);
        }

        public void ConfigureActivityExecute(Type activityType, IReceiveEndpointConfigurator executeEndpointConfigurator, Uri compensateAddress)
        {
            _registration.ConfigureActivityExecute(activityType, executeEndpointConfigurator, compensateAddress);
        }

        public void ConfigureActivityCompensate(Type activityType, IReceiveEndpointConfigurator compensateEndpointConfigurator)
        {
            _registration.ConfigureActivityCompensate(activityType, compensateEndpointConfigurator);
        }
    }
}
