using Microsoft.EntityFrameworkCore.Storage;
using Yuviron.Application.Abstractions.Data;

namespace Yuviron.Infrastructure.Persistence;

public sealed class EfDbTransaction : IDbTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfDbTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken ct = default) => _transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default) => _transaction.RollbackAsync(ct);

    public void Dispose() => _transaction.Dispose();

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}