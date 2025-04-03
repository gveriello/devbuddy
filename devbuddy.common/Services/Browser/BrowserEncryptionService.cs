using System.Security.Cryptography;
using System.Text;
using devbuddy.common.Services.Base;
using Microsoft.JSInterop;

namespace devbuddy.common.Services.Browser
{
    public class BrowserEncryptionService : EncryptionServiceBase
    {
        private readonly IJSRuntime _jsRuntime;
        public BrowserEncryptionService(DeviceServiceBase deviceService, MemoryCacheService memoryCacheService, IJSRuntime jsRuntime) : base(deviceService, memoryCacheService)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<byte[]> GenerateMasterKey(byte[] _password, byte[]? _salt)
        {
            var passwordBase64 = Convert.ToBase64String(_password);
            var saltBase64 = Convert.ToBase64String(_salt);

            var derivedKeyBase64 = await _jsRuntime.InvokeAsync<string>(
                "cryptoInterop.deriveKey",
                passwordBase64,
                saltBase64,
                10000,
                KeySize / 8);

            return Convert.FromBase64String(derivedKeyBase64);
        }

        /// <summary>
        /// Cripta una stringa
        /// </summary>
        public override async Task<string> EncryptStringAsync(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            await EnsureInitialized();

            try
            {
                // Genera IV usando Crypto.getRandomValues
                var iv = await _jsRuntime.InvokeAsync<byte[]>("cryptoInterop.getRandomBytes", 16);

                // Cripta usando la Web Crypto API
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var cipherTextBase64 = await _jsRuntime.InvokeAsync<string>(
                    "cryptoInterop.encrypt",
                    Convert.ToBase64String(MasterKey),
                    Convert.ToBase64String(iv),
                    Convert.ToBase64String(plainBytes));

                // Combina signature, IV e testo cifrato
                using var msEncrypt = new MemoryStream();
                byte[] signature = Encoding.UTF8.GetBytes("ENCV1");
                await msEncrypt.WriteAsync(signature);
                await msEncrypt.WriteAsync(iv);
                var cipherBytes = Convert.FromBase64String(cipherTextBase64);
                await msEncrypt.WriteAsync(cipherBytes);

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Errore durante la crittografia della stringa", ex);
            }
        }

        /// <summary>
        /// Decripta una stringa
        /// </summary>
        public override async Task<string> DecryptStringAsync(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            await EnsureInitialized();

            try
            {
                byte[] fullCipherBytes = Convert.FromBase64String(cipherText);

                // Verifica signature
                byte[] signature = new byte[5];
                Array.Copy(fullCipherBytes, 0, signature, 0, 5);
                string signatureStr = Encoding.UTF8.GetString(signature);
                if (signatureStr != "ENCV1")
                    throw new CryptographicException("Formato dati non valido");

                // Estrai IV
                byte[] iv = new byte[16];
                Array.Copy(fullCipherBytes, 5, iv, 0, 16);

                // Estrai il testo cifrato
                byte[] encryptedData = new byte[fullCipherBytes.Length - 21];
                Array.Copy(fullCipherBytes, 21, encryptedData, 0, encryptedData.Length);

                // Decripta usando la Web Crypto API
                var plainTextBase64 = await _jsRuntime.InvokeAsync<string>(
                    "cryptoInterop.decrypt",
                    Convert.ToBase64String(MasterKey),
                    Convert.ToBase64String(iv),
                    Convert.ToBase64String(encryptedData));

                var plainBytes = Convert.FromBase64String(plainTextBase64);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Errore durante la decrittografia della stringa", ex);
            }
        }
    }
}