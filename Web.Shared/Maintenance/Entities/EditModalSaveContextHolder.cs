namespace Web.Shared.Maintenance.Entities;

/// <summary>
/// Hands an <see cref="IEditModalSaveContext"/> from the calling
/// <c>ModelView</c> to the generic <c>EditModelModal</c> through Blazor DI
/// instead of through the per-page modal wrapper. The wrappers
/// (e.g. <c>EditSlipModal</c>) only forward <c>Model</c>; threading a save
/// callback through every wrapper would force a per-page edit on each one,
/// which the EP.4 brief explicitly rules out.
///
/// Scoped per circuit. Modal lifecycle is sequential (the user opens one
/// edit dialog at a time), so a single slot is safe; assigning a new
/// context while another is in flight overwrites it intentionally.
/// </summary>
public sealed class EditModalSaveContextHolder
{
    public IEditModalSaveContext? Current { get; set; }
}
