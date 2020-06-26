namespace MassTransit.KafkaIntegration.Tests
{
    using System;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Context;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using Serializers;
    using TestFramework;


    public class Receive_Specs :
        InMemoryTestFixture
    {
        const string Topic = "test";

        [Test]
        public async Task Should_receive()
        {
            TaskCompletionSource<ConsumeContext<KafkaMessage>> taskCompletionSource = GetTask<ConsumeContext<KafkaMessage>>();
            var services = new ServiceCollection();
            services.AddSingleton(taskCompletionSource);

            services.TryAddSingleton<ILoggerFactory>(LoggerFactory);
            services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                x.AddRider(rider =>
                {
                    rider.AddConsumer<KafkaMessageConsumer>();

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host("localhost:9092");

                        k.TopicEndpoint<KafkaMessage>(Topic, nameof(Receive_Specs), c =>
                        {
                            c.AutoOffsetReset = AutoOffsetReset.Earliest;
                            c.ConfigureConsumer<KafkaMessageConsumer>(context);
                        });
                    });
                });
            });

            var provider = services.BuildServiceProvider();

            var busControl = provider.GetRequiredService<IBusControl>();

            await busControl.StartAsync(TestCancellationToken);

            try
            {
                var config = new ProducerConfig {BootstrapServers = "localhost:9092"};

                using IProducer<Null, KafkaMessage> p = new ProducerBuilder<Null, KafkaMessage>(config)
                    .SetValueSerializer(new MassTransitJsonSerializer<KafkaMessage>())
                    .Build();

                var kafkaMessage = new KafkaMessageClass("test");
                var sendContext = new MessageSendContext<KafkaMessage>(kafkaMessage);
                var message = new Message<Null, KafkaMessage>
                {
                    Value = kafkaMessage,
                    Headers = DictionaryHeadersSerialize.Serializer.Serialize(sendContext)
                };

                await p.ProduceAsync(Topic, message);

                ConsumeContext<KafkaMessage> result = await taskCompletionSource.Task;

                Assert.AreEqual(message.Value.Text, result.Message.Text);
                Assert.AreEqual(sendContext.MessageId, result.MessageId);
                Assert.That(result.DestinationAddress, Is.EqualTo(new Uri($"loopback://localhost/{KafkaTopicAddress.PathPrefix}/{Topic}")));
            }
            finally
            {
                await busControl.StopAsync(TestCancellationToken);

                await provider.DisposeAsync();
            }
        }


        class KafkaMessageClass :
            KafkaMessage
        {
            public KafkaMessageClass(string text)
            {
                Text = text;
            }

            public string Text { get; }
        }


        class KafkaMessageConsumer :
            IConsumer<KafkaMessage>
        {
            readonly TaskCompletionSource<ConsumeContext<KafkaMessage>> _taskCompletionSource;

            public KafkaMessageConsumer(TaskCompletionSource<ConsumeContext<KafkaMessage>> taskCompletionSource)
            {
                _taskCompletionSource = taskCompletionSource;
            }

            public async Task Consume(ConsumeContext<KafkaMessage> context)
            {
                _taskCompletionSource.TrySetResult(context);
            }
        }


        public interface KafkaMessage
        {
            string Text { get; }
        }
    }
}
