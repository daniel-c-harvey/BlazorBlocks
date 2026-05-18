using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Models.Converters;
using Models.InputModels;
using Models.Models;
using MudBlazor;
using NetBlocks.Models;
using Web.ApiClients;
using Web.Errors;

namespace Web.Maintenance.Entities.New;

public partial class NewModelView<TModel, TInputModel, TClient, TClientConfig, TConverter> : ComponentBase
where TModel : class, IModel, new()
where TInputModel : class, IInputModel, new()
where TClient : ModelClient<TModel, TClientConfig>
where TClientConfig : ModelClientConfig
where TConverter : IModelToInputConverter<TModel, TInputModel>
{
    [SupplyParameterFromForm]
    public TInputModel Input { get; set; } = new();

    [Inject]
    public required TClient Client { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Parameter]
    public required string ModelDisplayName { get; set; }

    [Parameter]
    public required string PageRoute { get; set; }

    [Parameter]
    public required RenderFragment<TInputModel> ChildContent { get; set; }

    /// <summary>Set when the API returned 403; rendered as a page-level
    /// <c>MudAlert</c> above the form. Cleared on the next submit.</summary>
    protected string? ForbiddenMessage;

    private EditContext? _editContext;
    private ValidationMessageStore? _serverMessages;
    private bool _submitting;

    public async Task Post(EditContext editContext)
    {
        if (_submitting) return;
        _submitting = true;

        // Lazily bind the store to the form's EditContext on first submission.
        // Blazor owns the EditContext when the form uses Model=; storing it here
        // ensures all submissions share one store so messages don't accumulate.
        if (_editContext != editContext)
        {
            _editContext = editContext;
            _serverMessages = new ValidationMessageStore(editContext);
        }
        _serverMessages!.Clear();
        ForbiddenMessage = null;

        try
        {
            TModel newModel = TConverter.Convert(Input);
            ApiResult<TModel> apiResult = await Client.Update(newModel);
            WriteOutcome<TModel> outcome = Client.LastUpdateOutcome ?? new WriteOutcome<TModel>
            {
                Success = apiResult.Success,
                HttpStatus = apiResult.Success ? 200 : 0,
                Failures = [],
            };

            if (outcome.Success)
            {
                Navigation.NavigateTo($"/{PageRoute}", forceLoad: true);
                return;
            }

            switch (FormErrorPresenter.Classify(outcome))
            {
                case FormErrorPresentation.InlineField:
                    if (!FormErrorPresenter.ApplyFieldFailure(editContext, _serverMessages, outcome))
                    {
                        // Field path didn't resolve on this form — degrade to a toast
                        // rather than swallow the failure.
                        FormErrorPresenter.ShowSnackbar(Snackbar, outcome);
                    }
                    break;

                case FormErrorPresentation.SnackbarError:
                    FormErrorPresenter.ShowSnackbar(Snackbar, outcome);
                    break;

                case FormErrorPresentation.PageAlertWarning:
                    ForbiddenMessage = outcome.Failures.Count > 0
                        ? string.Join(" ", outcome.Failures.Select(f => f.Message))
                        : "You do not have permission to perform this action.";
                    break;

                case FormErrorPresentation.ServerErrorModal:
                    await ShowServerErrorModalAsync();
                    break;
            }
        }
        finally
        {
            _submitting = false;
        }
    }

    private async Task ShowServerErrorModalAsync()
    {
        // The 500 path keeps the existing ModelSubmittedModal — only the body
        // copy changes. The modal renders Result.Messages as bullets, so we
        // populate it with a single message containing the reference id.
        // The id is generated client-side: the EP.3 server logs its own
        // correlation id but does not surface it on the wire today. Users can
        // quote this id; ops correlates by timestamp + endpoint.
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
