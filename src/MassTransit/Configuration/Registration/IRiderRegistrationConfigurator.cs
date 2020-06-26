namespace MassTransit.Registration
{
    using Riders;


    public interface IRiderRegistrationConfigurator :
        IRegistrationConfigurator
    {
        IContainerRegistrar Registrar { get; }

        void AddRegistration<T>(T registration)
            where T : class;

        /// <summary>
        /// Add the rider to the container, configured properly
        /// </summary>
        /// <param name="riderFactory"></param>
        void SetRiderFactory<TRider>(IRegistrationRiderFactory<TRider> riderFactory)
            where TRider : class, IRider;
    }
}
