namespace devbuddy.common.Services
{
    public class ClipboardService
    {
        public static event EventHandler<string> OnClipboardTextChanged;
        public static void OnClipboardChanged(string text)
        {
            OnClipboardTextChanged?.Invoke(null, text);
        }
    }
}
