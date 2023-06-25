using System.ComponentModel.DataAnnotations;

namespace DocuRISE.Shared.Models.User
{
    public class UserChangePasswordRequestModel
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmNewPassword { get; set; }
    }
}
