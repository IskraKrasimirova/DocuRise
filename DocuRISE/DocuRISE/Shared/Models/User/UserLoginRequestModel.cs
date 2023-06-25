using System.ComponentModel.DataAnnotations;

namespace DocuRISE.Shared.Models.User
{
	public class UserLoginRequestModel
	{
		[Required]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
