namespace Web.Shared.Errors;

/// <summary>
/// How a <see cref="WriteOutcome{T}"/> should be surfaced to the user.
/// Picked by <see cref="FormErrorPresenter.Classify{T}"/> from the
/// HTTP status and the structured failure list. The caller decides what
/// to do with each kind — the policy is shared, the wiring is local.
/// </summary>
public enum FormErrorPresentation
{
    /// <summary>The call succeeded; nothing to surface.</summary>
    None,

    /// <summary>
    /// 400 / 409 / 422 with exactly one <see cref="StructuredFailure"/>
    /// carrying a <c>Field</c> — push the message inline under that field
    /// via <c>ValidationMessageStore</c>; keep the form open.
    /// </summary>
    InlineField,

    /// <summary>
    /// 400 / 409 / 422 with no usable field path — show a snackbar toast
    /// at <c>Severity.Error</c>; keep the form open.
    /// </summary>
    SnackbarError,

    /// <summary>
    /// 403 — show a page-level <c>MudAlert</c> at <c>Severity.Warning</c>;
    /// the user has nothing to fix on this form.
    /// </summary>
    PageAlertWarning,

    /// <summary>
    /// 500 (or any unclassified failure) — show the existing
    /// <c>ModelSubmittedModal</c> with a generated reference id; the
    /// modal remains the fallback for unexpected failures.
    /// </summary>
    ServerErrorModal,
}
