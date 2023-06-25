using DocuRISE.Shared.Models.User;

namespace DocuRISE.Server.Services.Contracts
{
    public interface IUserService
    {
        public Task<List<UserServiceModel>> GetUsers();

        public Task<RegisterResult> AddUser(UserRegisterRequestModel model, string managerEmail);

        public Task<LoginResult> Login(UserLoginRequestModel loginModel);

        public Task<ChangePasswordResult> ChangePassword(UserChangePasswordRequestModel model, string userEmail);

        public Task<CompanyServiceModel> GetCompanyByUserEmail(string userEmail);

        public Task<IEnumerable<RoleServiceModel>> GetAllRoles();
    }
}
