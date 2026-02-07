using System;

namespace TenantOrdersLab.App.Common
{
    /// <summary>
    /// Minimal Result type for application use cases.
    /// Keeps handlers explicit and interview-friendly.
    /// </summary>
    public sealed class Result
    {
        private Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public string? Error { get; }

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
    }

    public sealed class Result<T>
    {
        private Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(string error) => new(false, default, error);
    }
}
