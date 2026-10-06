namespace BLL.Common;

public sealed class AccountResult
{
    private static readonly AccountResult SuccessResult = new() { Succeeded = true };
    private static readonly AccountResult LockedOutResult = new() { IsLockedOut = true };

    public bool Succeeded { get; private init; }

    public bool IsLockedOut { get; private init; }

    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static AccountResult Success() => SuccessResult;

    public static AccountResult LockedOut() => LockedOutResult;

    public static AccountResult Failed(params string[] errors) => new() { Errors = errors };
}
