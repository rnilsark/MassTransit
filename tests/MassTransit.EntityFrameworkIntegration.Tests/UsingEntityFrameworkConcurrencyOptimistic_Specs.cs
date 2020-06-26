﻿namespace MassTransit.EntityFrameworkIntegration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit.Saga;
    using NUnit.Framework;
    using Saga;
    using TestFramework;
    using Testing;


    [TestFixture]
    [Category("EntityFramework")]
    public class When_using_EntityFrameworkConcurrencyOptimistic :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_capture_all_events_many_sagas()
        {
            var tasks = new List<Task>();

            var sagaIds = new Guid[20];
            for (var i = 0; i < 20; i++)
            {
                var correlationId = NewId.NextGuid();

                await InputQueueSendEndpoint.Send(new RehersalBegins {CorrelationId = correlationId});

                sagaIds[i] = correlationId;
            }

            for (var i = 0; i < 20; i++)
            {
                Guid? sagaId = await _repository.Value.ShouldContainSaga(sagaIds[i], TestTimeout);
                Assert.IsTrue(sagaId.HasValue);
            }

            for (var i = 0; i < 20; i++)
            {
                tasks.Add(InputQueueSendEndpoint.Send(new Bass
                {
                    CorrelationId = sagaIds[i],
                    Name = "John"
                }));
                tasks.Add(InputQueueSendEndpoint.Send(new Baritone
                {
                    CorrelationId = sagaIds[i],
                    Name = "Mark"
                }));
                tasks.Add(InputQueueSendEndpoint.Send(new Tenor
                {
                    CorrelationId = sagaIds[i],
                    Name = "Anthony"
                }));
                tasks.Add(InputQueueSendEndpoint.Send(new Countertenor
                {
                    CorrelationId = sagaIds[i],
                    Name = "Tom"
                }));
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            foreach (var sid in sagaIds)
            {
                Guid? sagaId = await _repository.Value.ShouldContainSaga(x => x.CorrelationId == sid
                    && x.CurrentState == _machine.Harmony.Name, TestTimeout);

                Assert.IsTrue(sagaId.HasValue);
            }
        }

        [Test]
        public async Task Should_capture_all_events_single_saga()
        {
            var correlationId = Guid.NewGuid();

            await InputQueueSendEndpoint.Send(new RehersalBegins {CorrelationId = correlationId});

            Guid? sagaId = await _repository.Value.ShouldContainSaga(correlationId, TestTimeout);

            Assert.IsTrue(sagaId.HasValue);

            await Task.WhenAll(
                InputQueueSendEndpoint.Send(new Bass
                {
                    CorrelationId = correlationId,
                    Name = "John"
                }),
                InputQueueSendEndpoint.Send(new Baritone
                {
                    CorrelationId = correlationId,
                    Name = "Mark"
                }),
                InputQueueSendEndpoint.Send(new Tenor
                {
                    CorrelationId = correlationId,
                    Name = "Anthony"
                }),
                InputQueueSendEndpoint.Send(new Countertenor
                {
                    CorrelationId = correlationId,
                    Name = "Tom"
                })
            );

            sagaId = await _repository.Value.ShouldContainSaga(x => x.CorrelationId == correlationId
                && x.CurrentState == _machine.Harmony.Name, TestTimeout);

            Assert.IsTrue(sagaId.HasValue);

            var instance = await GetSaga(correlationId);

            Assert.IsTrue(instance.CurrentState.Equals("Harmony"));
        }

        ChoirStateMachine _machine;
        readonly ISagaDbContextFactory<ChoirStateOptimistic> _sagaDbContextFactory;
        readonly Lazy<ISagaRepository<ChoirStateOptimistic>> _repository;

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            _machine = new ChoirStateMachine();

            configurator.UseRetry(x =>
            {
                x.Handle<DbUpdateConcurrencyException>();
                x.Immediate(5);
            });
            configurator.StateMachineSaga(_machine, _repository.Value);
        }

        public When_using_EntityFrameworkConcurrencyOptimistic()
        {
            _sagaDbContextFactory = new DelegateSagaDbContextFactory<ChoirStateOptimistic>(() =>
                new ChoirStateOptimisticSagaDbContext(SagaDbContextFactoryProvider.GetLocalDbConnectionString()));

            _repository = new Lazy<ISagaRepository<ChoirStateOptimistic>>(() =>
                EntityFrameworkSagaRepository<ChoirStateOptimistic>.CreateOptimistic(_sagaDbContextFactory));
        }

        async Task<ChoirStateOptimistic> GetSaga(Guid id)
        {
            using (var dbContext = _sagaDbContextFactory.Create())
            {
                return await dbContext.Set<ChoirStateOptimistic>().SingleOrDefaultAsync(x => x.CorrelationId == id);
            }
        }

        protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
            base.ConfigureInMemoryBus(configurator);

            configurator.TransportConcurrencyLimit = 16;
        }
    }
}
