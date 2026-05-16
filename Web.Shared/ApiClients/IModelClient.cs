using Models.Shared.Common;
using Models.Shared.Models;
using NetBlocks.Models;

namespace Web.Shared.ApiClients;

public interface IModelClient<TModel>
    where TModel : class, IModel, new()
{
    Task<ApiResult<TModel>> GetById(long id);
    Task<ApiResult<IEnumerable<TModel>>> GetAll();
    Task<ApiResult<PagedResult<TModel>>> GetByPage(PagedQuery query);
    Task<ApiResult<ItemCount>> GetPageCount(PagedQuery query);
    Task<ApiResult<TModel>> Update(TModel model);
    Task<ApiResult> Delete(TModel model);
}