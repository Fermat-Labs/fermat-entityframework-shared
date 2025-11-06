using Microsoft.EntityFrameworkCore.Storage;

namespace Fermat.EntityFramework.Shared.Repositories;

public class NoOpTransaction : IDbContextTransaction
{
    private readonly IDbContextTransaction _innerTransaction;

    public NoOpTransaction(IDbContextTransaction innerTransaction)
    {
        _innerTransaction = innerTransaction ?? throw new ArgumentNullException(nameof(innerTransaction));
    }

    public Guid TransactionId => _innerTransaction.TransactionId;
    public bool SupportsSavepoints => _innerTransaction.SupportsSavepoints;

    public void Commit() 
    { 
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) 
    {
        return Task.CompletedTask;
    }

    public void Rollback() 
    { 
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default) 
    {
        return Task.CompletedTask;
    }

    public void Dispose() 
    { 
    }

    public ValueTask DisposeAsync() 
    {
        return ValueTask.CompletedTask;
    }

    public Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        return _innerTransaction.CreateSavepointAsync(name, cancellationToken);
    }

    public Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        return _innerTransaction.ReleaseSavepointAsync(name, cancellationToken);
    }

    public Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        return _innerTransaction.RollbackToSavepointAsync(name, cancellationToken);
    }
}