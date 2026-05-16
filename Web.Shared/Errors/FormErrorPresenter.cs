using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Web.Shared.Errors;

/// <summary>
/// Implements the UI presentation policy for write failures coming off the
/// API. The policy itself is a pure classification of a
/// <see cref="WriteOutcome{T}"/>; the helpers apply that classification to
/// the right MudBlazor / Blazor surface so call sites carry exactly one
/// line of policy code each.
///
/// Reads the structured failure list verbatim from the outcome — the
/// upstream layers (server status mapping, structured <c>FailureReason</c>)
/// already did the work of saying *what kind* of failure it is. This class
/// only decides *where it goes*.
/// </summary>
public static class FormErrorPresenter
{
    /// <summary>
    /// Returns which surface the outcome belongs on. Pure function; no
    /// side effects.
    /// </summary>
    public static FormErrorPresentation Classify<T>(WriteOutcome<T> outcome)
    {
        if (outcome.Success) return FormErrorPresentation.None;

        return outcome.HttpStatus switch
        {
            403 => FormErrorPresentation.PageAlertWarning,
            400 or 409 or 422 => outcome.FieldFailure is not null
                ? FormErrorPresentation.InlineField
                : FormErrorPresentation.SnackbarError,
            // 500 and anything unclassified (including transport errors with
            // HttpStatus == 0) flow to the modal — that's the fallback path
            // by design.
            _ => FormErrorPresentation.ServerErrorModal,
        };
    }

    /// <summary>
    /// Push the outcome's single field-scoped failure into the form's
    /// <paramref name="editContext"/> so the existing
    /// <c>&lt;ValidationMessage For="..."/&gt;</c> surfaces it inline.
    ///
    /// Returns <c>true</c> when a field message was applied. When the
    /// <c>Field</c> path does not resolve to a property on the model
    /// (e.g. the server emits a path the form does not bind to), this
    /// returns <c>false</c> so the caller can fall back to a snackbar.
    /// </summary>
    public static bool ApplyFieldFailure<T>(
        EditContext editContext,
        ValidationMessageStore store,
        WriteOutcome<T> outcome)
    {
        ArgumentNullException.ThrowIfNull(editContext);
        ArgumentNullException.ThrowIfNull(store);

        var failure = outcome.FieldFailure;
        if (failure is null || string.IsNullOrEmpty(failure.Field)) return false;

        var identifier = TryResolveFieldIdentifier(editContext.Model, failure.Field);
        if (identifier is null) return false;

        store.Add(identifier.Value, failure.Message);
        editContext.NotifyValidationStateChanged();
        return true;
    }

    /// <summary>
    /// Show the outcome's failure message as a snackbar toast. Concatenates
    /// multiple failures with a separator so none are dropped silently.
    /// </summary>
    public static void ShowSnackbar<T>(
        ISnackbar snackbar,
        WriteOutcome<T> outcome,
        Severity severity = Severity.Error)
    {
        ArgumentNullException.ThrowIfNull(snackbar);

        var message = outcome.Failures.Count == 0
            ? "The request could not be completed."
            : string.Join(" ", outcome.Failures.Select(f => f.Message));
        snackbar.Add(message, severity);
    }

    /// <summary>
    /// Resolve a dotted-or-flat path to a <see cref="FieldIdentifier"/>. Only
    /// handles top-level properties today — that's what the EP.3 server emits
    /// (e.g. <c>"SlipNumber"</c>, <c>"SlipClassification"</c>). Returns null
    /// for paths that don't resolve so the caller can fall back to a toast.
    /// </summary>
    private static FieldIdentifier? TryResolveFieldIdentifier(object model, string fieldPath)
    {
        var dotIndex = fieldPath.IndexOf('.');
        if (dotIndex < 0)
        {
            return ModelHasProperty(model, fieldPath)
                ? new FieldIdentifier(model, fieldPath)
                : null;
        }

        // Nested path — walk to the owning object. Property names that don't
        // exist at any level bail out to null, as do sub-object instances that
        // are null on the model; both cases fall back to a snackbar.
        var head = fieldPath[..dotIndex];
        var tail = fieldPath[(dotIndex + 1)..];

        if (!ModelHasProperty(model, head)) return null;

        var nested = model.GetType().GetProperty(head)?.GetValue(model);
        return nested is null ? null : TryResolveFieldIdentifier(nested, tail);
    }

    /// <summary>
    /// Generates a short, human-readable client-side reference id for a
    /// server-error modal. Eight upper-case hex characters; unique enough for
    /// users to quote to support. Ops correlates by timestamp + endpoint
    /// because the server does not surface its own correlation id on the wire.
    /// </summary>
    public static string GenerateClientReference()
        => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    private static bool ModelHasProperty(object model, string name)
        => model.GetType().GetProperty(name) is not null;
}
