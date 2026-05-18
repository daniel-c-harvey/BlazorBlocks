namespace Web.Errors;

/// <summary>
/// One entry from the wire-level <c>failureReasons</c> array on an
/// <c>ApiResultDto&lt;T&gt;</c> response, lifted into a shape the UI can route on.
/// <see cref="Code"/> and <see cref="Field"/> are populated when the server
/// emitted a <c>StructuredResultMessage</c>; both absent means a plain
/// <c>ResultMessage</c> with only <see cref="Message"/>.
/// </summary>
public sealed record StructuredFailure(string Message, string? Code, string? Field);
