using Models.Shared.Common;
using Models.Shared.Models;
using Web.ApiClients;
using Web.Errors;

namespace Web.Maintenance.Entities;

public class ModelPageViewModel<TModel, TClient, TClientConfig> : IModelPageViewModel<TModel>
    where TModel : class, IModel, new()
    where TClient : ModelClient<TModel, TClientConfig>
    where TClientConfig : ModelClientConfig
{
    public PagedResult<TModel>? Page { get; private set; }
    public NetBlocks.Models.Result? ErrorResults { get; set; }

    public string SearchTerm => _currentSearchTerm;
    
    protected readonly TClient Client;
    private int _currentPage = 0;
    private int _currentPageSize = 0;
    private string _currentSearchTerm = string.Empty;
    private int? _cachedPageCount;

    public ModelPageViewModel(TClient client)
    {
        Client = client;
    }

    public async Task<(int, int)> SetPage(int pageNumber, int pageSize, string searchTerm, bool refresh = false)
    {        
        // Avoid unnecessary API calls if we're requesting the same page
        if (!refresh && _currentPage == pageNumber && _currentPageSize == pageSize && 
            searchTerm == _currentSearchTerm && Page != null)
        {
            return (pageNumber, pageSize);
        }

        int pageCount = 1;
        if (Page is null || _cachedPageCount is null)
        {
            var countResult = await Client.GetPageCount(
                new PagedQuery 
                { 
                    PageSize = pageSize, 
                    Search = searchTerm 
                });
            if (!countResult.Success || countResult.Value is null) 
            {
                ErrorResults = NetBlocks.Models.Result.From(countResult);
            }
            else
            {
                pageCount = countResult.Value.Count;
                _cachedPageCount = pageCount;
            }
        }
        else
        {
            pageCount = _cachedPageCount.Value;
        }

        if (pageNumber < 1 || pageNumber > pageCount)
        {
            pageNumber = 1;
        }

        var result = await Client.GetByPage(
            new PagedQuery 
            { 
                Page = pageNumber, 
                PageSize = pageSize, 
                Search = searchTerm 
            });
        if (result.Success && result.Value != null)
        {
            Page = result.Value;
            _currentSearchTerm = searchTerm;
            _currentPage = pageNumber;
            _currentPageSize = pageSize;
            ErrorResults = null;
        }
        else
        {
            ErrorResults = NetBlocks.Models.Result.From(result);
        }

        return (pageNumber, pageSize);
    }

    public void ClearCache()
    {
        _cachedPageCount = null;
        Page = null;
        _currentSearchTerm = string.Empty;
        _currentPage = 0;
        _currentPageSize = 0;
    }

    public virtual async Task UpdateItem(TModel model)
    {
        var result = await Client.Update(model);

        ErrorResults = result.Success ? null : NetBlocks.Models.Result.From(result);
    }
    public virtual async Task DeleteItem(TModel model)
    {
        var result = await Client.Delete(model);

        ErrorResults = result.Success ? null : NetBlocks.Models.Result.From(result);
    }

    public virtual async Task<WriteOutcome<TModel>> SubmitWithOutcome(TModel model)
    {
        await Client.Update(model);
        return Client.LastUpdateOutcome ?? new WriteOutcome<TModel>
        {
            Success = false,
            HttpStatus = 0,
        };
    }

    public virtual async Task<WriteOutcome<TModel>> DeleteWithOutcome(TModel model)
    {
        await Client.Delete(model);
        return Client.LastDeleteOutcome ?? new WriteOutcome<TModel>
        {
            Success = false,
            HttpStatus = 0,
        };
    }
}