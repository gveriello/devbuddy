using devbuddy.common.Applications;
using devbuddy.common.Services;
using devbuddy.plugins.CronConverter.Business.Services;
using devbuddy.plugins.CronConverter.Models;
using Microsoft.JSInterop;

namespace devbuddy.plugins.CronConverter
{
    public sealed partial class Index : AppComponentBase<CronExpressionDataModel>
    {
        private string activeTab = "builder";
        private CronExpressionType expressionType = CronExpressionType.Simple;
        private CronExpressionParts expressionParts = new();
        private string currentExpression = "* * * * *";
        private bool isExpressionValid = true;
        private string validationError = "";
        private string expressionDescription = "Runs every minute, every hour, every day of the month, every month, every day of the week";
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

        private void UpdateCurrentExpression()
        {
            if (expressionType == CronExpressionType.Simple)
            {
                currentExpression = expressionParts.ToString();
            }
            else if (expressionType == CronExpressionType.Preset && selectedPresetIndex >= 0 && selectedPresetIndex < presets.Count)
            {
                currentExpression = presets[selectedPresetIndex].Expression;
            }

            expressionDescription = CronService.GetExpressionDescription(currentExpression);
            Model.CurrentExpression = currentExpression;
            ValidateCurrentExpression();
        }

        private void ValidateCurrentExpression()
        {
            isExpressionValid = CronService.ValidateCronExpression(currentExpression);
            validationError = isExpressionValid ? "" : "Invalid cron expression format";
            expressionDescription = CronService.GetExpressionDescription(currentExpression);
        }

        private async Task CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            ToastService.Show("Expression copied to clipboard", ToastLevel.Success);
        }

        private void SimulateExpression()
        {
            ValidateCurrentExpression();

            if (!isExpressionValid)
            {
                ToastService.Show("Invalid cron expression", ToastLevel.Error);
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
                ToastService.Show("Cannot save invalid cron expression", ToastLevel.Error);
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
                ToastService.Show("Please enter a name for the expression", ToastLevel.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(saveExpressionValue) || !CronService.ValidateCronExpression(saveExpressionValue))
            {
                ToastService.Show("Please enter a valid cron expression", ToastLevel.Warning);
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
                ToastService.Show(isEditing ? "Expression updated" : "Expression saved", ToastLevel.Success);

                // Switch to the Saved Expressions tab
                activeTab = "saved";
            }
            catch (Exception ex)
            {
                ToastService.Show($"Error saving expression: {ex.Message}", ToastLevel.Error);
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