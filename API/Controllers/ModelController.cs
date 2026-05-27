using System.Linq.Expressions;
using API.Errors;
using Data.Errors;
using Data.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models.Common;
using Models.Entities;
using Models.Models;
using NetBlocks.Models;

namespace API.Controllers;

public abstract class ModelController<TEntity, TModel, TManager> : ControllerBase, IModelController<TModel> 
    where TEntity : class, IEntity, new()
    where TModel : class, IModel, new()
    where TManager : IManager<TEntity, TModel>
{
    protected readonly TManager Manager;
    protected readonly Dictionary<string, Expression<Func<TEntity, object>>> SortExpressions = new(StringComparer.OrdinalIgnoreCase);
    protected ModelController(TManager manager)
    {
        Manager = manager;
            
        SortExpressions[nameof(IEntity.Id)] = e => e.Id;
        SortExpressions[nameof(IEntity.CreatedAt)] = e => e.CreatedAt;
        SortExpressions[nameof(IEntity.UpdatedAt)] = e => e.UpdatedAt;
    }
        
    /// <summary>
    /// Get entity by ID
    /// </summary>
    [HttpGet("{id:long}")]
    public virtual async Task<ActionResult<ApiResultDto<TModel>>> Get(long id)
    {
        var getResult = await Manager.GetById(id);
            
        ApiResult<TModel> result = ApiResult<TModel>.From(getResult);
        ApiResultDto<TModel> dto = new(result);
            
        if (!result.Success) { return StatusCode(500, dto); }
            
        return result.Value == null ? NotFound(dto) : Ok(dto);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResultDto<IEnumerable<TModel>>>> GetAll()
    {
        var queryResult = await Manager.Get();
            
        var result = ApiResult<IEnumerable<TModel>>.From(queryResult);
        var dto = new ApiResultDto<IEnumerable<TModel>>(result);
            
        return !result.Success ? StatusCode(500, dto) : Ok(dto);
    }

    [HttpGet]
    public virtual async Task<ActionResult<ApiResultDto<PagedResult<TModel>>>> Get([FromQuery] PagedQuery query)
    {
        var paging = new PagingParameters<TEntity>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            OrderBy = GetSortExpression(query.Sort),
            IsDescending = query.Desc
        }; 
            
        var predicate = BuildSearchPredicate(query.Search);
        var pageResult = await Manager.GetPage(predicate, paging);
            
        var result = ApiResult<PagedResult<TModel>>.From(pageResult);
        ApiResultDto<PagedResult<TModel>> dto = new(result);
            
        return result.Success ? Ok(dto) : StatusCode(500, dto);
    }

    [HttpGet("count")]
    public async Task<ActionResult<ApiResultDto<ItemCount>>> GetCount([FromQuery] string? search = null)
    {
        var countResult = await Manager.GetCount(BuildSearchPredicate(search));
        
        ApiResult<ItemCount> result = ApiResult<ItemCount>.From(countResult, new ItemCount(countResult.Value));
        ApiResultDto<ItemCount> dto = new(result);
        
        return result.Success ? 
            Ok(dto) : 
            StatusCode(500, dto);
    }

    [HttpGet("pagecount")]
    public virtual async Task<ActionResult<ApiResultDto<ItemCount>>> GetPageCount([FromQuery] PagedQuery query)
    {
        var paging = new PagingParameters<TEntity>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            OrderBy = GetSortExpression(query.Sort),
            IsDescending = query.Desc
        };
        
        var predicate = BuildSearchPredicate(query.Search);
        var countResult = await Manager.GetPageCount(predicate, paging);
        
        ApiResult<ItemCount> result = ApiResult<ItemCount>.From(countResult, new ItemCount(countResult.Value));
        ApiResultDto<ItemCount> dto = new(result);

        return result.Success ? 
            Ok(dto) : 
            StatusCode(500, dto);
    }

    /// <summary>
    /// Create entity - accepts domain entity directly
    /// </summary>
    [HttpPost]
    public virtual async Task<ActionResult<ApiResultDto<TModel>>> Post([FromBody] TModel model)
    {
        var existsResult = await Manager.Exists(model);
        if (existsResult is not { Success: true }) { return StatusCode(500, ApiResult<TModel>.From(existsResult)); }

        // Add or update based on entity existence
        if (existsResult.Value)
        {
            // Entity exists, update
            var updateResult = await Manager.Update(model);
            ApiResult<TModel> result = ApiResult<TModel>.From(updateResult);
            result.Value = model;

            ApiResultDto<TModel> dto = new(result);

            if (result.Success)
            {
                return AcceptedAtAction(nameof(Post), new { id = model.Id }, dto);
            }

            var classified = (updateResult as ClassifiedResult)?.ClassifiedError;
            return BuildWriteFailureResponse(classified, dto, reasons => dto.FailureReasons = reasons);
        }
        else
        {
            // model does NOT exist, insert
            var addResult = await Manager.Add(model);
            ApiResult<TModel> result = ApiResult<TModel>.From(addResult);
            result.Value = model;

            ApiResultDto<TModel> dto = new(result);

            if (result.Success)
            {
                return CreatedAtAction(nameof(Post), new { id = model.Id }, dto);
            }

            var classified = (addResult as ClassifiedResult<TModel>)?.ClassifiedError;
            return BuildWriteFailureResponse(classified, dto, reasons => dto.FailureReasons = reasons);
        }
    }

    [HttpDelete("{id:long}")]
    public virtual async Task<ActionResult<ApiResultDto>> Delete(long id)
    {
        var result = await Manager.Delete(id);
        var dto = new ApiResultDto(ApiResult.From(result));

        if (result.Success)
        {
            return Ok(dto);
        }

        var classified = (result as ClassifiedResult)?.ClassifiedError;
        return BuildWriteFailureResponse(classified, dto, reasons => dto.FailureReasons = reasons);
    }

    /// <summary>
    /// Maps a write failure to an HTTP response. When <paramref name="classified"/>
    /// is non-null the category drives the status code and <paramref name="setFailureReasons"/>
    /// replaces the wire-level <c>FailureReasons</c> with a single
    /// <see cref="StructuredResultMessage"/>. Otherwise the failure is logged with
    /// a correlation id (server-side only) and returns <c>500</c>.
    /// </summary>
    private ActionResult BuildWriteFailureResponse(
        ClassifiedDbError? classified,
        object dto,
        Action<ResultMessage[]> setFailureReasons)
    {
        if (classified is not null)
        {
            setFailureReasons([StructuredResultMessage.FromError(classified)]);
            return StatusCode(DbErrorStatusMapping.MapCategoryToStatus(classified.Category), dto);
        }

        LogUnclassifiedFailure(dto);
        return StatusCode(500, dto);
    }

    /// <summary>
    /// Logs an unclassified write failure with a generated correlation id. The
    /// id is intentionally not surfaced on the response body — it lets ops
    /// trace the request without leaking internals to the caller.
    /// </summary>
    private void LogUnclassifiedFailure(object dto)
    {
        var loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>();
        if (loggerFactory is null) return;

        var logger = loggerFactory.CreateLogger(GetType());
        var correlationId = Guid.NewGuid();
        logger.LogError(
            "Unclassified write failure on {Controller}. CorrelationId={CorrelationId}. DtoType={DtoType}",
            GetType().Name, correlationId, dto.GetType().Name);
    }

    /// <summary>
    /// Override in derived classes for entity-specific search
    /// </summary>
    protected virtual Expression<Func<TEntity, bool>> BuildSearchPredicate(string? search) 
        => e => true;

    /// <summary>
    /// Add a sort expression using nameof() for type safety
    /// </summary>
    protected void AddSortExpression(string propertyName, Expression<Func<TEntity, object>> expression)
    {
        SortExpressions[propertyName] = expression;
    }

    /// <summary>
    /// Get sort expression by property name
    /// </summary>
    protected virtual Expression<Func<TEntity, object>>? GetSortExpression(string? sort) 
    {
        if (string.IsNullOrEmpty(sort))
            return null;
                
        return SortExpressions.TryGetValue(sort, out var expression) ? expression : null;
    }
}