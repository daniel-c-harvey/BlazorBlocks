using Data.Errors;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Postgres;

/// <summary>
/// Extension methods for registering Cerebellum.BlazorBlocks.Data.Postgres services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="PostgresDbExceptionClassifier"/> as the <see cref="IDbExceptionClassifier"/>
    /// singleton for this application. Call this in your application's DI setup when using
    /// Cerebellum.BlazorBlocks.Data with a PostgreSQL database.
    /// </summary>
    public static IServiceCollection AddBlazorBlocksPostgres(this IServiceCollection services)
    {
        services.AddSingleton<IDbExceptionClassifier, PostgresDbExceptionClassifier>();
        return services;
    }
}
