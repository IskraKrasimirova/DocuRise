using DocuRISE.Shared.Models.User;

namespace DocuRISE.Client.Services
{
    public interface IAuthService
    {
        Task<LoginResult> Login(UserLoginRequestModel loginModel);
        Task Logout();
        Task<RegisterResult> Register(UserRegisterRequestModel registerModel);
    }
}