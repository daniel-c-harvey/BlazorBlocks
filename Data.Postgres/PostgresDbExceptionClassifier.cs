using Data.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Data.Postgres;

/// <summary>
/// Postgres-specific implementation of <see cref="IDbExceptionClassifier"/>. Inspects the
/// inner-exception chain for a <see cref="PostgresException"/> and maps its SQLSTATE and
/// constraint name onto the provider-agnostic <see cref="ClassifiedDbError"/> shape.
/// Register as a singleton in DI; the class is stateless and thread-safe.
/// </summary>
public sealed class PostgresDbExceptionClassifier : IDbExceptionClassifier
{
    /// <summary>Default fallback code when no registry entry matches the violated constraint.</summary>
    public const string GenericConstraintCode = "constraint_violation";

    /// <summary>Code reported when the inner provider exception cannot be interpreted.</summary>
    public const string UnknownCode = "db_error";

    /// <summary>Code reported for EF optimistic-concurrency mismatches.</summary>
    public const string ConcurrencyCode = "concurrency_conflict";

    /// <inheritdoc />
    public ClassifiedDbError Classify(
        DbUpdateException exception,
        IReadOnlyDictionary<string, (string Code, string? Field)>? constraintRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is DbUpdateConcurrencyException)
        {
            return new ClassifiedDbError(
                Category: DbErrorCategory.Concurrency,
                Code: ConcurrencyCode,
                Field: null,
                ConstraintName: null,
                Message: "This record was changed by someone else. Reload and try again.");
        }

        // Walk the inner-exception chain looking for the typed Postgres exception. EF wraps
        // provider exceptions, but additional wrappers (e.g. retrying execution strategies)
        // can appear in between.
        var pg = FindPostgresException(exception);
        if (pg is null)
        {
            return new ClassifiedDbError(
                Category: DbErrorCategory.Unknown,
                Code: UnknownCode,
                Field: null,
                ConstraintName: null,
                Message: "A database error occurred while saving changes.");
        }

        var category = MapSqlState(pg.SqlState);
        var constraintName = string.IsNullOrEmpty(pg.ConstraintName) ? null : pg.ConstraintName;
        var (code, field) = ResolveCodeAndField(category, constraintName, constraintRegistry);
        var message = DefaultMessageFor(category);

        return new ClassifiedDbError(category, code, field, constraintName, message);
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (var ex = exception.InnerException; ex is not null; ex = ex.InnerException)
        {
            if (ex is PostgresException pg) return pg;
        }
        return null;
    }

    private static DbErrorCategory MapSqlState(string? sqlState) => sqlState switch
    {
        "23505" => DbErrorCategory.UniqueViolation,
        "23503" => DbErrorCategory.ForeignKeyViolation,
        "23502" => DbErrorCategory.NotNull,
        "22001" => DbErrorCategory.LengthOverrun,
        "22003" => DbErrorCategory.RangeOverrun,
        "23514" => DbErrorCategory.CheckViolation,
        _ => DbErrorCategory.Unknown
    };

    private static (string Code, string? Field) ResolveCodeAndField(
        DbErrorCategory category,
        string? constraintName,
        IReadOnlyDictionary<string, (string Code, string? Field)>? registry)
    {
        if (constraintName is not null
            && registry is not null
            && registry.TryGetValue(constraintName, out var mapped))
        {
            return mapped;
        }

        return category switch
        {
            DbErrorCategory.UniqueViolation => (GenericConstraintCode, null),
            DbErrorCategory.ForeignKeyViolation => (GenericConstraintCode, null),
            DbErrorCategory.CheckViolation => (GenericConstraintCode, null),
            DbErrorCategory.NotNull => ("required_field", null),
            DbErrorCategory.LengthOverrun => ("value_too_long", null),
            DbErrorCategory.RangeOverrun => ("value_out_of_range", null),
            DbErrorCategory.Concurrency => (ConcurrencyCode, null),
            _ => (UnknownCode, null)
        };
    }

    private static string DefaultMessageFor(DbErrorCategory category) => category switch
    {
        DbErrorCategory.UniqueViolation => "A record with this value already exists.",
        DbErrorCategory.ForeignKeyViolation => "A referenced record was not found.",
        DbErrorCategory.NotNull => "A required field was missing.",
        DbErrorCategory.LengthOverrun => "A value was too long for its field.",
        DbErrorCategory.RangeOverrun => "A numeric value was outside the allowed range.",
        DbErrorCategory.CheckViolation => "A value did not satisfy a validation rule.",
        DbErrorCategory.Concurrency => "This record was changed by someone else. Reload and try again.",
        _ => "A database error occurred while saving changes."
    };
}
