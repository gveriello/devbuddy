using System.Security.Cryptography;
using System.Text;
using devbuddy.common.Services.Base;

namespace devbuddy.common.Services.Desktop
{
    public class DesktopEncryptionService : EncryptionServiceBase
    {
        public DesktopEncryptionService(DeviceServiceBase deviceService, MemoryCacheService memoryCacheService) : base(deviceService, memoryCacheService)
        {
        }

        protected override async Task<byte[]> GenerateMasterKey(byte[] _password, byte[]? _salt)
        {
            byte[] passwordBytes = _password;
            byte[] saltBytes = _salt;

            using var deriveBytes = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 10000);

            return deriveBytes.GetBytes(KeySize / 8);
        }

        /// <summary>
        /// Cripta una stringa
        /// </summary>
        public override async Task<string> EncryptStringAsync(string toEncrypt)
        {
            if (string.IsNullOrEmpty(toEncrypt))
                return string.Empty;

            await EnsureInitialized();

            try
            {
                byte[] iv = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = MasterKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var msEncrypt = new MemoryStream();
                // Scrivi la signature
                byte[] signature = Encoding.UTF8.GetBytes("ENCV1");
                msEncrypt.Write(signature, 0, signature.Length);
                // Scrivi l'IV
                msEncrypt.Write(iv, 0, iv.Length);

                using (var encryptor = aes.CreateEncryptor())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(toEncrypt);
                }

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
        public override async Task<string> DecryptStringAsync(string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt))
                return string.Empty;

            await EnsureInitialized();
            try
            {
                byte[] fullCipherBytes = Convert.FromBase64String(toDecrypt);

                // Verifica la signature
                byte[] signature = new byte[5];
                Array.Copy(fullCipherBytes, 0, signature, 0, 5);
                string signatureStr = Encoding.UTF8.GetString(signature);
                if (signatureStr != "ENCV1")
                    throw new CryptographicException("Formato dati non valido");

                // Estrai l'IV
                byte[] iv = new byte[16];
                Array.Copy(fullCipherBytes, 5, iv, 0, 16);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = MasterKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var msDecrypt = new MemoryStream(fullCipherBytes, 21, fullCipherBytes.Length - 21);
                using var decryptor = aes.CreateDecryptor();
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Errore durante la decrittografia della stringa", ex);
            }
        }
    }
}