namespace Data.Shared.Errors;

/// <summary>
/// Structured representation of a write failure at the data boundary.
/// </summary>
/// <param name="Category">Provider-agnostic failure category.</param>
/// <param name="Code">Product-meaningful error code. Comes from the per-entity constraint
/// registry when the violated constraint is registered; otherwise a generic fallback.</param>
/// <param name="Field">The user-facing field path the failure attaches to, when known.</param>
/// <param name="ConstraintName">The raw constraint identifier from the provider (e.g.
/// <c>IX_slips_SlipNumber</c>), when available. Useful for diagnostics and for callers
/// that need to disambiguate registry misses.</param>
/// <param name="Message">A safe, user-facing default message. Pages may override.</param>
public sealed record ClassifiedDbError(
    DbErrorCategory Category,
    string Code,
    string? Field,
    string? ConstraintName,
    string Message);
