using DocuRISE.Server.Extensions;
using DocuRISE.Server.Services.Contracts;
using DocuRISE.Shared.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static DocuRISE.Common.GlobalConstants;

namespace DocuRISE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = $"{FacilityManagerRoleName}, {CompanyManagerRoleName}")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestModel model)
        {
            if (!this.User.IsFacilityManager() && !this.User.IsCompanyManager())
            {
                return Unauthorized();
            }

            var managerEmail = this.User.Identity.Name;

            if (model == null || managerEmail == null)
            {
                var errors = new List<string>
                {
                    "Data is invalid!"
                };
                return BadRequest(new RegisterResult {Successful = false, Errors = errors});
            }

            var result = await userService.AddUser(model, managerEmail);

            if (!result.Successful)
            {
                var errors = result.Errors;

                return Ok(new RegisterResult { Successful = false, Errors = errors });
            }
            var success = new List<string> { "Registration was successful!" };
            return Ok(new RegisterResult { Successful = true, Success = success });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestModel model)
        {
            if (model == null)
            {
                var errors = new List<string>
                {
                    "Data is invalid!"
                };
                return BadRequest(new RegisterResult { Successful = false, Errors = errors });
            }

            var loginResult = await userService.Login(model);

            if (!loginResult.Successful)
            {
                var errors = new List<string>
                {
                    "Email or password are invalid."
                };
                return BadRequest(new LoginResult { Successful = false, Errors = errors });
            }

            return Ok(new LoginResult { Successful = true, Token = loginResult.Token });
        }

        [HttpPut]
        [Route("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordRequestModel model)
        {
            if (model == null)
            {
                var errors = new List<string>
                {
                    "Data is invalid!"
                };
                return BadRequest(new LoginResult { Successful = false, Errors = errors });
            }

            var userEmail = this.User.Identity?.Name;
            var result = await userService.ChangePassword(model, userEmail);

            if (!result.Successful)
            {
                var errors = result.Errors;

                return Ok(new ChangePasswordResult { Successful = false, Errors = errors });
            }
            var success = new List<string> { "Password change was successful!" };
            return Ok(new ChangePasswordResult { Successful = true, Success = success });
        }

        [HttpGet]
        [Route("roles")]
        [Authorize]
        public async Task<IEnumerable<RoleServiceModel>> GetRoles()
        {
            var roles = await userService.GetAllRoles();

            return roles;
        }

        [HttpGet]
        [Authorize]
        public async Task<List<UserServiceModel>> GetAllUsers()
        {
            var users = await userService.GetUsers();

            if (!this.User.IsFacilityManager())
            {
                var userEmail = this.User.Identity.Name;
                var userCompany = await userService.GetCompanyByUserEmail(userEmail);
                users = users.Where(u => u.CompanyName == userCompany.Name).ToList();
            }

            return users;
        }

        [HttpGet]
        [Route("company")]
        [Authorize]
        public async Task<IActionResult> GetCompanyByUser()
        {
            var userEmail = this.User.Identity?.Name;
            var company = await userService.GetCompanyByUserEmail(userEmail);

            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }
    }
}
