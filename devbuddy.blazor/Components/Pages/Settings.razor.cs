using devbuddy.common;
using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.ExtensionMethods;

namespace devbuddy.blazor.Components.Pages
{
    [DynamicallyLoadable(ModulesItems.Settings)]
    public partial class Settings : AppComponentBase<SettingsDataModel>
    {
        private readonly string _apiKey = ModulesItems.Settings.AttributeValueOrDefault<ModuleKeyAttribute, string>(attr => attr.Key);
        protected override async Task OnModelChangedAsync()
        {
            await DataModelService.SaveChangesAsync(_apiKey, Model);
            await base.OnModelChangedAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            Model = await DataModelService.GetDataModelByApiKey<SettingsDataModel>(_apiKey);
        }
    }
}
