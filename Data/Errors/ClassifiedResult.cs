using NetBlocks.Models;

namespace Data.Errors;

/// <summary>
/// A <see cref="ResultContainer{T}"/> that also carries the structured
/// <see cref="ClassifiedDbError"/> from a write failure. The controller layer
/// inspects this side channel to choose an HTTP status code and to emit
/// <see cref="StructuredResultMessage"/> on the wire DTO.
///
/// The base <c>Messages</c> collection still carries the human-readable text
/// via <c>Fail(string)</c>, so existing consumers reading
/// <c>Messages[].Message</c> are unaffected.
/// </summary>
public sealed class ClassifiedResult<T> : ResultContainer<T>
{
    public ClassifiedDbError? ClassifiedError { get; init; }

    public ClassifiedResult() { }

    public static ClassifiedResult<T> FromException(ClassifiedDbException ex)
    {
        var result = new ClassifiedResult<T> { ClassifiedError = ex.Error };
        result.Fail(ex.Error.Message);
        return result;
    }
}

/// <summary>
/// Non-generic counterpart to <see cref="ClassifiedResult{T}"/> for write paths
/// that return <see cref="Result"/> (Update, Delete).
/// </summary>
public sealed class ClassifiedResult : Result
{
    public ClassifiedDbError? ClassifiedError { get; init; }

    public ClassifiedResult() { }

    public static ClassifiedResult FromException(ClassifiedDbException ex)
    {
        var result = new ClassifiedResult { ClassifiedError = ex.Error };
        result.Fail(ex.Error.Message);
        return result;
    }
}
