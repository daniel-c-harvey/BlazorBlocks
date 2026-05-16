namespace Web.Shared.Errors;

/// <summary>
/// The structured outcome of a write call (POST/DELETE) against the API.
/// Carries the HTTP status alongside the parsed <see cref="StructuredFailure"/>
/// list so the UI can route 400/403/409/422/500 to the right surface
/// (inline field error, snackbar toast, page alert, modal).
///
/// Constructed from a successfully-parsed <c>ApiResultDto&lt;T&gt;</c> response;
/// when the response cannot be parsed or the call throws, <see cref="HttpStatus"/>
/// is <c>0</c> and <see cref="Failures"/> carries the transport error message.
/// </summary>
public sealed record WriteOutcome<T>
{
    public required bool Success { get; init; }
    public required int HttpStatus { get; init; }
    public T? Value { get; init; }
    public IReadOnlyList<StructuredFailure> Failures { get; init; } = [];

    /// <summary>
    /// Convenience: the single field-scoped failure, or null if there is none
    /// (zero failures, multiple failures, or all failures lack a <c>Field</c>).
    /// </summary>
    public StructuredFailure? FieldFailure =>
        Failures.Count == 1 && !string.IsNullOrEmpty(Failures[0].Field)
            ? Failures[0]
            : null;
}
