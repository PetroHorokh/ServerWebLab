namespace BLL.Common;

public static class AccountLockout
{
    public static readonly DateTimeOffset LockedByAdminUntil = DateTimeOffset.MaxValue;

    public static bool IsLockedByAdmin(DateTimeOffset? lockoutEnd) =>
        lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(100);
}
