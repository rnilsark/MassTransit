namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationTests.Cleaner;
    using IntegrationTests.Problem;
    using MassTransit;
    using Newtonsoft.Json;
    using NUnit.Framework;
    
    [TestFixture]
    public class Tests
    {
        [Test]
        public async Task Publishing_hierarchical_event()
        {
            int i = 0;
            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<IProblem_2_Part1>)(context =>
                        {
                            Console.WriteLine($"Consumer: {nameof(IProblem_2_Part1)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            Interlocked.Increment(ref i);
                            return Task.CompletedTask;
                        }));

                        endpointConfigurator.Handler((MessageHandler<IProblem_2_Part2>)(context =>
                        {
                            Console.WriteLine($"Consumer: {nameof(IProblem_2_Part2)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            return Task.CompletedTask;
                        }));
                    });
            });

            await consumer.StartAsync();

            var publisher = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });
            });

            await publisher.StartAsync();
            await publisher.Publish(new TheProblemEvent());
            
            await Task.Delay(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, i);
        }

        [Test]
        public async Task Publishing_hierarchical_event_cleaner()
        {
            int i = 0;
            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event_cleaner),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.EnableDuplicateDetection(TimeSpan.FromSeconds(30));

                        endpointConfigurator.Handler((MessageHandler<IPart1>)(context =>
                        {
                            Console.WriteLine($"Consumer: {nameof(IPart1)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            Interlocked.Increment(ref i);
                            return Task.CompletedTask;
                        }));

                        endpointConfigurator.Handler((MessageHandler<IPart2>)(context =>
                        {
                            Console.WriteLine($"Consumer: {nameof(IPart2)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            return Task.CompletedTask;
                        }));
                    });
            });

            await consumer.StartAsync();

            var publisher = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });
            });

            await publisher.StartAsync();
            await publisher.Publish(new TheCleanerEvent());

            await Task.Delay(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, i);
        }

        [Test]
        public async Task Publishing_hierarchical_event_deploying_topology_first()
        {
            var producerBootstrapper = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.DeployTopologyOnly = true;

                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });
            });

            var consumerBootstrapper = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.DeployTopologyOnly = true;

                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event_deploying_topology_first),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<IProblem_2_Part1>)(context => Task.CompletedTask));
                    });
            });

            await consumerBootstrapper.StartAsync();
            await consumerBootstrapper.StopAsync();

            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event_deploying_topology_first),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<IProblem_2_Part1>)(context =>
                        {
                            Console.WriteLine($"Consumer: {nameof(IProblem_2_Part1)} Message: {JsonConvert.SerializeObject(context.Message)}");
                            return Task.CompletedTask;
                        }));
                    });
            });

            await consumer.StartAsync();

            await producerBootstrapper.StartAsync();
            await producerBootstrapper.StopAsync();

            var publisher = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });
            });

            await publisher.StartAsync();
            await publisher.Publish(new TheProblemEvent());
            await publisher.Publish(new TheProblemEvent());

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task Publishing_hierarchical_event_from_multiple_producers()
        {
            var consumer = Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_hierarchical_event_from_multiple_producers),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<IProblem_2_Part1>)(context =>
                        {
                            Console.WriteLine(
                                $"Consumer: {nameof(IProblem_2_Part1)} Message: {JsonConvert.SerializeObject(context.Message)}");
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
                        configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                        {
                        });
                    });

                    await publisher.StartAsync();
                    await publisher.Publish(new TheProblemEvent());

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
                configurator.Host(TestConstants.BusConnectionString, hostConfigurator =>
                {
                });

                configurator.ReceiveEndpoint(nameof(Publishing_flat_event_from_multiple_producers),
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Handler((MessageHandler<FlatEvent>)(context =>
                        {
                            Console.WriteLine(
                                $"Consumer: {nameof(FlatEvent)} Message: {JsonConvert.SerializeObject(context.Message)}");
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

        public class FlatEvent
        {
            public string P1 { get; } = "Flat";
        }
    }
}
