using System.ComponentModel.DataAnnotations;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Shared.Models.Document
{
    public class FileUploadModel
    {
        
        [StringLength(DocumentNameMaxLength, MinimumLength =DocumentNameMinLength)]
        [Display(Name ="File Name")]
        public string FileName { get; set; }

        
        [StringLength(PathMaxLength, MinimumLength = PathMinLength)]
        public string Path { get; set; }

        [Required]
        [StringLength(CompanyNameMaxLength, MinimumLength = CompanyNameMinLength)]
        public string CompanyName { get; set; }

        public int DocumentTypeId { get; set; }
    }
}