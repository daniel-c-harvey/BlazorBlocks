using Models.Common;
using Models.Models;
using NetBlocks.Models;

namespace Web.ApiClients;

public interface IModelClient<TModel>
    where TModel : class, IModel, new()
{
    Task<ApiResult<TModel>> GetById(long id);
    Task<ApiResult<IEnumerable<TModel>>> GetAll();
    Task<ApiResult<ItemCount>> GetCount();
    Task<ApiResult<PagedResult<TModel>>> GetByPage(PagedQuery query);
    Task<ApiResult<ItemCount>> GetPageCount(PagedQuery query);
    Task<ApiResult<TModel>> Update(TModel model);
    Task<ApiResult> Delete(TModel model);
}