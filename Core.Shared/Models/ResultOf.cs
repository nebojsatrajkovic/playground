namespace Core.Shared.Models
{
    public abstract class AResultOf
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class ResultOf<T> : AResultOf
    {
        public T OperationResult { get; set; }

        public ResultOf(T operationResult)
        {
            OperationResult = operationResult;
            Success = true;
        }
    }

    public class ResultOf : AResultOf
    {

    }
}