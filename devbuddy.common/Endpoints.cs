namespace devbuddy.common
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

    public static class TokenEndpoints
    {
        private const string CONTROLLER = "token";
        public const string VERIFY_TOKEN = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/verify";
    }

    public static class KeysEndpoints
    {
        private const string CONTROLLER = "keys";
        public const string TEST = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/test";
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

    public static class DataModelEndpoints
    {
        private const string CONTROLLER = "DataModels";

        public const string UPSERT_DATAMODEL = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/upsert";
        public const string GET_DATAMODEL = $"{Endpoints.URI_HUBCONNECT}{CONTROLLER}/get";
    }
}
