using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Documents = new HashSet<Document>();
        }

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(FirstNameMaxLength)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(LastNameMaxLength)]
        public string LastName { get; set; }

        [Required]
        [ForeignKey(nameof(Company))]
        public string CompanyId { get; set; }
        public Company Company { get; set; }

        public ICollection<Document> Documents { get; set; }
    }
}
