namespace Core.Shared.ExceptionHandling.Exceptions
{
    public class CORE_UnauthorizedException(string message, Exception? ex = null) : Exception(message, ex)
    {

    }
}