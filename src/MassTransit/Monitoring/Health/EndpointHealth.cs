namespace MassTransit.Monitoring.Health
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EndpointConfigurators;
    using Registration;
    using Util;


    public class EndpointHealth :
        IReceiveEndpointObserver,
        IEndpointConfigurationObserver,
        IEndpointHealth
    {
        readonly ConcurrentDictionary<Uri, EndpointStatus> _endpoints;

        public EndpointHealth()
        {
            _endpoints = new ConcurrentDictionary<Uri, EndpointStatus>();
        }

        public void EndpointConfigured<T>(T configurator)
            where T : IReceiveEndpointConfigurator
        {
            GetEndpoint(configurator.InputAddress);
        }

        public HealthResult CheckHealth()
        {
            EndpointStatus[] faulted = _endpoints.Values.Where(x => !x.Ready).ToArray();
            Dictionary<string, object> data = _endpoints.ToDictionary(x => x.Key.ToString(), x => x.Value.GetData());

            HealthResult healthCheckResult;
            if (faulted.Any())
            {
                healthCheckResult = HealthResult.Unhealthy($"Unhealthy Endpoints: {string.Join(",", faulted.Select(x => x.InputAddress.GetLastPart()))}",
                    faulted.Select(x => x.LastException).FirstOrDefault(e => e != null), data);
            }
            else
                healthCheckResult = HealthResult.Healthy("Endpoints are healthy", data);

            return healthCheckResult;
        }

        public Task Ready(ReceiveEndpointReady ready)
        {
            GetEndpoint(ready.InputAddress).OnReady(ready);

            return TaskUtil.Completed;
        }

        public Task Stopping(ReceiveEndpointStopping stopping)
        {
            GetEndpoint(stopping.InputAddress).OnStopping();

            return TaskUtil.Completed;
        }

        public Task Completed(ReceiveEndpointCompleted completed)
        {
            GetEndpoint(completed.InputAddress).OnCompleted(completed);

            return TaskUtil.Completed;
        }

        public Task Faulted(ReceiveEndpointFaulted faulted)
        {
            GetEndpoint(faulted.InputAddress).OnFaulted(faulted);

            return TaskUtil.Completed;
        }

        EndpointStatus GetEndpoint(Uri inputAddress)
        {
            return _endpoints.GetOrAdd(inputAddress, address => new EndpointStatus(address));
        }


        class EndpointStatus
        {
            public EndpointStatus(Uri inputAddress)
            {
                InputAddress = inputAddress;
            }

            public Uri InputAddress { get; }
            public bool Ready { get; set; }
            public string Message { get; set; } = "not started";
            public Exception LastException { get; set; }

            public void OnReady(ReceiveEndpointReady ready)
            {
                Ready = true;
                Message = ready.IsStarted ? "ready" : "ready (not started)";
            }

            public void OnStopping()
            {
                Ready = true;
                Message = "stopping";
            }

            public void OnCompleted(ReceiveEndpointCompleted completed)
            {
                Ready = false;
                Message = $"stopped (delivered {completed.DeliveryCount} messages)";
            }

            public void OnFaulted(ReceiveEndpointFaulted faulted)
            {
                Ready = false;
                LastException = faulted.Exception;
                Message = $"faulted ({faulted.Exception.Message})";
            }

            public object GetData()
            {
                if (LastException == null)
                    return new {Message};
                return new
                {
                    Message,
                    LastException
                };
            }
        }
    }
}
