using devbuddy.common.Applications;
using devbuddy.common.Services;
using devbuddy.plugins.CronConverter.Business.Services;
using devbuddy.plugins.CronConverter.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.CronConverter
{
    public sealed partial class Index : AppComponentBase<CronExpressionDataModel>
    {
        private string activeTab = "builder";


        private CronExpressionType _expressionType = CronExpressionType.Simple;
        private CronExpressionType expressionType
        {
            get => _expressionType;
            set
            {
                _expressionType = value;
                UpdateCurrentExpression();
            }
        }

        private CronExpressionParts expressionParts = new();
        private string currentExpression = "* * * * *";
        private bool isExpressionValid = true;
        private string validationError = "";
        private string expressionDescription = "Eseguito ogni minuto, ogni ora, ogni giorno del mese, ogni mese, ogni giorno del weekend.";
        private List<CronScheduleResult> scheduleResults = [];
        private int occurrencesToShow = 5;
        private List<CronPreset> presets = [];
        private int _selectedPresetIndex = 0;
        private int selectedPresetIndex
        {
            get => _selectedPresetIndex;
            set
            {
                if (_selectedPresetIndex != value)
                {
                    _selectedPresetIndex = value;
                    OnPresetSelected();
                }
            }
        }

        // Save dialog fields
        private ModalComponentBase saveModal;
        private string saveExpressionName = "";
        private string saveExpressionValue = "";
        private string saveExpressionDescription = "";
        private bool isEditing = false;
        private SavedExpression expressionBeingEdited;

        // Delete dialog fields
        private ModalComponentBase deleteModal;
        private SavedExpression expressionToDelete;

        protected override void OnInitialized()
        {
            Model = DataModelService.ValueByKey<CronExpressionDataModel>(nameof(CronConverter));
            presets = CronService.GetCommonPresets();

            if (!string.IsNullOrEmpty(Model.CurrentExpression))
            {
                currentExpression = Model.CurrentExpression;
                ValidateCurrentExpression();
            }

            expressionDescription = CronService.GetExpressionDescription(currentExpression);
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                UpdateCurrentExpression();
            }
        }

        private void UpdateCronExpression()
        {
            // Genera l'espressione cron dai valori correnti
            currentExpression = $"{minuteValue} {hourValue} {dayOfMonthValue} {monthValue} {dayOfWeekValue}";

            // Aggiorna la descrizione
            expressionDescription = CronService.GetExpressionDescription(currentExpression);

            // Convalida l'espressione
            ValidateCurrentExpression();
        }

        private string minuteValue = "*";
        private void OnMinuteChange(ChangeEventArgs e)
        {
            minuteValue = e.Value.ToString();
            UpdateCronExpression();
        }

        private string hourValue = "*";
        private void OnHourChange(ChangeEventArgs e)
        {
            hourValue = e.Value.ToString();
            UpdateCronExpression();
        }

        // Metodi simili per gli altri campi
        private string dayOfMonthValue = "*";
        private void OnDayOfMonthChange(ChangeEventArgs e)
        {
            dayOfMonthValue = e.Value.ToString();
            UpdateCronExpression();
        }

        private string monthValue = "*";
        private void OnMonthChange(ChangeEventArgs e)
        {
            monthValue = e.Value.ToString();
            UpdateCronExpression();
        }

        private string dayOfWeekValue = "*";
        private void OnDayOfWeekChange(ChangeEventArgs e)
        {
            dayOfWeekValue = e.Value.ToString();
            UpdateCronExpression();
        }

        private void UpdateCurrentExpression()
        {
            currentExpression = $"{expressionParts.Minute} {expressionParts.Hour} " +
                                $"{expressionParts.DayOfMonth} {expressionParts.Month} {expressionParts.DayOfWeek}";

            expressionDescription = CronService.GetExpressionDescription(currentExpression);
            Model.CurrentExpression = currentExpression;
            ValidateCurrentExpression();

            StateHasChanged();
        }

        private void ValidateCurrentExpression()
        {
            isExpressionValid = CronService.ValidateCronExpression(currentExpression);
            validationError = isExpressionValid ? "" : "Cron expression non valida.";
            expressionDescription = CronService.GetExpressionDescription(currentExpression);
        }

        private async Task CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            ToastService.Show("Copiato negli appunti");
        }

        private void SimulateExpression()
        {
            ValidateCurrentExpression();

            if (!isExpressionValid)
            {
                ToastService.Show("Cron expression non valida.", ToastLevel.Error);
                return;
            }

            scheduleResults = CronService.GetNextOccurrences(currentExpression, occurrencesToShow);
            activeTab = "simulator";
        }

        private void OnPresetSelected()
        {
            if (selectedPresetIndex >= 0 && selectedPresetIndex < presets.Count)
            {
                currentExpression = presets[selectedPresetIndex].Expression;
                expressionDescription = CronService.GetExpressionDescription(currentExpression);
                ValidateCurrentExpression();
            }
        }

        private void ShowSaveDialog()
        {
            ValidateCurrentExpression();

            if (!isExpressionValid)
            {
                ToastService.Show("Non è possibile salvare una cron expression non valida.", ToastLevel.Error);
                return;
            }

            isEditing = false;
            saveExpressionName = "";
            saveExpressionValue = currentExpression;
            saveExpressionDescription = expressionDescription;
            saveModal.Show();
        }

        private async Task SaveExpression()
        {
            if (string.IsNullOrWhiteSpace(saveExpressionName))
            {
                ToastService.Show("Per favore, inserisci un nome valido.", ToastLevel.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(saveExpressionValue) || !CronService.ValidateCronExpression(saveExpressionValue))
            {
                ToastService.Show("Per favore, inserisci una cron expression valida.", ToastLevel.Warning);
                return;
            }

            try
            {
                if (isEditing && expressionBeingEdited != null)
                {
                    // Update existing expression
                    var index = Model.SavedExpressions.IndexOf(expressionBeingEdited);
                    if (index >= 0)
                    {
                        Model.SavedExpressions[index] = new SavedExpression
                        {
                            Name = saveExpressionName,
                            Expression = saveExpressionValue,
                            Description = saveExpressionDescription,
                            CreatedDate = expressionBeingEdited.CreatedDate
                        };
                    }
                }
                else
                {
                    // Add new expression
                    Model.SavedExpressions.Add(new SavedExpression
                    {
                        Name = saveExpressionName,
                        Expression = saveExpressionValue,
                        Description = saveExpressionDescription,
                        CreatedDate = DateTime.Now
                    });
                }

                await DataModelService.AddOrUpdateAsync(nameof(CronConverter), Model);
                ToastService.Show(isEditing ? "Cron expression modificata." : "Cron expression salvata.", ToastLevel.Success);

                // Switch to the Saved Expressions tab
                activeTab = "saved";
            }
            catch (Exception ex)
            {
                ToastService.Show($"Si è verificato un errore durante il salvataggio della cron expression: {ex.Message}", ToastLevel.Error);
            }
        }

        private void LoadExpression(SavedExpression expression)
        {
            isEditing = true;
            expressionBeingEdited = expression;
            saveExpressionName = expression.Name;
            saveExpressionValue = expression.Expression;
            saveExpressionDescription = expression.Description;
            saveModal.Show();
        }

        private void UseExpression(SavedExpression expression)
        {
            currentExpression = expression.Expression;
            expressionType = CronExpressionType.Advanced;
            expressionDescription = CronService.GetExpressionDescription(currentExpression);
            ValidateCurrentExpression();

            // Switch to the Builder tab
            activeTab = "builder";
        }

        private void DeleteExpression(SavedExpression expression)
        {
            expressionToDelete = expression;
            deleteModal.Show();
        }

        private async Task ConfirmDeleteExpression()
        {
            if (expressionToDelete != null)
            {
                Model.SavedExpressions.Remove(expressionToDelete);
                await DataModelService.AddOrUpdateAsync(nameof(CronConverter), Model);
                ToastService.Show("Expression deleted", ToastLevel.Success);
            }
        }

        protected override async Task OnModelChangedAsync()
        {
            await DataModelService.AddOrUpdateAsync(nameof(CronConverter), Model);
            await base.OnModelChangedAsync();
        }
    }
}