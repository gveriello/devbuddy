using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace devbuddy.Desktop.Services
{
    public class ClipboardEventArgs(string text) : EventArgs
    {
        public string Text { get; } = text;
        public DateTime Timestamp { get; } = DateTime.Now;
    }

    class ClipboardMonitor : IDisposable
    {
        private static class NativeMethods
        {
            public const int WM_CLIPBOARDUPDATE = 0x031D;

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool AddClipboardFormatListener(nint hwnd);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RemoveClipboardFormatListener(nint hwnd);
        }

        private readonly nint _windowHandle;
        private readonly HwndSource _hwndSource;
        private bool _disposed;

        // Evento che verrà generato quando viene copiato del testo nella clipboard
        public event EventHandler<string> OnClipboardTextChanged;

        public ClipboardMonitor()
        {
            // Creiamo una finestra Win32 per ricevere i messaggi della clipboard
            var parameters = new HwndSourceParameters("ClipboardMonitorWindow")
            {
                WindowStyle = 0,
                ExtendedWindowStyle = 0,
                WindowClassStyle = 0
            };

            _hwndSource = new HwndSource(parameters);
            _windowHandle = _hwndSource.Handle;

            // Registriamo il hook per i messaggi Win32
            _hwndSource.AddHook(WndProc);

            // Registriamo il listener per la clipboard
            if (!NativeMethods.AddClipboardFormatListener(_windowHandle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                try
                {
                    // Eseguiamo il controllo nel thread UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Clipboard.ContainsText())
                        {
                            string text = Clipboard.GetText();
                            OnClipboardTextChanged?.Invoke(this, text);
                        }
                    });
                }
                catch (Exception ex)
                {
                    // Gestiamo eventuali errori di accesso alla clipboard
                    Console.WriteLine($"Errore durante l'accesso alla clipboard: {ex.Message}");
                }
                handled = true;
            }
            return nint.Zero;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Rilasciamo le risorse managed
                    if (_windowHandle != nint.Zero)
                    {
                        NativeMethods.RemoveClipboardFormatListener(_windowHandle);
                    }
                    _hwndSource?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class ClipboardService : IDisposable
    {
        private readonly ClipboardMonitor _monitor;

        public event EventHandler<string> ClipboardTextChanged;

        public ClipboardService()
        {
            _monitor = new ClipboardMonitor();
            _monitor.OnClipboardTextChanged += (sender, text) =>
            {
                ClipboardTextChanged?.Invoke(this, text);
            };
        }

        public void Dispose()
        {
            _monitor?.Dispose();
        }
    }

    public class ClipboardListener : IDisposable
    {
        private readonly ClipboardService _clipboardService;

        public ClipboardListener()
        {
            _clipboardService = new ClipboardService();
            _clipboardService.ClipboardTextChanged += (sender, text) =>
            {
                common.Services.ClipboardService.OnClipboardChanged(text);
            };
        }

        public void Dispose()
        {
            _clipboardService?.Dispose();
        }
    }
}
