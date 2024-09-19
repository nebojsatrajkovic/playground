namespace Core.Shared.ExceptionHandling.Exceptions
{
    public class CORE_UnauthenticatedException(string message, Exception? ex = null) : Exception(message, ex)
    {

    }
}