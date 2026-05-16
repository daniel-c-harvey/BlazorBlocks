namespace Data.Shared.Errors;

/// <summary>
/// Product-meaningful classification of an EF / database write failure. Decouples the
/// rest of the stack from provider-specific SQLSTATE codes and exception types.
/// </summary>
public enum DbErrorCategory
{
    /// <summary>Unique constraint or unique index violation (Postgres 23505).</summary>
    UniqueViolation,

    /// <summary>Foreign key violation (Postgres 23503).</summary>
    ForeignKeyViolation,

    /// <summary>NOT NULL violation (Postgres 23502).</summary>
    NotNull,

    /// <summary>Value too long for column type (Postgres 22001).</summary>
    LengthOverrun,

    /// <summary>Numeric value out of range (Postgres 22003).</summary>
    RangeOverrun,

    /// <summary>CHECK constraint violation (Postgres 23514).</summary>
    CheckViolation,

    /// <summary>EF optimistic concurrency token mismatch.</summary>
    Concurrency,

    /// <summary>Unclassified write failure.</summary>
    Unknown
}
