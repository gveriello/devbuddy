using devbuddy.common;
using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages
{
    [DynamicallyLoadable(ModulesItems.Settings)]
    public partial class Settings : AppComponentBase<SettingsDataModel>
    {
        protected override async Task OnModelChangedAsync()
        {
            DataModelService.UpdateSettings(Model);
            await base.OnModelChangedAsync();
        }

        protected override Task OnInitializedAsync()
        {
            Model = DataModelService.GetSettings();
            return base.OnInitializedAsync();
        }
    }
}
