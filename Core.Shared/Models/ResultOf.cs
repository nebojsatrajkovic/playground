namespace Core.Shared.Models
{
    public enum CORE_OperationStatus
    {
        SUCCESS,
        ERROR
    }

    public abstract class AResultOf
    {
        public CORE_OperationStatus Status { get; set; }
        public string? Message { get; set; }
    }

    public class ResultOf<T> : AResultOf
    {
        public T? OperationResult { get; set; }

        public ResultOf(T? operationResult)
        {
            Status = CORE_OperationStatus.SUCCESS;
            OperationResult = operationResult;
        }

        public ResultOf(CORE_OperationStatus status, T? operationResult)
        {
            Status = status;
            OperationResult = operationResult;
        }

        public ResultOf(CORE_OperationStatus status, T? operationResult, string message)
        {
            Status = status;
            OperationResult = operationResult;
            Message = message;
        }

        public ResultOf(CORE_OperationStatus status)
        {
            Status = status;
            OperationResult = default;
        }

        public ResultOf(CORE_OperationStatus status, string message)
        {
            Status = status;
            OperationResult = default;
            Message = message;
        }
    }

    public class ResultOf : AResultOf
    {
        public ResultOf()
        {
            Status = CORE_OperationStatus.SUCCESS;
        }

        public ResultOf(CORE_OperationStatus status)
        {
            Status = status;
        }

        public ResultOf(CORE_OperationStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}