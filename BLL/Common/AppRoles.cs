namespace BLL.Common;

public static class AppRoles
{
    public const string Admin = "Admin";

    public const string Member = "Member";

    public const string AdminOrMember = Admin + "," + Member;

    public static readonly IReadOnlyList<string> All = [Admin, Member];
}
