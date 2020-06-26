﻿namespace MassTransit.Azure.ServiceBus.Core.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using TestFramework.Messages;
    using Util;


    [TestFixture]
    public class Renewing_a_lock_on_an_existing_message :
        AzureServiceBusTestFixture
    {
        [Test, Explicit]
        public async Task Should_complete_the_consumer()
        {
            var context = await PingConsumer.Completed.Task;
        }

        public Renewing_a_lock_on_an_existing_message()
        {
            TestTimeout = TimeSpan.FromMinutes(30);
        }

        protected override void ConfigureServiceBusReceiveEndpoint(IServiceBusReceiveEndpointConfigurator configurator)
        {
            configurator.LockDuration = TimeSpan.FromMinutes(5);
            configurator.MaxAutoRenewDuration = TimeSpan.FromMinutes(20);

            configurator.Consumer<PingConsumer>();
        }

        public async Task Setup()
        {
            await InputQueueSendEndpoint.Send(new PingMessage());
        }


        class PingConsumer :
            IConsumer<PingMessage>
        {
            public static readonly TaskCompletionSource<PingMessage> Completed = TaskUtil.GetTask<PingMessage>();

            public async Task Consume(ConsumeContext<PingMessage> context)
            {
                Console.WriteLine($"Consumer Starting at {DateTime.Now} (redeliver: {context.ReceiveContext.Redelivered}");

                await Task.Delay(TimeSpan.FromMinutes(15), context.CancellationToken).ConfigureAwait(false);

                Console.WriteLine($"Consumer Completed at {DateTime.Now}");
                Completed.TrySetResult(context.Message);
            }
        }
    }
}
