using DocuRISE.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Data.Models
{
    public class Document
    {
        public Document()
        {
            Id = Guid.NewGuid().ToString();
            Notes = new List<Note>();
            Users = new HashSet<ApplicationUser>();
        }

        [Key]
        [Required]
        public string Id { get; set; }

        [ForeignKey(nameof(DocumentType))]
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }

        [Required]
        [Display(Name = "File Name")]
        [MaxLength(DocumentNameMaxLength)]
        public string Name { get; set; }

        [Display(Name = "Document Number")]
        [Range(0, int.MaxValue)]
        public int? DocumentNumber { get; set; }

        [Required]
        [MaxLength(PathMaxLength)]
        public string Path { get; set; }

        [MaxLength(GroundsMaxLength)]
        public string? Grounds { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IssueDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "date")]
        public DateTime? ExpirationDate => IssueDate?.AddYears(ExpirationInYears);

        public Status Status { get; set; } = Status.Pending;

        public IEnumerable<ApplicationUser> Users { get; set; }

        public ICollection<Note> Notes { get; set; }

        [Required]
        [ForeignKey(nameof(Company))]
        public string CompanyId { get; set; }
        public Company Company { get; set; }
    }
}