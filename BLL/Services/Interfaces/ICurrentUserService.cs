namespace BLL.Services.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    bool IsInRole(string role);
}
