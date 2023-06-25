using DocuRISE.Shared.Models.Document;

namespace DocuRISE.Shared.Models.User
{
    public class UserServiceModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CompanyName { get; set; }

        public string Role { get; set; }

        public string Email { get; set; }

        public ICollection<FileListingModel> Documents { get; set; }
    }
}
