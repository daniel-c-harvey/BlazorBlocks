using Microsoft.EntityFrameworkCore;

namespace Data.Shared.Errors;

/// <summary>
/// Translates a provider <see cref="DbUpdateException"/> into a <see cref="ClassifiedDbError"/>.
/// Register the provider-specific implementation via DI so <see cref="Data.Repositories.Repository{TContext,TEntity}"/>
/// can classify write failures without a hard dependency on any database driver.
/// </summary>
public interface IDbExceptionClassifier
{
    /// <summary>Code used when no classifier is registered or the exception cannot be classified.</summary>
    const string UnknownCode = "db_error";

    /// <summary>
    /// Classify the exception. The optional <paramref name="constraintRegistry"/> maps
    /// provider constraint names to product-specific (code, field) pairs.
    /// </summary>
    ClassifiedDbError Classify(
        DbUpdateException exception,
        IReadOnlyDictionary<string, (string Code, string? Field)>? constraintRegistry = null);
}
