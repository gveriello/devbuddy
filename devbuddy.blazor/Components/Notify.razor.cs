using System.ComponentModel;
using devbuddy.common.ExtensionMethods;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public sealed partial class Notify : IDisposable
    {
        private List<ToastMessage> toasts = new();
        private Dictionary<Guid, Timer> timers = new();
        private ToastPosition currentPosition = ToastPosition.TopRight;

        [Inject]
        private ToastService ToastService { get; set; }

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            ToastService.OnClose += CloseToast;
            ToastService.OnPositionChanged += HandlePositionChanged;
            currentPosition = ToastService.Position;
        }

        private void HandlePositionChanged(ToastPosition newPosition)
        {
            InvokeAsync(() =>
            {
                currentPosition = newPosition;
                StateHasChanged();
            });
        }

        private string GetPositionClass() => currentPosition.AttributeValueOrDefault<DescriptionAttribute, string>(attribute => attribute.Description);

        private async void ShowToast(ToastMessage message)
        {
            await InvokeAsync(() =>
            {
                toasts.Add(message);
                StateHasChanged();
            });

            var timer = new Timer(async _ =>
            {
                await InvokeAsync(() =>
                {
                    CloseToast(message.Id);
                });
            }, null, message.DurationSeconds * 1000, Timeout.Infinite);

            timers[message.Id] = timer;
        }

        private async void CloseToast(Guid id)
        {
            await InvokeAsync(() =>
            {
                var toast = toasts.FirstOrDefault(x => x.Id == id);
                if (toast != null)
                {
                    toasts.Remove(toast);
                    if (timers.ContainsKey(id))
                    {
                        timers[id].Dispose();
                        timers.Remove(id);
                    }
                }
                StateHasChanged();
            });
        }

        private string GetToastClass(ToastLevel level) => $"toast-{level.ToString().ToLower()}";

        public void Dispose()
        {
            if (ToastService != null)
            {
                ToastService.OnShow -= ShowToast;
                ToastService.OnClose -= CloseToast;
                ToastService.OnPositionChanged -= HandlePositionChanged;
            }

            foreach (var timer in timers.Values)
            {
                timer?.Dispose();
            }
        }
    }
}
