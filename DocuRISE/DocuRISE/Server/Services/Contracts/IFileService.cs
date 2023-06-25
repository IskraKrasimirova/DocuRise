using DocuRISE.Data.Models;
using DocuRISE.Shared.Models.Document;

namespace DocuRISE.Server.Services.Contracts
{
    public interface IFileService
    {
        public Task<int> AddFile(FileUploadModel document);

        public Task<IEnumerable<DocumentTypeServiceModel>> GetDocumentTypes();

        public Task SetInvoiceStatusToDone(string fileName);

        public Task ApproveDocument(string fileName);

        public Task RejectDocument(string fileName);

        public Task<Document> GetDocument(string fileName);

        public Task<Document> GetDocumentById(string id);

        public IQueryable<Document> GetAllDocuments();

        public Task<bool> EditFile(FileEditFormModel fileModel);

        public IQueryable<Document> ChangeQueryForFacilityAccountant(IQueryable<Document> query, FilterModel filter);

        public IQueryable<Document> ChangeQueryFilterByCompanyName(IQueryable<Document> query, FilterModel filter);

        public IQueryable<Document> ChangeQueryFilterByFileName(IQueryable<Document> query, FilterModel filter);
    }
}
