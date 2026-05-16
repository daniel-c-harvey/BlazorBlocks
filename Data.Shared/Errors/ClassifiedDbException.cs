namespace Data.Shared.Errors;

/// <summary>
/// Carries a <see cref="ClassifiedDbError"/> from the repository up to the manager
/// boundary without changing the public method signatures of <c>Repository&lt;T&gt;</c>.
/// The manager catches this specifically and surfaces <see cref="ClassifiedDbError.Message"/>
/// into the returned <c>Result</c> / <c>ResultContainer&lt;T&gt;</c>; raw exception text
/// never escapes the data layer.
/// </summary>
public sealed class ClassifiedDbException : Exception
{
    public ClassifiedDbError Error { get; }

    public ClassifiedDbException(ClassifiedDbError error, Exception innerException)
        : base(error.Message, innerException)
    {
        Error = error;
    }
}
