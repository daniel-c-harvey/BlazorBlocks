using Microsoft.AspNetCore.Mvc;
using Models.Shared.Common;
using Models.Shared.Entities;
using Models.Shared.Models;
using NetBlocks.Models;

namespace API.Shared.Controllers;

public interface IModelController<TModel>
where TModel : class, IModel, new()
{
    Task<ActionResult<ApiResultDto<TModel>>> Get(long id);
    Task<ActionResult<ApiResultDto<IEnumerable<TModel>>>> GetAll();
    Task<ActionResult<ApiResultDto<PagedResult<TModel>>>> Get([FromQuery] PagedQuery query);
    Task<ActionResult<ApiResultDto<ItemCount>>> GetCount([FromQuery] PagedQuery query);
    Task<ActionResult<ApiResultDto<TModel>>> Post([FromBody] TModel model);
    Task<ActionResult<ApiResultDto>> Delete(long id);
}