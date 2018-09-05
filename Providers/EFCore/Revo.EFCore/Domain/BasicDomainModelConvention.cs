﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Revo.Domain.ReadModel;
using Revo.EFCore.DataAccess.Conventions;

namespace Revo.EFCore.Domain
{
    public class BasicDomainModelConvention : EFCoreConventionBase
    {
        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainAggregateEvent>();

            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(IRowVersioned).IsAssignableFrom(x.ClrType)))
            {
                entity.FindProperty(nameof(IRowVersioned.Version)).IsConcurrencyToken = true;
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(IManuallyRowVersioned).IsAssignableFrom(x.ClrType)))
            {
                entity.FindProperty(nameof(IManuallyRowVersioned.Version)).IsConcurrencyToken = true;
            }
            
            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(BasicAggregateRoot).IsAssignableFrom(x.ClrType)))
            {
                var entityBuilder = modelBuilder.Entity(entity.ClrType);

                entityBuilder.Ignore(nameof(BasicAggregateRoot.IsChanged));
                entityBuilder.Ignore(nameof(BasicAggregateRoot.IsDeleted));
                entityBuilder.Ignore(nameof(BasicAggregateRoot.UncommittedEvents));
            }
        }
    }
}
