namespace devbuddy.common.Services.Base
{
    public abstract class EncryptionServiceBase : IAsyncDisposable
    {
        protected byte[] MasterKey;
        protected const int KeySize = 256;
        protected const int BlockSize = 128;
        private bool _disposed, _isInitialized;
        private readonly DeviceServiceBase _deviceService;
        private readonly MemoryCacheService _memoryCacheService;

        public EncryptionServiceBase(DeviceServiceBase deviceService, MemoryCacheService memoryCacheService)
        {
            _deviceService = deviceService;
            _memoryCacheService = memoryCacheService;
        }

        protected async Task InitializeAsync()
        {
            if (_isInitialized) return;

            if (!_memoryCacheService.TryGetValueIfIsNotExpired(nameof(MasterKey), out MasterKey))
            {
                var _password = _deviceService.GetDevicePassword();
                var _salt = _deviceService.GetSalt();
                MasterKey = await GenerateMasterKey(_password, _salt);
                _memoryCacheService.AddOrUpdate(nameof(MasterKey), MasterKey);
            }

            _isInitialized = true;
        }

        protected async Task EnsureInitialized()
        {
            if (!_isInitialized)
                await InitializeAsync();
        }

        protected abstract Task<byte[]> GenerateMasterKey(byte[] _password, byte[]? _salt);

        /// <summary>
        /// Cripta una stringa
        /// </summary>
        public abstract Task<string> EncryptStringAsync(string toEncrypt);

        /// <summary>
        /// Decripta una stringa
        /// </summary>
        public abstract Task<string> DecryptStringAsync(string toDecrypt);

        /// <summary>
        /// Esporta la chiave master
        /// </summary>
        public string ExportKey()
        {
            return Convert.ToBase64String(MasterKey);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                Array.Clear(MasterKey, 0, MasterKey.Length);
                _disposed = true;
            }
        }
    }
}