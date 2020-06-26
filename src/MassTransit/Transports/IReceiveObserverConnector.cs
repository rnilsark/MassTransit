namespace MassTransit.Transports
{
    using GreenPipes;


    public interface IReceiveObserverConnector
    {
        /// <summary>
        /// Connect an observer to the receiving endpoint
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        ConnectHandle ConnectReceiveObserver(IReceiveObserver observer);
    }
}
