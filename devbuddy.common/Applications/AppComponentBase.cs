using System.Reflection;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.common.Applications
{
    public abstract class AppComponentBase : PageComponentsBase
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected ToastService ToastService { get; set; }
        [Inject] protected DataModelService DataModelService { get; set; }


        protected virtual async Task OnModelChangedAsync() { await InvokeAsync(StateHasChanged); }

        protected virtual void OnModelChanged(string propertyName) { OnModelChangedAsync(); }

        protected override async Task OnAfterRenderAsync(bool firstRender) { if (firstRender) await JSRuntime.InvokeVoidAsync("initializeTooltips"); }

        protected Dictionary<string, object> Tooltip(string text, TooltipPosition position = TooltipPosition.Bottom)
        {
            return string.IsNullOrEmpty(text) ? [] : new Dictionary<string, object>
                                                     {
                                                         { "data-bs-toggle", "tooltip" },
                                                         { "data-bs-placement", position.ToString().ToLower() },
                                                         { "data-bs-title", text }
                                                     };
        }
    }

    public abstract class AppComponentBase<TModel> : AppComponentBase
        where TModel : CustomDataModelBase, new()
    {

        protected TModel Model { get; set; } = new();
        protected bool ModelHasChanged { get; set; } = false;
        [Parameter, EditorRequired] public string ApiKey { get; set; }

        protected override void OnModelChanged(string propertyName)
        {
            ModelHasChanged = true;
            base.OnModelChanged(propertyName);
        }
        protected override Task OnModelChangedAsync()
        {
            ModelHasChanged = true;
            return base.OnModelChangedAsync();
        }

        protected async Task<bool> SaveDataModelAsync()
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace("ApiKey mancante.", ApiKey);
            try
            {
                await DataModelService.SaveChangesAsync(ApiKey, Model);
                ModelHasChanged = false;
                return true;
            }
            catch
            {
                ToastService.Show("Si è verificato un errore durante il salvataggio dei dati.", ToastLevel.Error);
                return false;
            }
        }

        protected async Task LoadDataModelAsync()
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace("ApiKey mancante.", ApiKey);
            Model = await DataModelService.GetDataModelByApiKey<TModel>(ApiKey);
        }
    }

    public enum TooltipPosition
    { Left, Top, Right, Bottom }
}
