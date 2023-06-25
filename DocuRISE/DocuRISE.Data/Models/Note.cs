using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Data.Models
{
    public class Note
    {
        public Note()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        [MaxLength(DocumentNameMaxLength)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Document Number")]
        [Range(0, int.MaxValue)]
        public int? DocumentNumber { get; set; }

        [Required]
        [MaxLength(GroundsMaxLength)]
        public string Grounds { get; set; }

        [Column(TypeName = "date")]
        public DateTime IssueDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime ExpirationDate { get; set; }

        [Column(TypeName = "date")]

        public DateTime DateOfChange { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(DescriptionMaxLength)]
        [Display(Name = "Reasons for change")]
        public string Description { get; set; }

        [Required]
        [ForeignKey(nameof(Document))]
        public string DocumentId { get; set; }
        public Document Document { get; set; }
    }
}
