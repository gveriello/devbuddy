using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.ExtensionMethods;
using devbuddy.common.Services;
using devbuddy.plugins.YamlFormatter.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace devbuddy.plugins.YamlFormatter
{
    [DynamicallyLoadable(ModulesItems.YamlFormatter)]
    public partial class Index : AppComponentBase<YamlFormatterDataModel>
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private ToastService ToastService { get; set; }

        private string _inputYaml = string.Empty;
        private string _outputYaml = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _autoFormat = true;
        private bool _minify = false;
        private bool _showSavedYamls = false;
        private int _yamlSize = 0;
        private int _yamlLines = 0;
        private int _yamlNodes = 0;
        private string activeTab = "formatter";

        // Modal references and properties
        private ModalComponentBase saveModal;
        private ModalComponentBase deleteModal;
        private string SaveYamlName { get; set; } = string.Empty;
        private string SaveYamlDescription { get; set; } = string.Empty;
        private SavedYaml yamlToDelete;

        public string InputYaml
        {
            get => _inputYaml;
            set
            {
                _inputYaml = value;
                if (AutoFormat && !string.IsNullOrWhiteSpace(_inputYaml))
                {
                    FormatYaml();
                }
            }
        }

        public string OutputYaml
        {
            get => _outputYaml;
            set => _outputYaml = value;
        }

        public bool AutoFormat
        {
            get => _autoFormat;
            set => _autoFormat = value;
        }

        public bool Minify
        {
            get => _minify;
            set => _minify = value;
        }

        public bool ShowSavedYamls
        {
            get => _showSavedYamls;
            set => _showSavedYamls = value;
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value;
        }

        public int YamlSize => _yamlSize;
        public int YamlLines => _yamlLines;
        public int YamlNodes => _yamlNodes;

        protected override async Task OnInitializedAsync()
        {
            ApiKey = ModulesItems.YamlFormatter.AttributeValueOrDefault<ModuleKeyAttribute, string>(attr => attr.Key);
            await LoadDataModelAsync();

            if (!string.IsNullOrEmpty(Model.CurrentYaml))
            {
                InputYaml = Model.CurrentYaml;
            }
        }

        protected override async Task OnModelChangedAsync()
        {
            await SaveDataModelAsync();
            await base.OnModelChangedAsync();
        }

        public void FormatYaml()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(InputYaml))
            {
                OutputYaml = string.Empty;
                _yamlSize = 0;
                _yamlLines = 0;
                _yamlNodes = 0;
                return;
            }

            try
            {
                // Deserializza il YAML per verificare che sia valido
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                // Tentativo di deserializzazione per verifica
                var yamlObject = deserializer.Deserialize<object>(InputYaml);

                // Configurazione del serializzatore in base alle impostazioni
                var serializerBuilder = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance);

                if (Minify)
                {
                    // Per la minificazione: rimuovi gli spazi in eccesso
                    serializerBuilder = serializerBuilder
                        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                        .DisableAliases();
                }
                else
                {
                    // Per la formattazione: usa indentazione standard
                    serializerBuilder = serializerBuilder
                        .WithIndentedSequences();
                }

                var serializer = serializerBuilder.Build();

                // Serializza di nuovo l'oggetto per formattarlo
                OutputYaml = serializer.Serialize(yamlObject);

                // Calcola le statistiche
                _yamlSize = OutputYaml.Length;
                _yamlLines = OutputYaml.Split('\n').Length;
                _yamlNodes = CountYamlNodes(yamlObject);

                // Salva il YAML corrente nel modello
                Model.CurrentYaml = InputYaml;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"YAML non valido: {ex.Message}";
                OutputYaml = string.Empty;
            }
        }

        private int CountYamlNodes(object yamlObject, int depth = 0)
        {
            if (yamlObject == null)
                return 0;

            int count = 1; // Conta il nodo corrente

            if (yamlObject is Dictionary<object, object> dictionary)
            {
                foreach (var kvp in dictionary)
                {
                    count += CountYamlNodes(kvp.Value, depth + 1);
                }
            }
            else if (yamlObject is List<object> list)
            {
                foreach (var item in list)
                {
                    count += CountYamlNodes(item, depth + 1);
                }
            }

            return count;
        }

        public async Task PasteFromClipboard()
        {
            try
            {
                var clipboardText = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
                InputYaml = clipboardText;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nell'accesso alla clipboard: {ex.Message}";
            }
        }

        public void ClearInput()
        {
            InputYaml = string.Empty;
            OutputYaml = string.Empty;
            ErrorMessage = string.Empty;
            _yamlSize = 0;
            _yamlLines = 0;
            _yamlNodes = 0;
        }

        public async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(OutputYaml))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", OutputYaml);
                ToastService.Show("YAML copiato negli appunti", ToastLevel.Success);
            }
        }

        public void ViewSavedYamls()
        {
            ShowSavedYamls = !ShowSavedYamls;
        }

        public void ShowSaveDialog()
        {
            if (string.IsNullOrWhiteSpace(OutputYaml))
            {
                ToastService.Show("Non c'è nulla da salvare. Formatta prima un YAML valido.", ToastLevel.Warning);
                return;
            }

            SaveYamlName = string.Empty;
            SaveYamlDescription = string.Empty;
            saveModal.Show();
        }

        public async Task SaveYaml()
        {
            if (string.IsNullOrWhiteSpace(SaveYamlName))
            {
                ToastService.Show("Inserisci un nome per il YAML", ToastLevel.Warning);
                return;
            }

            try
            {
                Model.SavedYamls.Add(new SavedYaml
                {
                    Name = SaveYamlName,
                    Content = OutputYaml,
                    Description = SaveYamlDescription,
                    CreatedDate = DateTime.Now
                });

                await SaveDataModelAsync();
                ToastService.Show("YAML salvato con successo", ToastLevel.Success);
                ShowSavedYamls = true;
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante il salvataggio: {ex.Message}", ToastLevel.Error);
            }
        }

        public void LoadSavedYaml(SavedYaml yaml)
        {
            InputYaml = yaml.Content;
            ToastService.Show($"YAML '{yaml.Name}' caricato", ToastLevel.Info);
        }

        public void DeleteSavedYaml(SavedYaml yaml)
        {
            yamlToDelete = yaml;
            deleteModal.Show();
        }

        public async Task ConfirmDeleteYaml()
        {
            if (yamlToDelete != null)
            {
                Model.SavedYamls.Remove(yamlToDelete);
                await SaveDataModelAsync();
                ToastService.Show("YAML eliminato", ToastLevel.Success);
            }
        }
    }
}