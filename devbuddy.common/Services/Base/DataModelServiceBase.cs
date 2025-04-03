using System.Text.Json;
using devbuddy.common.Applications;
using devbuddy.common.Models;

namespace devbuddy.common.Services.Base
{
    public abstract class DataModelServiceBase : IDisposable
    {
        protected DataModel _dataModel;
        protected bool _isDirty, _isInitialized;
        protected readonly object _lock = new();
        protected EncryptionServiceBase _encryptionService;

        protected readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };


        public DataModelServiceBase(EncryptionServiceBase encryptionService)
        {
            _encryptionService = encryptionService;
            _dataModel = new DataModel();
        }

        public abstract Task InitializeAsync();
        public abstract Task SaveChangesAsync();

        protected async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
                _isInitialized = true;
            }
        }


        #region Sidebar

        public void UpdateSidebar(List<NavItem> sidebarItems)
        {
            _dataModel.SidebarItems = sidebarItems;
            _isDirty = true;
            SaveChangesAsync();
        }
        #endregion

        #region Settings
        public SettingsDataModel GetSettings() => _dataModel.Settings;
        public void UpdateSettings(SettingsDataModel settings)
        {
            _dataModel.Settings = settings;
            _isDirty = true;
            SaveChangesAsync();
        }
        #endregion

        #region Tasks
        public List<TaskBase> GetTasks() => _dataModel.Tasks?.Values?.SelectMany(value => value)?.ToList() ?? [];
        public List<TaskBase> GetTasksByApplicationName(string appName) => _dataModel.Tasks?.FirstOrDefault(row => row.Key == appName).Value ?? [];
        #endregion

        public TCustomDataModel ValueByKey<TCustomDataModel>(string key)
            where TCustomDataModel : CustomDataModelBase, new()
        {
            ArgumentNullException.ThrowIfNull(key);

            lock (_lock)
            {
                if (_dataModel.ApplicationsDataModels.TryGetValue($"{key}({typeof(TCustomDataModel).Name})", out var jsonModel))
                {
                    if (!string.IsNullOrWhiteSpace(jsonModel))
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<TCustomDataModel>(jsonModel, _options)!;
                        }
                        catch
                        {
                            return new TCustomDataModel();
                        }
                    }
                }
                return new TCustomDataModel();
            }
        }

        public (string key, TCustomDataModel value) ValueByType<TCustomDataModel>()
            where TCustomDataModel : CustomDataModelBase, new()
        {
            lock (_lock)
            {
                var pair = _dataModel.ApplicationsDataModels
                    .FirstOrDefault(x => x.Value is TCustomDataModel);

                if (pair is { Key: not null })
                {
                    UpdateLastUsed(pair.Value! as TCustomDataModel);
                    return (pair.Key, pair.Value as TCustomDataModel)!;
                }
                return default;
            }
        }

        public async Task AddOrUpdateAsync<TCustomDataModel>(string key, TCustomDataModel dataModel)
            where TCustomDataModel : CustomDataModelBase, new()
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(dataModel);

            string finalKey = key;

            // Check if exists by key 
            if (_dataModel.ApplicationsDataModels.Keys.Any(row => row.StartsWith(key)))
            {
                finalKey = $"{key}({typeof(TCustomDataModel).Name})";
            }
            else
            {
                var existingByType = ValueByType<TCustomDataModel>();
                if (existingByType != default)
                {
                    finalKey = $"{key}({typeof(TCustomDataModel).Name})";
                }
                else
                {
                    finalKey = $"{key}({typeof(TCustomDataModel).Name})";
                }
            }

            _dataModel.ApplicationsDataModels[finalKey] = JsonSerializer.Serialize(dataModel);
            _isDirty = true;
            await SaveChangesAsync();
        }

        private void UpdateLastUsed<T>(T model) where T : CustomDataModelBase
        {
            model.LastUsed = DateTime.UtcNow;
            _isDirty = true;
        }

        public void Dispose()
        {
            if (_isDirty)
            {
                SaveChangesAsync();
            }
        }
    }
}
