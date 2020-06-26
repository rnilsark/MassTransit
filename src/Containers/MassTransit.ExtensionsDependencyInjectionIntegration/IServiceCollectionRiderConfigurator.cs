namespace MassTransit.ExtensionsDependencyInjectionIntegration
{
    using MassTransit.Registration;
    using Microsoft.Extensions.DependencyInjection;


    public interface IServiceCollectionRiderConfigurator :
        IRiderRegistrationConfigurator
    {
        IServiceCollection Collection { get; }
    }
}
