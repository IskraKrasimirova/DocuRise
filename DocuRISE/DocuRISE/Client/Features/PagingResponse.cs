using DocuRISE.Server.RequestFeatures;
using DocuRISE.Shared.Models.Document;

namespace DocuRISE.Client.Features
{
    public class PagingResponse<T> where T : class
    {
        public List<FileListingModel> Items { get; set; }
        public MetaData MetaData { get; set; }
    }
}
