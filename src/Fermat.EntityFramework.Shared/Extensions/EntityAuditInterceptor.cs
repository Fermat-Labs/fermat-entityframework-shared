using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Fermat.EntityFramework.Shared.Extensions;

/// <summary>
/// Intercepts Entity Framework save operations to apply audit metadata
/// </summary>
public class EntityAuditInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepts the saving changes operation to apply audit metadata
    /// </summary>
    /// <param name="eventData">The event data containing the database context</param>
    /// <param name="result">The interception result</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The interception result</returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var httpContextAccessor = context.GetService<IHttpContextAccessor>();

        context.SetCreationTimestamps(httpContextAccessor);
        context.SetModificationTimestamps(httpContextAccessor);
        context.SetSoftDelete(httpContextAccessor);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

/// <summary>
/// Provides extension methods for DbContextOptionsBuilder to configure entity metadata tracking
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the DbContext to use entity metadata tracking interceptor
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure</param>
    /// <returns>The configured options builder</returns>
    public static DbContextOptionsBuilder UseEntityMetadataTracking(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new EntityAuditInterceptor());
        return optionsBuilder;
    }
}