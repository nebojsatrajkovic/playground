namespace Core.Shared.ExceptionHandling.Exceptions
{
    public class CORE_UnauthenticatedException : Exception
    {
        public CORE_UnauthenticatedException(string message, Exception? ex = null) : base(message, ex) { }
    }
}