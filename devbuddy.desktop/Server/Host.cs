using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace devbuddy.Desktop.Server
{
    public class Host(int port = 5000) : IDisposable
    {
        private WebApplication? app;
        private readonly int _port = port;
        private CancellationTokenSource? _cancellationTokenSource;

        public async Task StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.ConfigureServerServices();
            app = builder.Build();

            app.ConfigureDesktopConfiguraton();

            _cancellationTokenSource = new CancellationTokenSource();

            // Avvia il server in background
            await Task.Run(() => app.RunAsync($"http://localhost:{_port}"), _cancellationTokenSource.Token);
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource != null)
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (app != null)
            {
                await app.DisposeAsync();
                app = null;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            app?.DisposeAsync().AsTask().Wait();
        }
    }
}
