using Web.Errors;

namespace Web.Maintenance.Entities;

/// <summary>
/// Cascaded by <c>ModelView&lt;...&gt;</c> into the edit modal so the modal
/// can submit the model itself and receive a structured outcome. The
/// per-page modal wrapper (e.g. <c>EditSlipModal</c>) does not need to know
/// about this — it just forwards <c>Model</c>; the generic
/// <c>EditModelModal</c> picks the cascade up directly.
///
/// Non-generic by design: the modal calls <see cref="SubmitAsync"/> with the
/// boxed model and receives a non-generic <see cref="WriteOutcome{Object}"/>;
/// the <c>Value</c> field is unused on the edit path (the form already has
/// the model). Generic types here would force every per-page wrapper to
/// declare the cascade's generic argument.
/// </summary>
public interface IEditModalSaveContext
{
    Task<WriteOutcome<object>> SubmitAsync(object model);
}
