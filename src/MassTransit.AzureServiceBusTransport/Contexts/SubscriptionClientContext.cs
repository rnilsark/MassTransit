// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.AzureServiceBusTransport.Contexts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes;
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using Transport;


    public class SubscriptionClientContext :
        BasePipeContext,
        ClientContext,
        IAsyncDisposable
    {
        static readonly ILog _log = Logger.Get<QueueClientContext>();
        readonly SubscriptionClient _client;
        readonly ClientSettings _settings;

        public SubscriptionClientContext(SubscriptionClient client, Uri inputAddress, ClientSettings settings)
        {
            _client = client;
            _settings = settings;
            InputAddress = inputAddress;
        }

        public string EntityPath => _client.TopicPath;
        public Uri InputAddress { get; }

        public Task RegisterSessionHandlerFactoryAsync(IMessageSessionAsyncHandlerFactory factory, EventHandler<ExceptionReceivedEventArgs> exceptionHandler)
        {
            return _client.RegisterSessionHandlerFactoryAsync(factory, _settings.GetSessionHandlerOptions(exceptionHandler));
        }

        public void OnMessageAsync(Func<BrokeredMessage, Task> callback, EventHandler<ExceptionReceivedEventArgs> exceptionHandler)
        {
            _client.OnMessageAsync(callback, _settings.GetOnMessageOptions(exceptionHandler));
        }

        async Task IAsyncDisposable.DisposeAsync(CancellationToken cancellationToken)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Closing client: {0}", InputAddress);

            try
            {
                if (_client != null && !_client.IsClosed)
                    await _client.CloseAsync().ConfigureAwait(false);

                if (_log.IsDebugEnabled)
                    _log.DebugFormat("Closed client: {0}", InputAddress);
            }
            catch (Exception exception)
            {
                if (_log.IsWarnEnabled)
                    _log.Warn($"Exception closing the client: {InputAddress}", exception);
            }
        }
    }
}