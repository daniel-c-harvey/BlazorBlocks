using NetBlocks.Models;

namespace Data.Errors;

/// <summary>
/// A <see cref="ResultMessage"/> that carries the structured fields of a
/// <see cref="ClassifiedDbError"/> alongside the existing user-facing message.
///
/// Wire-additive: consumers that read <c>FailureReasons[].Message</c> are
/// unaffected; consumers that opt in can read <see cref="Code"/>, <see cref="Field"/>,
/// and <see cref="Category"/>. Polymorphic JSON serialization for the derived
/// fields is configured at the API entry (see SkipperAPI <c>Program.cs</c>).
/// </summary>
public sealed class StructuredResultMessage : ResultMessage
{
    public string Code { get; set; }
    public string? Field { get; set; }
    public DbErrorCategory Category { get; set; }

    public StructuredResultMessage(string message, string code, string? field, DbErrorCategory category)
        : base(message)
    {
        Code = code;
        Field = field;
        Category = category;
    }

    public static StructuredResultMessage FromError(ClassifiedDbError error)
        => new(error.Message, error.Code, error.Field, error.Category);
}
