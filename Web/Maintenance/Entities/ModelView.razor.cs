using Microsoft.AspNetCore.Components;
using Models.Converters;
using Models.Entities;
using Models.InputModels;
using Models.Models;
using MudBlazor;
using Web.Errors;
using Web.Maintenance.Entities.New;

namespace Web.Maintenance.Entities;

[CascadingTypeParameter(nameof(T))]
public partial class ModelView<T, TModel, TEditModal, TViewModel, TConverter>  : ComponentBase
    where T : class, IInputModel, new()
    where TModel : class, IModel, new()
    where TEditModal : IEditModal<T>
    where TViewModel : IModelPageViewModel<TModel>
    where TConverter : IModelToInputConverter<TModel, T>
{
    [SupplyParameterFromQuery]
    public int? Page { get; set; }

    [SupplyParameterFromQuery]
    public int? PageSize { get; set; }

    [SupplyParameterFromQuery]
    public string? SearchTerm { get; set; }

    [Inject]
    public required TViewModel ViewModel { get; set; }

    [Inject]
    public required NavigationManager Nav { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required EditModalSaveContextHolder SaveContextHolder { get; set; }

    [Parameter]
    public required string PageRoute { get; set; }

    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public required string ModelDisplayName { get; set; }

    [Parameter]
    public required RenderFragment Columns { get; set; }

    [Parameter]
    public RenderFragment<T>? EditAction { get; set; }

    [Parameter]
    public RenderFragment<T>? DeleteAction { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = "Search...";

    private MudDataGrid<T>? Grid;
    private bool _updatingParameters = false;
    private bool _forceReload = false;

    protected override async Task OnParametersSetAsync()
    {
        // If parameters are missing, set defaults and update URL
        if (Page == null || PageSize == null || SearchTerm is null)
        {
            UpdatePageWithUrl(Page ?? 1, PageSize ?? 10, SearchTerm ?? string.Empty, replace: true);
            return; // URL change will trigger OnParametersSetAsync again with proper values
        }

        _updatingParameters = true;
        if (Grid != null) await Grid.ReloadServerData();
        _updatingParameters = false;
    }

    private void OnSearchChanged(string searchTerm)
    {
        UpdatePageWithUrl(1, PageSize ?? 10, searchTerm);
    }

    private async Task<GridData<T>> LoadGridServerData(GridState<T> state)
    {
        if (!_updatingParameters) UpdatePageWithUrl(state.Page + 1, state.PageSize, ViewModel.SearchTerm);
        await LoadServerData(Page ?? 1, PageSize ?? 10 , SearchTerm ?? string.Empty);

        return new GridData<T>()
        {
            Items = ViewModel.Page?.Items?.Select(TConverter.Convert) ?? Enumerable.Empty<T>(),
            TotalItems = ViewModel.Page?.TotalCount ?? 0
        };
    }

    private void UpdatePageWithUrl(int page, int pageSize, string searchTerm, bool replace = false)
    {
        var currentPage = Page;
        var currentPageSize = PageSize;
        var currentSearchTerm = ViewModel.SearchTerm;

        if (currentPage == page && currentPageSize == pageSize && currentSearchTerm == searchTerm)
            return;

        var queryParams = new Dictionary<string, object?>
        {
            ["Page"] = page,
            ["PageSize"] = pageSize,
            ["SearchTerm"] = searchTerm
        };

        var queryString = Nav.GetUriWithQueryParameters(queryParams);
        Nav.NavigateTo(queryString, forceLoad: false, replace: replace);
    }

    private async Task LoadServerData(int page, int pageSize, string searchTerm)
    {
        // Ensure ViewModel has the latest data
        (page, pageSize) = await ViewModel.SetPage(page, pageSize, searchTerm, _forceReload);

        // if page was coerced, update again
        if (page != Page || pageSize != PageSize)
        {
            UpdatePageWithUrl(page, pageSize, searchTerm);
        }
    }

    private void NewEntity()
    {
        Nav.NavigateTo($"{PageRoute}/new");
    }

    public async Task EditItem(T inputModel)
    {
        var saveContext = new EditModalSaveContext<T, TModel>(ViewModel, TConverter.Convert);
        SaveContextHolder.Current = saveContext;
        try
        {
            var parameters = new DialogParameters<TEditModal>
            {
                { x => x.Model, inputModel },
            };

            var options = new DialogOptions { FullWidth = true };

            var dialog = await DialogService.ShowAsync<TEditModal>("Edit Item", parameters, options);
            var result = await dialog.Result;
            if (result is null || result.Canceled) return;

            // The modal closes with the saved model on success, or with a
            // WriteOutcome carrying a 500 when the server-error modal needs
            // to fire. Field-level / 422 / 403 failures stay inside the
            // modal and never reach this branch.
            switch (result.Data)
            {
                case T:
                    await RefreshGridData();
                    break;
                case WriteOutcome<object>:
                    await ShowServerErrorModalAsync();
                    break;
            }
        }
        finally
        {
            SaveContextHolder.Current = null;
        }
    }

    public async Task RefreshGridData()
    {
        // Refresh data
        _forceReload = true;
        await (Grid?.ReloadServerData() ?? Task.CompletedTask);
        _forceReload = false;
    }

    public async Task DeleteItem(T inputModel)
    {
        var dialog = await DialogService.ShowAsync<ConfirmDeleteModal>("Delete Item");
        var result = await dialog.Result;
        if (result is not { Canceled: false, Data: bool }) return;

        var outcome = await ViewModel.DeleteWithOutcome(TConverter.Convert(inputModel));
        if (outcome.Success)
        {
            await RefreshGridData();
            return;
        }

        switch (FormErrorPresenter.Classify(outcome))
        {
            case FormErrorPresentation.SnackbarError:
            case FormErrorPresentation.InlineField:
                // Delete has no form to attach an inline message to; both
                // 400/409/422 surface as a toast here.
                FormErrorPresenter.ShowSnackbar(Snackbar, outcome);
                break;
            case FormErrorPresentation.PageAlertWarning:
                // 403 on delete: no page-level form to attach an alert to;
                // degrade to warning toast.
                Snackbar.Add(
                    outcome.Failures.Count > 0
                        ? string.Join(" ", outcome.Failures.Select(f => f.Message))
                        : "You do not have permission to perform this action.",
                    Severity.Warning);
                break;
            case FormErrorPresentation.ServerErrorModal:
                await ShowServerErrorModalAsync();
                break;
        }
    }

    private async Task ShowServerErrorModalAsync()
    {
        var reference = FormErrorPresenter.GenerateClientReference();
        var failureResult = NetBlocks.Models.Result.CreateFailResult(
            $"Something went wrong — reference: {reference}");
        var resultTask = Task.FromResult(failureResult);

        var parameters = new DialogParameters<ModelSubmittedModal>
        {
            { x => x.ResultTask, resultTask },
            { x => x.ModelName, ModelDisplayName },
        };
        var options = new DialogOptions { CloseButton = true, FullWidth = true };

        var dialog = await DialogService.ShowAsync<ModelSubmittedModal>(
            $"Submit {ModelDisplayName} Result", parameters, options);
        await dialog.Result;
    }
}

/// <summary>
/// Bridges <c>EditModelModal</c>'s non-generic save contract to the typed
/// <c>ViewModel.SubmitWithOutcome</c> on the page side. Lives here because
/// the closure needs both the page's generic types and the per-edit
/// converter — pulling it into its own file would force exposing all five
/// type params.
/// </summary>
internal sealed class EditModalSaveContext<T, TModel> : IEditModalSaveContext
    where T : class, IInputModel, new()
    where TModel : class, IModel, new()
{
    private readonly IModelPageViewModel<TModel> _viewModel;
    private readonly Func<T, TModel> _convert;

    public EditModalSaveContext(IModelPageViewModel<TModel> viewModel, Func<T, TModel> convert)
    {
        _viewModel = viewModel;
        _convert = convert;
    }

    public async Task<WriteOutcome<object>> SubmitAsync(object model)
    {
        var typed = (T)model;
        var outcome = await _viewModel.SubmitWithOutcome(_convert(typed));
        // Re-shape TModel-typed outcome to object-typed so the modal layer
        // stays untyped. Value is unused on the edit path.
        return new WriteOutcome<object>
        {
            Success = outcome.Success,
            HttpStatus = outcome.HttpStatus,
            Value = outcome.Value,
            Failures = outcome.Failures,
        };
    }
}
