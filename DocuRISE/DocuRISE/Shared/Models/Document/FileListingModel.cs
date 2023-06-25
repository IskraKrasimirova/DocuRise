using System.ComponentModel.DataAnnotations;

namespace DocuRISE.Shared.Models.Document
{
    public class FileListingModel
    {
        public string Id { get; set; }

        [Display(Name = "Document Name")]
        public string FileName { get; set; }
        public string CompanyName { get; set; }

        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        public string Status { get; set; }
    }
}
