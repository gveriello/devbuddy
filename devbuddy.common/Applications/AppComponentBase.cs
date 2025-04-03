using devbuddy.common.Services;
using devbuddy.common.Services.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.common.Applications
{
    public abstract class AppComponentBase : PageComponentsBase
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected ToastService ToastService { get; set; }
        [Inject] protected DataModelServiceBase DataModelService { get; set; }
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
    }

    public enum TooltipPosition
    { Left, Top, Right, Bottom }
}
