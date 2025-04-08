namespace devbuddy.business.Models
{
    public static class Endpoints
    {
        public const string APP_ID = "64efe054-69d4-463b-8cdf-eaed7cb1828e";
        public const string URI_HUBCONNECT = "https://www.hubconnect.altervista.org/api/";
    }

    public static class AuthEndpoints
    {
        private const string CONTROLLER = "auth";

        public const string LOGIN = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/login"; 
        public const string REGISTER = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/register";
    }

    public static class UserEndpoints
    {
        private const string CONTROLLER = "users";

        public const string GET_USER_DATA = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/getUserData";
    }

    public static class ModulesEndpoints
    {
        private const string CONTROLLER = "modules";

        public const string GET_APP_MODULES = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/getAppModules";
    }
}
