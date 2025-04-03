namespace devbuddy.common
{
    public static class SpecialPaths
    {
        private static string DIR_NAME => "DevBuddy";
        private static string CUSTOM_DATA_MODEL => "Customs";
        private static string DATA_MODEL => "data.json";
        private static string DATA_MODEL_DEBUG => "data-debug.json";

        public static string NOTIFICATION_HUB => "devbuddy.NotificationHub.exe";
        private static string APP_DATA => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string BASE_DIR => Path.Combine(APP_DATA, DIR_NAME);
        public static string BASE_DATA_MODEL_JSON => Path.Combine(BASE_DIR, DATA_MODEL);
        public static string BASE_DATA_MODEL_DEBUG_JSON => Path.Combine(BASE_DIR, DATA_MODEL_DEBUG);
    }
}
