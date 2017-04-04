﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Platform.Core;
using GTRevo.Platform.Events;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.EventSourcing
{
    public class EventSourcedRepositoryTests
    {
        private readonly IEventStore eventStore;
        private readonly IEventQueue eventQueue;
        private readonly IActorContext actorContext;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IClock clock;

        private DateTime now = DateTime.Now;
        private Guid entityId = Guid.NewGuid();
        private Guid entity2Id = Guid.NewGuid();
        private Guid entityClassId = Guid.NewGuid();
        private Guid entity2ClassId = Guid.NewGuid();

        private readonly IEventSourcedRepository sut;

        public EventSourcedRepositoryTests()
        {
            eventStore = Substitute.For<IEventStore>();
            eventQueue = Substitute.For<IEventQueue>();
            actorContext = Substitute.For<IActorContext>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            actorContext = Substitute.For<IActorContext>();
            actorContext.CurrentActorName.Returns("actor");
            clock = Substitute.For<IClock>();
            clock.Now.Returns(now);

            eventStore.GetLastStateAsync(entity2Id)
                .Returns(new AggregateState(1, new List<DomainAggregateEvent>()
                {
                  new SetFooEvent()
                  {
                      AggregateId = entity2Id,
                      AggregateClassId = entity2ClassId
                  }
                }));

            entityTypeManager.GetClrTypeByClassId(entityClassId)
                .Returns(typeof(MyEntity));
            entityTypeManager.GetClrTypeByClassId(entity2ClassId)
                .Returns(typeof(MyEntity2));

            sut = new EventSourcedRepository(eventStore, eventQueue, actorContext,
                clock, entityTypeManager);
        }

        [Fact]
        public async Task GetAsync_ReturnsCorrectAggregate()
        {
            var entity = await sut.GetAsync<MyEntity2>(entity2Id);

            Assert.Equal(entity2Id, entity.Id);
            Assert.Equal(entity2ClassId, entity.ClassId);

            Assert.Equal(1, entity.LoadedEvents.Count);
            Assert.IsType<SetFooEvent>(entity.LoadedEvents[0]);
            Assert.Equal(1, entity.Version);
        }

        [Fact]
        public async Task GetAsync_CachesReturnedEntities()
        {
            var entity1 = await sut.GetAsync<MyEntity2>(entity2Id);
            var entity2 = await sut.GetAsync<MyEntity2>(entity2Id);

            Assert.NotNull(entity1);
            Assert.Equal(entity1, entity2);
        }

        [Fact]
        public async Task GetAsync_ReturnsCorrectDescendant()
        {
            var entity = await sut.GetAsync<MyEntity>(entity2Id);
            Assert.IsType<MyEntity2>(entity);
        }

        [Fact]
        public void SaveChangesAsync_AddTwiceWithSameIdsThrows()
        {
            var entity = new MyEntity(entityId, entityClassId, 5);
            sut.Add(entity);

            var entity2 = new MyEntity(entityId, entityClassId, 5);

            Assert.Throws<ArgumentException>(() => sut.Add(entity2));
        }

        [Fact]
        public async Task AddThenGetReturnsTheSame()
        {
            var entity = new MyEntity(entityId, entityClassId, 5);
            sut.Add(entity);
            var entity2 = await sut.GetAsync<MyEntity>(entity.Id);

            Assert.Equal(entity, entity2);
        }

        [Fact]
        public async Task SaveChangesAsync_SavesChangedAggregates()
        {
            var entity = new MyEntity(entityId, entityClassId, 5);
            sut.Add(entity);

            entity.UncommitedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId,
                    AggregateClassId = entityClassId
                }
            };

            List<DomainAggregateEventRecord> eventsRecords = new List<DomainAggregateEventRecord>()
            {
                new DomainAggregateEventRecord()
                {
                    ActorName = "actor",
                    AggregateVersion = entity.Version + 1,
                    DatePublished = now,
                    Event = entity.UncommitedEvents.ElementAt(0)
                }
            };

            await sut.SaveChangesAsync();

            eventStore.Received(1).AddAggregate(entityId, entityClassId);
            eventStore.Received(1)
                .PushEventsAsync(entityId,
                    Arg.Is<IEnumerable<DomainAggregateEventRecord>>(x => x.Count() == eventsRecords.Count
                        && x.Select((y, i) => new {Y = y, I = i}).All(z => eventsRecords[z.I].Equals(z.Y))),
                    entity.Version);
            eventStore.Received(1).CommitChangesAsync();
        }

        [Fact]
        public async Task SaveChangesAsync_PublishesEvents()
        {
            var entity = new MyEntity(entityId, entityClassId, 5);
            sut.Add(entity);

            var ev = new SetFooEvent()
            {
                AggregateId = entityId,
                AggregateClassId = entityClassId
            };

            entity.UncommitedEvents = new List<DomainAggregateEvent>()
            {
                ev
            };

            await sut.SaveChangesAsync();

            eventQueue.Received(1).PushEvent(ev);
        }

        public class EventA : DomainAggregateEvent
        {
        }

        public class SetFooEvent : DomainAggregateEvent
        {
        }

        public class MyEntity : IEventSourcedAggregateRoot
        {
            public MyEntity(Guid id, Guid classId, int foo) : this(id, classId)
            {
                if (foo != 5)
                {
                    throw new InvalidOperationException();
                }
            }

            protected MyEntity(Guid id, Guid classId)
            {
                Id = id;
                ClassId = classId;
            }

            public Guid Id { get; private set; }
            public Guid ClassId { get; }

            public IEnumerable<DomainAggregateEvent> UncommitedEvents { get; set; } =
                new List<DomainAggregateEvent>();
            public int Version { get; private set; }

            internal List<DomainAggregateEvent> LoadedEvents;

            public void Commit()
            {
                UncommitedEvents = new List<DomainAggregateEvent>();
                Version++;
            }

            public void LoadState(AggregateState state)
            {
                if (LoadedEvents != null)
                {
                    throw new InvalidOperationException();
                }

                Version = state.Version;
                LoadedEvents = state.Events.ToList();
            }
        }

        public class MyEntity2 : MyEntity
        {
            public MyEntity2(Guid id, Guid classId, int foo) : base(id, classId, foo)
            {
            }

            protected MyEntity2(Guid id, Guid classId) : base(id, classId)
            {
            }
        }
    }
}