using System.Linq.Expressions;
using Fermat.Domain.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fermat.EntityFramework.Shared.Extensions;

/// <summary>
/// Provides extension methods for ModelBuilder to apply global configurations
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies global configurations to all entity types in the model
    /// </summary>
    /// <param name="builder">The model builder to configure</param>
    public static void ApplyGlobalConfigurations(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var entityClrType = entityType.ClrType;
            var entityInterfaces = entityClrType.GetInterfaces();

            var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
            if (isIEntity)
            {
                var idProperty = entityClrType.GetProperty("Id");
                if (idProperty != null)
                {
                    builder.Entity(entityClrType)
                        .Property(idProperty.Name)
                        .ValueGeneratedOnAdd();
                }
            }

            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                        Expression.Constant(false)
                    ),
                    parameter
                );

                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).Property
                        (nameof(ICreationAuditedObject.CreationTime))
                    .IsRequired();

                builder.Entity(entityType.ClrType)
                    .Property(nameof(ICreationAuditedObject.CreatorId))
                    .HasMaxLength(256)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType).HasIndex(nameof(ICreationAuditedObject.CreatorId));
                builder.Entity(entityType.ClrType).HasIndex(nameof(ICreationAuditedObject.CreationTime));
            }

            if (typeof(IAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditedObject.LastModificationTime))
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditedObject.LastModifierId))
                    .HasMaxLength(256)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType).HasIndex(nameof(IAuditedObject.LastModifierId));
                builder.Entity(entityType.ClrType).HasIndex(nameof(IAuditedObject.LastModificationTime));
            }

            if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IDeletionAuditedObject.DeletionTime))
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IDeletionAuditedObject.DeleterId))
                    .HasMaxLength(256)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType).HasIndex(nameof(IDeletionAuditedObject.DeleterId));
                builder.Entity(entityType.ClrType).HasIndex(nameof(IDeletionAuditedObject.DeletionTime));
                builder.Entity(entityType.ClrType).HasIndex(nameof(IDeletionAuditedObject.IsDeleted));
            }
        }
    }

    /// <summary>
    /// Applies global entity configurations to a specific entity type
    /// </summary>
    /// <typeparam name="T">The entity type to configure</typeparam>
    /// <param name="builder">The entity type builder to configure</param>
    public static void ApplyGlobalEntityConfigurations<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        var entityType = typeof(T);
        var entityInterfaces = entityType.GetInterfaces();

        var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
        if (isIEntity)
        {
            var idProperty = entityType.GetProperty("Id");
            if (idProperty != null)
            {
                builder.Property(idProperty.Name)
                    .ValueGeneratedOnAdd();
            }
        }

        if (typeof(ISoftDelete).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                    Expression.Constant(false)
                ),
                parameter
            );

            builder.HasQueryFilter((LambdaExpression)filter);
        }

        if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ICreationAuditedObject.CreationTime))
                .IsRequired();

            builder.Property(nameof(ICreationAuditedObject.CreatorId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(ICreationAuditedObject.CreatorId));
            builder.HasIndex(nameof(ICreationAuditedObject.CreationTime));
        }

        if (typeof(IAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IAuditedObject.LastModificationTime))
                .IsRequired(false);

            builder.Property(nameof(IAuditedObject.LastModifierId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IAuditedObject.LastModifierId));
            builder.HasIndex(nameof(IAuditedObject.LastModificationTime));
        }

        if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IDeletionAuditedObject.DeletionTime))
                .IsRequired(false);

            builder.Property(nameof(IDeletionAuditedObject.DeleterId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IDeletionAuditedObject.DeleterId));
            builder.HasIndex(nameof(IDeletionAuditedObject.DeletionTime));
            builder.HasIndex(nameof(IDeletionAuditedObject.IsDeleted));
        }
    }
}