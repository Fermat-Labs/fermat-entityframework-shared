using Fermat.Domain.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fermat.EntityFramework.Shared.Repositories;

public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private bool _disposed;

    public UnitOfWork(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IDbContextTransaction? CurrentTransaction => _context.Database.CurrentTransaction;
    
    public bool HasActiveTransaction => CurrentTransaction != null;

    public IDbContextTransaction BeginTransaction()
    {
        if (HasActiveTransaction)
        {
            return new NoOpTransaction(CurrentTransaction!);
        }
        
        return _context.Database.BeginTransaction();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            return new NoOpTransaction(CurrentTransaction!);
        }
        
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}