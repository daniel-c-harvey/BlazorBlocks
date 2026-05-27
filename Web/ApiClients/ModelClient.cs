using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Models.Common;
using Models.Models;
using NetBlocks.Models;
using Web.Errors;

namespace Web.ApiClients;

public abstract class ModelClient<TModel, TConfig> : ApiClient<TConfig>, IModelClient<TModel>
where TModel : class, IModel, new()
where TConfig : ModelClientConfig
{
    protected readonly JsonSerializerOptions Options;

    /// <summary>
    /// The structured outcome of the most recent <see cref="Update"/> call on
    /// this instance — HTTP status plus parsed <c>{code, field, message}</c>
    /// failures. Populated synchronously by <see cref="Update"/> before it
    /// returns so callers can read it immediately after awaiting. Per-call,
    /// per-instance state: do not interleave <c>Update</c> calls on a single
    /// client (Blazor scopes one client per circuit / page so this is safe in
    /// the existing topology).
    /// </summary>
    public WriteOutcome<TModel>? LastUpdateOutcome { get; private set; }

    /// <summary>
    /// As <see cref="LastUpdateOutcome"/> but for the most recent
    /// <see cref="Delete"/> call. <c>Value</c> is unused.
    /// </summary>
    public WriteOutcome<TModel>? LastDeleteOutcome { get; private set; }

    protected ModelClient(TConfig config, IOptions<JsonSerializerOptions> options) : base(config)
    {
        Options = options.Value;
    }

    public virtual async Task<ApiResult<TModel>> GetById(long id)
    {
        try
        {
            var dtoResult = await http.GetFromJsonAsync<ApiResultDto<TModel>>($"api/{config.ControllerName}/{id}", Options)
                ?? throw new HttpRequestException("Failed to deserialize response");
            
            return dtoResult.From();
        }
        catch (Exception e)
        {
            return ApiResult<TModel>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ApiResult<IEnumerable<TModel>>> GetAll()
    {
        try
        {
            var uri = $"api/{config.ControllerName}/all";
            var result = await http.GetFromJsonAsync<ApiResultDto<IEnumerable<TModel>>>(uri, Options)
                         ?? throw new HttpRequestException("Failed to deserialize response");

            return result.From();
        }
        catch (Exception e)
        {
            return ApiResult<IEnumerable<TModel>>.CreateFailResult(e.Message);
        }
    }

    public async Task<ApiResult<ItemCount>> GetCount()
    {
        try
        {
            var uri = $"api/{config.ControllerName}/count";
            var result = await http.GetFromJsonAsync<ApiResultDto<ItemCount>>(uri, Options)
                         ?? throw new HttpRequestException("Failed to deserialize response");

            return result.From();
        }
        catch (Exception e)
        {
            return ApiResult<ItemCount>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ApiResult<PagedResult<TModel>>> GetByPage(PagedQuery query)
    {
        try
        {
            var queryMap = new Dictionary<string, string?>
            {
                { nameof(query.Page).ToLower(), query.Page.ToString() },
                { nameof(query.PageSize).ToLower(), query.PageSize.ToString() },
                { nameof(query.Search).ToLower(), query.Search },
                { nameof(query.Sort).ToLower(), query.Sort },
                { nameof(query.Desc).ToLower(), query.Desc.ToString() }
            };
            
            var uri = QueryHelpers.AddQueryString($"api/{config.ControllerName}", queryMap);
            
            var result = await http.GetFromJsonAsync<ApiResultDto<PagedResult<TModel>>>(uri, Options)
                   ?? throw new HttpRequestException("Failed to deserialize response");

            return result.From();
        }
        catch (Exception e)
        {
            return ApiResult<PagedResult<TModel>>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ApiResult<ItemCount>> GetPageCount(PagedQuery query)
    {
        try
        { 
            var queryMap = new Dictionary<string, string?>
                {
                    { nameof(query.Page).ToLower(), query.Page.ToString() },
                    { nameof(query.PageSize).ToLower(), query.PageSize.ToString() },
                    { nameof(query.Search).ToLower(), query.Search },
                    { nameof(query.Sort).ToLower(), query.Sort },
                    { nameof(query.Desc).ToLower(), query.Desc.ToString() }
                };

            var uri = QueryHelpers.AddQueryString($"api/{config.ControllerName}/pagecount", queryMap);

            var result = await http.GetFromJsonAsync<ApiResultDto<ItemCount>>(uri, Options)
                   ?? throw new HttpRequestException("Failed to deserialize response");

            return result.From();
        }
        catch (Exception e)
        {
            return ApiResult<ItemCount>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ApiResult<TModel>> Update(TModel model)
    {
        // Reset before each call so a transport failure cannot leak the
        // previous call's outcome to the caller.
        LastUpdateOutcome = null;
        try
        {
            var response = await http.PostAsJsonAsync($"api/{config.ControllerName}", model, Options);
            if (response == null) throw new HttpRequestException(HttpRequestError.InvalidResponse);

            // Read once as a string so we can parse twice: structured outcome
            // for the UI policy, and the existing ApiResultDto<T> shape for
            // legacy callers. Two parses on a small payload; no second I/O.
            var body = await response.Content.ReadAsStringAsync();
            var dto = body.Length == 0
                ? null
                : JsonSerializer.Deserialize<ApiResultDto<TModel>>(body, Options);
            var legacy = dto?.From() ?? ApiResult<TModel>.CreateFailResult("Failed to deserialize response");

            LastUpdateOutcome = BuildOutcome<ApiResult<TModel>>(legacy, response.IsSuccessStatusCode, (int)response.StatusCode, body);
            return legacy;
        }
        catch (Exception e)
        {
            LastUpdateOutcome = new WriteOutcome<TModel>
            {
                Success = false,
                HttpStatus = 0,
                Failures = [new StructuredFailure(e.Message, null, null)],
            };
            return ApiResult<TModel>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ApiResult> Delete(TModel model)
    {
        LastDeleteOutcome = null;
        try
        {
            var response = await http.DeleteAsync($"api/{config.ControllerName}/{model.Id}");
            if (response == null) throw new HttpRequestException(HttpRequestError.InvalidResponse);

            var body = await response.Content.ReadAsStringAsync();
            var dto = body.Length == 0
                ? null
                : JsonSerializer.Deserialize<ApiResultDto>(body, Options);
            var legacy = dto?.From() ?? ApiResult.CreateFailResult("Failed to deserialize response");

            LastDeleteOutcome = BuildOutcome<ApiResult>(legacy, response.IsSuccessStatusCode, (int)response.StatusCode, body, valueOverride: null);
            return legacy;
        }
        catch (Exception e)
        {
            LastDeleteOutcome = new WriteOutcome<TModel>
            {
                Success = false,
                HttpStatus = 0,
                Failures = [new StructuredFailure(e.Message, null, null)],
            };
            return ApiResult.CreateFailResult(e.Message);
        }
    }

    /// <summary>
    /// Convert a parsed legacy result plus the raw response into a
    /// <see cref="WriteOutcome{TModel}"/>. The structured fields
    /// (<c>code</c>, <c>field</c>) live on the wire JSON but get dropped by
    /// the base-class deserializer — so we re-read them from the raw body
    /// as a <see cref="JsonNode"/>.
    /// </summary>
    private static WriteOutcome<TModel> BuildOutcome<TResult>(
        TResult legacy,
        bool isSuccess,
        int statusCode,
        string body) where TResult : ResultBase<TResult>, new()
        => BuildOutcome<TResult>(legacy, isSuccess, statusCode, body, valueOverride: GetLegacyValue(legacy));

    private static WriteOutcome<TModel> BuildOutcome<TResult>(
        TResult legacy,
        bool isSuccess,
        int statusCode,
        string body,
        TModel? valueOverride) where TResult : ResultBase<TResult>, new()
    {
        var failures = FailureReasonParser.Parse(body).ToList();

        // The legacy parse already carries Message text per failure reason.
        // If the wire payload lacked structured fields entirely (e.g. plain
        // ResultMessage from a non-EP.3 endpoint), fall back to that so the
        // UI still has something to show.
        if (failures.Count == 0 && !isSuccess && legacy.Messages is { } messages)
        {
            failures = messages
                .Select(m => new StructuredFailure(m.Message ?? string.Empty, null, null))
                .ToList();
        }

        return new WriteOutcome<TModel>
        {
            Success = isSuccess && legacy.Success,
            HttpStatus = statusCode,
            Value = valueOverride,
            Failures = failures,
        };
    }

    private static TModel? GetLegacyValue<TResult>(TResult legacy) where TResult : ResultBase<TResult>, new()
        => legacy is ApiResult<TModel> typed ? typed.Value : null;

}