using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Domain.Common;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Extensions;

public static class ValidationExtensions
{
    public static async Task EnsureAllExistAsync<TEntity>(
        this IQueryable<TEntity> query, 
        IEnumerable<Guid> ids, 
        string entityName, 
        CancellationToken cancellationToken = default) where TEntity : Entity
    {
        var uniqueIds = ids.Distinct().ToList();
        if (!uniqueIds.Any()) return;

        var existingCount = await query.CountAsync(e => uniqueIds.Contains(e.Id), cancellationToken);
        
        if (existingCount != uniqueIds.Count)
        {
            throw new NotFoundException(entityName, "One or more provided IDs do not exist.");
        }
    }
}