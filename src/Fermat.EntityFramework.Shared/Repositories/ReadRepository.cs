using System.Linq.Expressions;
using Fermat.Domain.Shared.Interfaces;
using Fermat.Domain.Shared.Models;
using Fermat.Domain.Shared.Repositories;
using Fermat.EntityFramework.Shared.Extensions;
using Fermat.Exceptions.Core.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Fermat.EntityFramework.Shared.Repositories;

public class ReadRepository<TEntity, TKey, TContext>(TContext context) :
    IReadRepository<TEntity, TKey>
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
}

public class ReadRepository<TEntity, TContext>(TContext context) :
    IReadRepository<TEntity>
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
}