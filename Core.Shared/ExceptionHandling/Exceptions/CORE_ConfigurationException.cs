namespace Core.Shared.ExceptionHandling.Exceptions
{
    public class CORE_ConfigurationException(string message, Exception? ex = null) : Exception(message, ex)
    {

    }
}