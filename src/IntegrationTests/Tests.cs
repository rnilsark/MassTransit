namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Azure.ServiceBus.Core;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Microsoft.ApplicationInsights;

    [TestFixture]
    public class Tests
    {
        [Test]
        public async Task Publishing_hierarchical_event()
        {
            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                ConfigureTelemetry(configurator);

                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event_from_multiple_producers),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<IBasiestEventInterface>)(context =>
                        {
                            Console.WriteLine($"Consumer: {nameof(IBasiestEventInterface)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            return Task.CompletedTask;
                        }));
                    });
            });

            await consumer.StartAsync();

            var publisher = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                ConfigureTelemetry(configurator);

                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });
            });

            await publisher.StartAsync();
            await publisher.Publish(new TheEvent());
            await publisher.Publish(new TheEvent());
            
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task Publishing_hierarchical_event_from_multiple_producers()
        {
            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                ConfigureTelemetry(configurator);

                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event_from_multiple_producers),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<IBasiestEventInterface>)(context =>
                        {
                            Console.WriteLine(
                                $"Consumer: {nameof(IBasiestEventInterface)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            return Task.CompletedTask;
                        }));
                    });
            });

            await consumer.StartAsync();

            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var publisher = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
                    {
                        ConfigureTelemetry(configurator);

                        configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                        {
                        });
                    });

                    await publisher.StartAsync();
                    await publisher.Publish(new TheEvent());

                }));
            }

            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task Publishing_flat_event_from_multiple_producers()
        { 
            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                ConfigureTelemetry(configurator);

                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_flat_event_from_multiple_producers),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<FlatEvent>)(context =>
                        {
                            Console.WriteLine(
                                $"Consumer: {nameof(IBasiestEventInterface)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            return Task.CompletedTask;
                        }));
                    });
            });

            await consumer.StartAsync();

            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var publisher = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
                    {
                        ConfigureTelemetry(configurator);

                        configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                        {
                        });
                    });

                    await publisher.StartAsync();
                    await publisher.Publish(new FlatEvent());
                }));
            }

            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        static void ConfigureTelemetry(IServiceBusBusFactoryConfigurator configurator)
        {
            var telemetryClient = new TelemetryClient();

            configurator.UseApplicationInsightsOnSend(telemetryClient);
            configurator.UseApplicationInsightsOnConsume(telemetryClient);
            configurator.UseApplicationInsightsOnPublish(telemetryClient);
        }


        public class TheEvent : IBaseEventInterface
        {
            public string P1 { get; } = "A";
            public string P2 { get; } = "B";
            public string P3 { get; } = "C";
        }

        public class FlatEvent
        {
            public string P1 { get; } = "Flat";
        }
    }
}
