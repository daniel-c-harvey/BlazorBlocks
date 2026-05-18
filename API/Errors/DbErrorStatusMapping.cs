using Data.Errors;
using Microsoft.AspNetCore.Http;

namespace API.Errors;

/// <summary>
/// Maps a <see cref="DbErrorCategory"/> to the HTTP status code the API should
/// return. Conflict-shaped failures (unique violation, optimistic concurrency)
/// surface as <c>409</c>; semantic-rule failures (foreign key, check, not-null,
/// length / range overrun) surface as <c>422</c>; anything unclassified is a
/// server-side defect and surfaces as <c>500</c>.
/// </summary>
public static class DbErrorStatusMapping
{
    public static int MapCategoryToStatus(DbErrorCategory category) => category switch
    {
        DbErrorCategory.UniqueViolation => StatusCodes.Status409Conflict,
        DbErrorCategory.Concurrency => StatusCodes.Status409Conflict,
        DbErrorCategory.ForeignKeyViolation => StatusCodes.Status422UnprocessableEntity,
        DbErrorCategory.CheckViolation => StatusCodes.Status422UnprocessableEntity,
        DbErrorCategory.NotNull => StatusCodes.Status422UnprocessableEntity,
        DbErrorCategory.LengthOverrun => StatusCodes.Status422UnprocessableEntity,
        DbErrorCategory.RangeOverrun => StatusCodes.Status422UnprocessableEntity,
        DbErrorCategory.Unknown => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError,
    };
}
