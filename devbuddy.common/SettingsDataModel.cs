using devbuddy.common.Applications;

namespace devbuddy.common
{
    public class SettingsDataModel : CustomDataModelBase
    {
        public bool StartOnWindowsStartup { get; set; } = true;
        public bool AuthorizeAnonymousSend { get; set; } = true;
        public bool AuthorizeNotifications { get; set; } = true;
    }
}
