using BLL.Common;
using BLL.Models.Actions;

namespace BLL.Services.Interfaces;

public interface IAccountService
{
    Task<AccountResult> RegisterAsync(RegisterModel model);

    Task<AccountResult> LoginAsync(LoginModel model);

    Task LogoutAsync();
}
