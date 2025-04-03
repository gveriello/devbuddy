using System.Text.Json;
using devbuddy.common.Exceptions;
using devbuddy.common.Models;
using devbuddy.common.Services.Base;

namespace devbuddy.common.Services.Desktop
{
    public class DesktopDataModelService : DataModelServiceBase
    {
        private readonly string _filePath;
        public DesktopDataModelService(EncryptionServiceBase encryptionService) : base(encryptionService)
        {
            _filePath = SpecialPaths.BASE_DATA_MODEL_JSON;
        }

        public override async Task InitializeAsync()
        {
            try
            {
                EnsureDirectoryExists();
                await LoadOrCreateDataModelAsync();
            }
            catch (Exception ex)
            {
                throw new DataModelServiceException("Failed to initialize DataModelService", ex);
            }
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(SpecialPaths.BASE_DIR))
                Directory.CreateDirectory(SpecialPaths.BASE_DIR);
        }

        private async Task LoadOrCreateDataModelAsync()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var content = File.ReadAllText(_filePath);
                    this._dataModel = string.IsNullOrEmpty(content) ?
                                    new DataModel() :
                                    JsonSerializer.Deserialize<DataModel>(await _encryptionService.DecryptStringAsync(content), _options)!;
                }
                catch
                {
                    File.Delete(_filePath);
                    await LoadOrCreateDataModelAsync();
                }
            }
            else
            {
                _dataModel = new DataModel();
                _isDirty = true;
                await SaveChangesAsync();
            }
        }

        public override async Task SaveChangesAsync()
        {
            if (!_isDirty) return;

            try
            {
                var content = JsonSerializer.Serialize(_dataModel, _options);
                var contentCrypted = await _encryptionService.EncryptStringAsync(content);
                File.WriteAllText(_filePath, contentCrypted);
#if DEBUG
                File.WriteAllText(SpecialPaths.BASE_DATA_MODEL_DEBUG_JSON, content);
#endif
                _isDirty = false;
            }
            catch (Exception ex)
            {
                throw new DataModelServiceException("Errore durante il salvataggio dei dati.", ex);
            }
        }
    }
}
