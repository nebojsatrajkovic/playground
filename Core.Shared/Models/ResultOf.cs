﻿using System.Text.Json.Serialization;

namespace Core.Shared.Models
{
    public enum CORE_OperationStatus
    {
        SUCCESS = 1,
        FAILED,
        ERROR,
        TIMED_OUT
    }

    public abstract class AResultOf
    {
        public CORE_OperationStatus Status { get; set; }
        public string? Message { get; set; }
    }

    public class ResultOf<T> : AResultOf
    {
        public T? OperationResult { get; set; }
        [JsonIgnore]
        public Exception? OccurredException { get; set; }

        public ResultOf(T? operationResult)
        {
            Status = CORE_OperationStatus.SUCCESS;
            OperationResult = operationResult;
            Message = Status.ToString();
        }

        public ResultOf(CORE_OperationStatus status, T? operationResult)
        {
            Status = status;
            OperationResult = operationResult;
            Message = status.ToString();
        }

        public ResultOf(CORE_OperationStatus status, T? operationResult, string message)
        {
            Status = status;
            OperationResult = operationResult;
            Message = message ?? status.ToString();
        }

        public ResultOf(CORE_OperationStatus status)
        {
            Status = status;
            OperationResult = default;
            Message = status.ToString();
        }

        public ResultOf(CORE_OperationStatus status, string message)
        {
            Status = status;
            OperationResult = default;
            Message = message ?? status.ToString();
        }

        public ResultOf(Exception ex)
        {
            Status = CORE_OperationStatus.ERROR;
            OccurredException = ex;
            Message = ex.Message;
        }

        public ResultOf(Exception ex, string message)
        {
            Status = CORE_OperationStatus.ERROR;
            OccurredException = ex;
            Message = message ?? ex.Message;
        }
    }

    public class ResultOf : AResultOf
    {
        public ResultOf(CORE_OperationStatus status)
        {
            Status = status;
            Message = status.ToString();
        }

        public ResultOf(CORE_OperationStatus status, string message)
        {
            Status = status;
            Message = message ?? status.ToString();
        }
    }
}