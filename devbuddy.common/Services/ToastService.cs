using System.ComponentModel;
using devbuddy.common.Attributes;
using devbuddy.common.ExtensionMethods;
using Microsoft.JSInterop;

namespace devbuddy.common.Services
{
    public class ToastService
    {
        IJSRuntime jsRuntime;
        public ToastService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        private ToastPosition _position = ToastPosition.TopRight;

        public event Action<ToastMessage> OnShow;
        public event Action<Guid> OnClose;
        public event Action<ToastPosition> OnPositionChanged;

        public ToastPosition Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPositionChanged?.Invoke(value);
                }
            }
        }

        public void Show(string message, ToastLevel level = ToastLevel.Info, int seconds = 5, ToastPosition position = ToastPosition.TopRight)
        {
            this.Position = position;
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Level = level,
                DurationSeconds = seconds
            };

            var soundToReproduce = level.AttributeValueOrDefault<SoundAttribute, string>(attribute => attribute.Name);
            jsRuntime.InvokeVoidAsync("playSystemSound", soundToReproduce);
            OnShow?.Invoke(toast);
        }

        public void CloseToast(Guid id)
        {
            OnClose?.Invoke(id);
        }
    }

    public class ToastMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public ToastLevel Level { get; set; }
        public int DurationSeconds { get; set; }
    }

    public enum ToastLevel
    {
        [Sound("asterisk")]
        Info,
        [Sound("question")]
        Success,
        [Sound("exclamation")]
        Warning,
        [Sound("error")]
        Error
    }

    public enum ToastPosition
    {
        [Description("toast-top-right")]
        TopRight,
        [Description("toast-top-left")]
        TopLeft,
        [Description("toast-bottom-right")]
        BottomRight,
        [Description("toast-bottom-left")]
        BottomLeft,
        [Description("toast-top-center")]
        TopCenter,
        [Description("toast-bottom-center")]
        BottomCenter
    }
}
