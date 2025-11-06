using Fermat.Domain.Shared.Interfaces;
using Fermat.Domain.Shared.Repositories;
using Fermat.Exceptions.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace Fermat.EntityFramework.Shared.Repositories;

public class WriteRepository<TEntity, TKey, TContext>(TContext context) :
    IWriteRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext
{
    public async Task<TEntity> AddAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddAsync(entity, cancellationToken);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddRangeAsync(entities, cancellationToken);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        context.Update(entity);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        context.UpdateRange(entities);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entities;
    }
    
    public async Task<TEntity> DeleteAsync(
        TKey id,
        bool permanent = false,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<TEntity>().FirstOrDefaultAsync(item => Equals(item.Id, id), cancellationToken: cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException(typeof(TEntity), id);

        //await SetEntityAsDeletedAsync(entity, permanent);
        if (permanent)
        {
            context.Remove(entity);
        }
        else
        {
            if (entity is ISoftDelete softDeletedEntity)
            {
                softDeletedEntity.IsDeleted = true;
                context.Update(entity);
            }
            else
            {
                context.Remove(entity);
            }
        }

        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity> DeleteAsync(
        TEntity entity,
        bool permanent = false,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        //await SetEntityAsDeletedAsync(entity, permanent);
        if (permanent)
        {
            context.Remove(entity);
        }
        else
        {
            if (entity is ISoftDelete softDeletedEntity)
            {
                softDeletedEntity.IsDeleted = true;
                context.Update(entity);
            }
            else
            {
                context.Remove(entity);
            }
        }

        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        bool permanent = false,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        //await SetEntityAsDeletedAsync(entity, permanent);
        if (permanent)
        {
            context.RemoveRange(entities);
        }
        else
        {
            foreach (var entity in entities)
            {
                if (entity is ISoftDelete softDeletedEntity)
                {
                    softDeletedEntity.IsDeleted = true;
                    context.Update(entity);
                }
                else
                {
                    context.Remove(entity);
                }
            }
        }

        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entities;
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    #region Delete Protected Method

    private async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
    {
        foreach (var entity in entities)
            await SetEntityAsDeletedAsync(entity, permanent);
    }

    private async Task SetEntityAsDeletedAsync(TEntity entity, bool permanent)
    {
        if (!permanent)
        {
            if (entity is ISoftDelete fullAuditedEntity)
            {
                var processedEntities = new HashSet<object>();
                await SetEntityAsSoftDeletedAsync(fullAuditedEntity, processedEntities);
            }
            else
            {
                context.Remove(entity);
            }
        }
        else
        {
            context.Remove(entity);
        }
    }

    private async Task SetEntityAsSoftDeletedAsync(ISoftDelete entity, HashSet<object> processedEntities)
    {
        if (entity.IsDeleted)
            return;

        if (!processedEntities.Add(entity))
            return;

        var entityType = entity.GetType();

        var navigations = context.Entry(entity)
            .Metadata
            .GetNavigations()
            .Where(x =>
                (
                    x.ForeignKey.DeleteBehavior == DeleteBehavior.Cascade ||
                    x.ForeignKey.DeleteBehavior == DeleteBehavior.ClientCascade
                ) &&
                x.ForeignKey.PrincipalEntityType.ClrType == entityType &&
                x.TargetEntityType.ClrType != entityType
            )
            .ToList();

        foreach (var navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned() || navigation.PropertyInfo == null)
                continue;

            if (navigation.IsCollection)
            {
                var collection = await context.Entry(entity)
                    .Collection(navigation.PropertyInfo.Name)
                    .Query()
                    .Cast<ISoftDelete>()
                    .Where(x => !x.IsDeleted)
                    .ToListAsync();

                foreach (var relatedEntity in collection)
                    await SetEntityAsSoftDeletedAsync(relatedEntity, processedEntities);
            }
            else
            {
                var reference = await context.Entry(entity)
                    .Reference(navigation.PropertyInfo.Name)
                    .Query()
                    .Cast<ISoftDelete>()
                    .FirstOrDefaultAsync();

                if (reference != null && !reference.IsDeleted)
                    await SetEntityAsSoftDeletedAsync(reference, processedEntities);
            }
        }

        entity.IsDeleted = true;
        context.Update(entity);
    }

    #endregion
}

public class WriteRepository<TEntity, TContext>(TContext context) :
    IWriteRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    public async Task<TEntity> AddAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddAsync(entity, cancellationToken);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddRangeAsync(entities, cancellationToken);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        context.Update(entity);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        context.UpdateRange(entities);
        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TEntity> DeleteAsync(
        TEntity entity,
        bool permanent = false,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        //await SetEntityAsDeletedAsync(entity, permanent);
        if (permanent)
        {
            context.Remove(entity);
        }
        else
        {
            if (entity is ISoftDelete softDeletedEntity)
            {
                softDeletedEntity.IsDeleted = true;
                context.Update(entity);
            }
            else
            {
                context.Remove(entity);
            }
        }

        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        bool permanent = false,
        bool autoSave = false,
        CancellationToken cancellationToken = default
    )
    {
        //await SetEntityAsDeletedAsync(entity, permanent);
        if (permanent)
        {
            context.RemoveRange(entities);
        }
        else
        {
            foreach (var entity in entities)
            {
                if (entity is ISoftDelete softDeletedEntity)
                {
                    softDeletedEntity.IsDeleted = true;
                    context.Update(entity);
                }
                else
                {
                    context.Remove(entity);
                }
            }
        }

        if (autoSave) await SaveChangesAsync(cancellationToken);
        return entities;
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    #region Delete Protected Method

    private async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
    {
        foreach (var entity in entities)
            await SetEntityAsDeletedAsync(entity, permanent);
    }

    private async Task SetEntityAsDeletedAsync(TEntity entity, bool permanent)
    {
        if (!permanent)
        {
            if (entity is ISoftDelete fullAuditedEntity)
            {
                var processedEntities = new HashSet<object>();
                await SetEntityAsSoftDeletedAsync(fullAuditedEntity, processedEntities);
            }
            else
            {
                context.Remove(entity);
            }
        }
        else
        {
            context.Remove(entity);
        }
    }

    private async Task SetEntityAsSoftDeletedAsync(ISoftDelete entity, HashSet<object> processedEntities)
    {
        if (entity.IsDeleted)
            return;

        if (!processedEntities.Add(entity))
            return;

        var entityType = entity.GetType();

        var navigations = context.Entry(entity)
            .Metadata
            .GetNavigations()
            .Where(x =>
                (
                    x.ForeignKey.DeleteBehavior == DeleteBehavior.Cascade ||
                    x.ForeignKey.DeleteBehavior == DeleteBehavior.ClientCascade
                ) &&
                x.ForeignKey.PrincipalEntityType.ClrType == entityType &&
                x.TargetEntityType.ClrType != entityType
            )
            .ToList();

        foreach (var navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned() || navigation.PropertyInfo == null)
                continue;

            if (navigation.IsCollection)
            {
                var collection = await context.Entry(entity)
                    .Collection(navigation.PropertyInfo.Name)
                    .Query()
                    .Cast<ISoftDelete>()
                    .Where(x => !x.IsDeleted)
                    .ToListAsync();

                foreach (var relatedEntity in collection)
                    await SetEntityAsSoftDeletedAsync(relatedEntity, processedEntities);
            }
            else
            {
                var reference = await context.Entry(entity)
                    .Reference(navigation.PropertyInfo.Name)
                    .Query()
                    .Cast<ISoftDelete>()
                    .FirstOrDefaultAsync();

                if (reference != null && !reference.IsDeleted)
                    await SetEntityAsSoftDeletedAsync(reference, processedEntities);
            }
        }

        entity.IsDeleted = true;
        context.Update(entity);
    }

    #endregion
}