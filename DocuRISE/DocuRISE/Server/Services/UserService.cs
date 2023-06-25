using DocuRISE.Data;
using DocuRISE.Data.Models;
using DocuRISE.Server.Messaging;
using DocuRISE.Server.Services.Contracts;
using DocuRISE.Shared.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Server.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext data;
        private readonly UserManager<ApplicationUser> userManager;

        private readonly IConfiguration configuration;
        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly IEmailSender emailSender;

        public UserService(ApplicationDbContext data, UserManager<ApplicationUser> userManager,
            IConfiguration configuration, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            this.data = data;
            this.userManager = userManager;

            this.configuration = configuration;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
        }

        public async Task<List<UserServiceModel>> GetUsers()
        {
            var users = await data
                .Users
                .Include(u => u.Company)
                .OrderBy(u => u.Company.Name)
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Join(data.UserRoles,
                u => u.Id,
                ur => ur.UserId,
                (u, ur) => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Company,
                    ur.UserId,
                    ur.RoleId,
                })
                .Join(data.Roles, u => u.RoleId, r => r.Id, (u, r) => new UserServiceModel()
                {
                    Id = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    CompanyName = u.Company.Name,
                    Role = r.Name
                })
                .ToListAsync();

            return users;
        }
        private async Task<UserResponceModel> GetUser(string userEmail)
        {
            var user = await data
                .Users
                .Where(u => u.Email == userEmail)
                .Select(u => new UserResponceModel
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CompanyName = u.Company.Name,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<RegisterResult> AddUser(UserRegisterRequestModel model, string managerEmail)
        {
            if (this.data.Users.Any(u => u.Email == model.Email))
            {
                return new RegisterResult { Successful = false, Errors = new List<string> { "This email is alreadt taken." } };
            }

            var company = await GetCompany(model.CompanyName);

            if (company == null)
            {
                company = await CreateCompany(model.CompanyName);
            }

            var newUser = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                CompanyId = company.Id,
                Company = company
            };

            if (model.Password == null)
            {
                model.Password = CreateRandomPassword();
            }

            var password = model.Password;

            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var result = await userManager.CreateAsync(newUser, password);
            await userManager.AddToRoleAsync(newUser, data.Roles.Where(r => r.Id == model.RoleId).FirstOrDefault().ToString());

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);

                return new RegisterResult { Successful = false, Errors = errors };
            }

            await data.SaveChangesAsync();

            var manager = await GetUser(managerEmail);

            await SendEmail(model, manager);

            return new RegisterResult { Successful = true };
        }

        public async Task<LoginResult> Login(UserLoginRequestModel loginModel)
        {
            var result = await signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, false);

            if (!result.Succeeded)
            {
                var errors = new List<string>
                {
                    "Email or password are invalid."
                };
                return new LoginResult { Successful = false, Errors =  errors};
            }

            var user = await signInManager.UserManager.FindByEmailAsync(loginModel.Email);
            var roles = await signInManager.UserManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginModel.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            JwtSecurityToken token = GenerateJWT(claims);

            return new LoginResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) };
        }

        public async Task<ChangePasswordResult> ChangePassword(UserChangePasswordRequestModel model, string userEmail)
        {
            var user = await data.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                var errors = new List<string>
                {
                    "The new passwords do not match! Try again."
                };

                return new ChangePasswordResult { Successful = false, Errors = errors };
            }

            var result = await this.userManager.ChangePasswordAsync(
                user,
                model.OldPassword,
                model.NewPassword);

            await data.SaveChangesAsync();

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);

                return (new ChangePasswordResult { Successful = false, Errors = errors });
            }

            return new ChangePasswordResult { Successful = true };
        }

        public async Task<CompanyServiceModel> GetCompanyByUserEmail(string userEmail)
        {
            var user = await data.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var company = await data
                .Companies
                .Where(c => c.Id == user.CompanyId)
                .Select(c => new CompanyServiceModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .FirstOrDefaultAsync();

            return company;
        }

        public async Task<IEnumerable<RoleServiceModel>> GetAllRoles()
        {
            var roles = await data
                .Roles
                .Select(r => new RoleServiceModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                })
                .ToListAsync();

            return roles;
        }

        private JwtSecurityToken GenerateJWT(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(
                configuration["JwtIssuer"],
                configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return token;
        }

        private async Task<Company> GetCompany(string companyName)
        {
            var company = await data
                .Companies
                .FirstOrDefaultAsync(c => c.Name == companyName);

            return company;
        }

        private async Task<Company> CreateCompany(string companyName)
        {
            var company = new Company()
            {
                Id = Guid.NewGuid().ToString(),
                Name = companyName
            };

            await data.Companies.AddAsync(company);
            await data.SaveChangesAsync();

            return company;
        }

        private async Task SendEmail(UserRegisterRequestModel user, UserResponceModel manager)
        {
            var html = new StringBuilder();
            html.AppendLine($"<h1>Dear Mr/Mrs {user.FirstName} {user.LastName},</h1>");
            html.AppendLine($"<h3>I am sending you a link with email and password to log into our document storage and management application DocuRISE.</h3>");
            html.AppendLine($"<h3>Link: https://localhost:7025/</h3>");
            html.AppendLine($"<h3>Email: {user.Email}</h3>");
            html.AppendLine($"<h3>Password: {user.Password}</h3>");
            html.AppendLine($"<h3>If you encounter any problems, you can contact me for assistance.</h3>");
            html.AppendLine("<br/>");
            html.AppendLine($"<h3>Best regards,</h3>");
            html.AppendLine($"<h3> {manager.FirstName} {manager.LastName},</h3>");
            html.AppendLine($"<h3>{manager.CompanyName},</h3>");
            html.AppendLine($"<h3>Manager,</h3>");
            html.AppendLine($"<h3>Email: {manager.Email}</h3>");

            await this.emailSender.SendEmailAsync("iskra_krasimirova@abv.bg", "DocuRISE Easy document management", $"{user.Email}", "Notification to register in the application DocuRISE", html.ToString());
        }

        private static string CreateRandomPassword(int length = PasswordDefaultLength)
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }

            int digitsCount = 0;
            int lowerLettersCount = 0;
            int upperLettersCount = 0;
            int symbolsCount = 0;

            foreach (char c in chars)
            {
                if (char.IsDigit(c))
                {
                    digitsCount++;
                }
                else if (char.IsLower(c))
                {
                    lowerLettersCount++;

                }
                else if (char.IsUpper(c))
                {
                    upperLettersCount++;
                }
                else
                {
                    symbolsCount++;
                }
            }

            if (digitsCount == 0 || lowerLettersCount == 0 || upperLettersCount == 0 || symbolsCount == 0)
            {
                CreateRandomPassword();
            }

            return new string(chars);
        }
    }
}
