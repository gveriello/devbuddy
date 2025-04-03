using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace devbuddy.common.Applications
{
    public sealed partial class GlobalUnhadledErrorComponent
    {
        [Parameter] public ErrorBoundary ErrorBoundary { get; set; }
        [Parameter] public Exception Exception { get; set; }
        [Inject] public LoggerService LoggerService { get; set; }
        private bool IsDebug { get; set; } = false;

        protected override Task OnInitializedAsync()
        {
            //chiamata http per inviare l'errore al server
#if DEBUG
            IsDebug = true;
#endif
            return base.OnInitializedAsync();
        }

        private void RecoverError()
        {
            ErrorBoundary?.Recover();
        }
    }
}
