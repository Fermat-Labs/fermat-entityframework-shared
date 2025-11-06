using System.Linq.Expressions;
using Fermat.Domain.Shared.Interfaces;
using Fermat.Domain.Shared.Models;
using Fermat.Domain.Shared.Repositories;
using Fermat.EntityFramework.Shared.Extensions;
using Fermat.Exceptions.Core.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Fermat.EntityFramework.Shared.Repositories;

/// <summary>
/// Base implementation of repository pattern for Entity Framework with typed key
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type</typeparam>
/// <typeparam name="TContext">The database context type</typeparam>
public class EfRepositoryBase<TEntity, TKey, TContext>(TContext context) :
    IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext
{
    public IQueryable<TEntity> AsQueryable(bool ignoreFilters = false)
    {
        var queryable = context.Set<TEntity>();
        return ignoreFilters ? queryable.IgnoreQueryFilters() : queryable;
    }

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.SingleOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<TEntity> GetAsync(
        TKey id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        var entity =
            await queryable.FirstOrDefaultAsync(item => Equals(item.Id, id), cancellationToken: cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException(typeof(TEntity), id);
        return entity;
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<SortRequest>? sorts = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sorts != null) queryable = queryable.ApplySort(sorts);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<PageableResult<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<PageableResult<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<SortRequest>? sorts = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sorts != null) queryable = queryable.ApplySort(sorts);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.AnyAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (predicate != null) queryable = queryable.Where(predicate);
        queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.CountAsync(cancellationToken: cancellationToken);
    }

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

/// <summary>
/// Base implementation of repository pattern for Entity Framework without typed key
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TContext">The database context type</typeparam>
public class EfRepositoryBase<TEntity, TContext>(TContext context) :
    IRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    public IQueryable<TEntity> AsQueryable(bool ignoreFilters = false)
    {
        var queryable = context.Set<TEntity>();
        return ignoreFilters ? queryable.IgnoreQueryFilters() : queryable;
    }

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.SingleOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<SortRequest>? sorts = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sorts != null) queryable = queryable.ApplySort(sorts);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<PageableResult<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<PageableResult<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<SortRequest>? sorts = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sorts != null) queryable = queryable.ApplySort(sorts);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.AnyAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (predicate != null) queryable = queryable.Where(predicate);
        queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.CountAsync(cancellationToken: cancellationToken);
    }

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