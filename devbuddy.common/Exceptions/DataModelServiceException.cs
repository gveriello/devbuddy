namespace devbuddy.common.Exceptions
{
    public class DataModelServiceException : Exception
    {
        public DataModelServiceException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
