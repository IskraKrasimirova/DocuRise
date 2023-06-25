using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Data.Models
{
    public class Company
    {
        public Company()
        {
            Documents = new HashSet<Document>();
            Users = new HashSet<ApplicationUser>();
        }

        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        [MaxLength(CompanyNameMaxLength)]
        public string Name { get; set; }

        public IEnumerable<Document> Documents { get; set; }

        public IEnumerable<ApplicationUser> Users { get; set; }
    }
}
