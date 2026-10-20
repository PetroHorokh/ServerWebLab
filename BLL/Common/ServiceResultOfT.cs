namespace BLL.Common;

public sealed class ServiceResult<T>
{
    private ServiceResult(ServiceResultStatus status, T? value, string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public ServiceResultStatus Status { get; }

    public T? Value { get; }

    public string? Error { get; }

    public bool Succeeded => Status == ServiceResultStatus.Success;

    public static ServiceResult<T> Success(T value) => new(ServiceResultStatus.Success, value, null);

    public static ServiceResult<T> NotFound() => new(ServiceResultStatus.NotFound, default, null);

    public static ServiceResult<T> Invalid(string error) => new(ServiceResultStatus.Invalid, default, error);

    public static ServiceResult<T> Forbidden(string error) => new(ServiceResultStatus.Forbidden, default, error);
}
