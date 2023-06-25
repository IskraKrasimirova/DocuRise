using System.ComponentModel.DataAnnotations;
using static DocuRISE.Common.DataConstants;

namespace DocuRISE.Data.Models
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Document type")]
        [MaxLength(DocumentTypeNameMaxLength)]
        public string Name { get; set; }

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
