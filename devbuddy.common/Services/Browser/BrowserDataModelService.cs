using System.Text.Json;
using Blazored.LocalStorage;
using devbuddy.common.Exceptions;
using devbuddy.common.Models;
using devbuddy.common.Services.Base;

namespace devbuddy.common.Services.Browser
{
    public class BrowserDataModelService : DataModelServiceBase
    {
        private readonly ILocalStorageService _localStorage;

        public BrowserDataModelService(EncryptionServiceBase encryptionService, ILocalStorageService localStorage) : base(encryptionService)
        {
            _localStorage = localStorage;
        }

        public override async Task SaveChangesAsync()
        {
            if (!_isDirty) return;

            try
            {
//                var content = JsonSerializer.Serialize(_dataModel, _options);
//                var contentCrypted = await _encryptionService.EncryptStringAsync(content);
//                await _localStorage.SetItemAsync("datamodel", contentCrypted);
//#if DEBUG
//                await _localStorage.SetItemAsync("datamodel_debug", content);
//#endif
//                _isDirty = false;
            }
            catch (Exception ex)
            {
                throw new DataModelServiceException("Errore durante il salvataggio dei dati.", ex);
            }
        }

        public override async Task InitializeAsync()
        {
            try
            {
                await LoadOrCreateDataModelAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                throw new DataModelServiceException("Failed to initialize DataModelService", ex);
            }
        }

        private async Task LoadOrCreateDataModelAsync()
        {
            var fromSession = await _localStorage.GetItemAsync<string>("datamodel");
            if (!string.IsNullOrEmpty(fromSession))
            {
                this._dataModel = JsonSerializer.Deserialize<DataModel>(await _encryptionService.DecryptStringAsync(fromSession), _options)!;
            }
            else
            {
                _dataModel = new DataModel();
                _isDirty = true;
                await SaveChangesAsync();
            }
        }
    }
}
