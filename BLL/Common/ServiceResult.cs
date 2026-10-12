namespace BLL.Common;

public enum ServiceResultStatus
{
    Success,
    NotFound,
    Invalid,
}

public sealed class ServiceResult
{
    private static readonly ServiceResult SuccessResult = new(ServiceResultStatus.Success, null);
    private static readonly ServiceResult NotFoundResult = new(ServiceResultStatus.NotFound, null);

    private ServiceResult(ServiceResultStatus status, string? error)
    {
        Status = status;
        Error = error;
    }

    public ServiceResultStatus Status { get; }

    public string? Error { get; }

    public bool Succeeded => Status == ServiceResultStatus.Success;

    public static ServiceResult Success() => SuccessResult;

    public static ServiceResult NotFound() => NotFoundResult;

    public static ServiceResult Invalid(string error) => new(ServiceResultStatus.Invalid, error);
}
