using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Maintenance.Entities;

namespace Web;

/// <summary>
/// Extension methods for registering Cerebellum.BlazorBlocks.Web services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the library-owned dependencies required by the maintenance
    /// components (<c>ModelView</c>, <c>EditModelModal</c>). Call this in your
    /// application's DI setup when using Cerebellum.BlazorBlocks.Web.
    ///
    /// <para>
    /// Registers <see cref="EditModalSaveContextHolder"/> as <b>scoped</b>. This is
    /// required, not incidental: the holder is the per-circuit bridge that hands the
    /// save context from <c>ModelView</c> to <c>EditModelModal</c>. Singleton would
    /// cross-contaminate concurrent circuits' edits; transient would give the two
    /// components different instances and silently break the bridge. Do not "tidy"
    /// this to a different lifetime.
    /// </para>
    ///
    /// <para>
    /// MudBlazor is a separate, caller-owned prerequisite: this method does not call
    /// <c>AddMudServices()</c>. Consumers must register MudBlazor themselves.
    /// </para>
    /// </summary>
    public static IServiceCollection AddBlazorBlocksWeb(this IServiceCollection services)
    {
        services.TryAddScoped<EditModalSaveContextHolder>();
        return services;
    }
}
