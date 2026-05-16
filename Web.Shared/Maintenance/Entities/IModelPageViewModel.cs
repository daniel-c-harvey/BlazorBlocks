using Models.Shared.Common;
using Models.Shared.Models;
using Web.Shared.Errors;

namespace Web.Shared.Maintenance.Entities;

public interface IModelPageViewModel<TModel>
    where TModel : class, IModel, new()
{
    PagedResult<TModel>? Page { get; }
    NetBlocks.Models.Result? ErrorResults { get; set; }
    string SearchTerm { get; }
    Task<(int, int)> SetPage(int pageNumber, int pageSize, string searchTerm, bool refresh = false);
    void ClearCache();
    Task UpdateItem(TModel model);
    Task DeleteItem(TModel model);

    /// <summary>
    /// Submit an upsert and return the structured outcome — HTTP status plus
    /// parsed <c>{code, field, message}</c> failures — for the UI policy to
    /// route. The legacy <see cref="UpdateItem"/> still works; new call sites
    /// should prefer this one.
    /// </summary>
    Task<WriteOutcome<TModel>> SubmitWithOutcome(TModel model);

    /// <summary>
    /// Delete an item and return the structured outcome. <see cref="DeleteItem"/>
    /// remains for callers that only care about pass/fail.
    /// </summary>
    Task<WriteOutcome<TModel>> DeleteWithOutcome(TModel model);
}
