﻿namespace MassTransit.AmazonSqsTransport.Transport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Contexts;
    using Exceptions;
    using GreenPipes;
    using GreenPipes.Agents;
    using GreenPipes.Internals.Extensions;
    using Policies;
    using Transports;


    public class ConnectionContextFactory :
        IPipeContextFactory<ConnectionContext>
    {
        readonly IRetryPolicy _connectionRetryPolicy;
        readonly IAmazonSqsHostConfiguration _hostConfiguration;

        public ConnectionContextFactory(IAmazonSqsHostConfiguration hostConfiguration)
        {
            _hostConfiguration = hostConfiguration;

            _connectionRetryPolicy = Retry.CreatePolicy(x =>
            {
                x.Handle<AmazonSqsConnectException>();

                x.Exponential(1000, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
            });
        }

        IPipeContextAgent<ConnectionContext> IPipeContextFactory<ConnectionContext>.CreateContext(ISupervisor supervisor)
        {
            Task<ConnectionContext> context = Task.Run(() => CreateConnection(supervisor), supervisor.Stopping);

            IPipeContextAgent<ConnectionContext> contextHandle = supervisor.AddContext(context);

            return contextHandle;
        }

        IActivePipeContextAgent<ConnectionContext> IPipeContextFactory<ConnectionContext>.CreateActiveContext(ISupervisor supervisor,
            PipeContextHandle<ConnectionContext> context, CancellationToken cancellationToken)
        {
            return supervisor.AddActiveContext(context, CreateSharedConnection(context.Context, cancellationToken));
        }

        static async Task<ConnectionContext> CreateSharedConnection(Task<ConnectionContext> context, CancellationToken cancellationToken)
        {
            return context.IsCompletedSuccessfully()
                ? new SharedConnectionContext(context.Result, cancellationToken)
                : new SharedConnectionContext(await context.OrCanceled(cancellationToken).ConfigureAwait(false), cancellationToken);
        }

        async Task<ConnectionContext> CreateConnection(ISupervisor supervisor)
        {
            return await _connectionRetryPolicy.Retry(async () =>
            {
                if (supervisor.Stopping.IsCancellationRequested)
                    throw new OperationCanceledException($"The connection is stopping and cannot be used: {_hostConfiguration.HostAddress}");

                IConnection connection = null;
                try
                {
                    TransportLogMessages.ConnectHost(_hostConfiguration.Settings.ToString());

                    connection = _hostConfiguration.Settings.CreateConnection();

                    return new AmazonSqsConnectionContext(connection, _hostConfiguration, supervisor.Stopped);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AmazonSqsConnectException("Connect failed: " + _hostConfiguration.Settings, ex);
                }
            }, supervisor.Stopping).ConfigureAwait(false);
        }
    }
}
