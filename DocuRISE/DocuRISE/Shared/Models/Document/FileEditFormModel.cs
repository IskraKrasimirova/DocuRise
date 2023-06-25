using System.ComponentModel.DataAnnotations;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Shared.Models.Document
{
    public class FileEditFormModel
    {
        [Required]
        public string Id { get; set; }

        [Display(Name = "Document Number")]
        [Range(0, int.MaxValue)]
        public int? DocumentNumber { get; set; }

        [StringLength(GroundsMaxLength, MinimumLength = GroundsMinLength)]
        public string? Grounds { get; set; }

        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime? IssueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.Date;
    }
}
