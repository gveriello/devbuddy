namespace devbuddy.common.Services
{
    public class DevUtilityService
    {
        public HttpClient InjectDevEnvironment(HttpClient request)
        {
#if DEBUG
            request.DefaultRequestHeaders.Add("DevEnvironment", "true");
#endif
            return request;
        }

    }
}
