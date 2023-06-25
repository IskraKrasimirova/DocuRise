using DocuRISE.Data;
using DocuRISE.Data.Enums;
using DocuRISE.Data.Models;
using DocuRISE.Server.Services.Contracts;
using DocuRISE.Shared.Models.Document;
using Microsoft.EntityFrameworkCore;

namespace DocuRISE.Server.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext data;
        private readonly IWebHostEnvironment env;

        public FileService(ApplicationDbContext data, IWebHostEnvironment env)
        {
            this.data = data;
            this.env = env;
        }

        public string ErrorMessage { get; private set; }

        public async Task<int> AddFile(FileUploadModel document)
        {
            var company = await GetCompany(document.CompanyName);

            //if (company == null)
            //{
            //    return ErrorMessage = "Company does not exist.";
            //}

            var newDocument = new Document()
            {
                Company = company,
                CompanyId = company.Id,
                Name = document.FileName,
                Path = document.Path,
                DocumentTypeId = document.DocumentTypeId,
            };

            await data.Documents.AddAsync(newDocument);
            var result = await data.SaveChangesAsync();

            return result;

            //return "You successfully uploaded a file!";
        }

        public async Task<IEnumerable<DocumentTypeServiceModel>> GetDocumentTypes()
        {
            var types = await data
                .DocumentTypes
                .Select(t => new DocumentTypeServiceModel()
                {
                    Id = t.Id,
                    Name = t.Name,
                })
                .ToListAsync();

            return types;
        }

        public async Task SetInvoiceStatusToDone(string fileName)
        {
            var selectedInvoice = await data
                .Documents
                .FirstOrDefaultAsync(d => d.Name == fileName && d.DocumentType.Name == "Invoice" && d.Status == Status.Approved);

            selectedInvoice.Status = Status.Done;

            await data.SaveChangesAsync();
        }

        public async Task ApproveDocument(string fileName)
        {
            var fileToApprove = await GetDocument(fileName);
            fileToApprove.Status = Status.Approved;

            await data.SaveChangesAsync();
        }

        public async Task RejectDocument(string fileName)
        {
            var fileToReject = await GetDocument(fileName);
            fileToReject.Status = Status.Rejected;

            await data.SaveChangesAsync();
        }

        public async Task<Document> GetDocument(string fileName)
        {
            var document = await data
                .Documents
                .FirstOrDefaultAsync(d => d.Name == fileName);

            return document;
        }

        public async Task<Document> GetDocumentById(string id)
        {
            var document = await data
                .Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            return document;
        }

        public IQueryable<Document> GetAllDocuments()
        {
            var documents = data.Documents.AsQueryable();

            return documents;
        }

        public async Task<bool> EditFile(FileEditFormModel fileModel)
        {
            var file = await GetDocumentById(fileModel.Id);

            if (file == null)
            {
                return false;
            }

            file.DocumentNumber = fileModel.DocumentNumber;
            file.Grounds = fileModel.Grounds;
            file.IssueDate = fileModel.IssueDate;
            file.CreatedOn = fileModel.CreatedOn;

            this.data.Documents.Update(file);
            await this.data.SaveChangesAsync();

            return true;
        }

        public IQueryable<Document> ChangeQueryForFacilityAccountant(IQueryable<Document> query, FilterModel filter)
        {
            if (filter.FilteredByStatus == "Done") 
            { 
                query = query.Where(d => d.Status == Status.Done && d.DocumentType.Name == "Invoice"); 
            } 
            else 
            { 
                query = query.Where(d => d.Status == Status.Approved && d.DocumentType.Name == "Invoice"); 
            }

            return query;
        }

        public IQueryable<Document> ChangeQueryFilterByCompanyName(IQueryable<Document> query, FilterModel filter)
        {
            query = query.Where(d => d.Company.Name.ToLower().Contains(filter.FilteredByCompanyName.ToLower()));

            return query;
        }

        public IQueryable<Document> ChangeQueryFilterByFileName(IQueryable<Document> query, FilterModel filter)
        {
            query = query.Where(d => d.Name.Replace(".pdf", "").TrimEnd()
                                .ToLower().Contains(filter.FilteredByFileName.ToLower()));

            return query;
        }

        private async Task<Company> GetCompany(string companyName)
        {
            var company = await data
                .Companies
                .FirstOrDefaultAsync(c => c.Name == companyName);

            return company;
        }
    }
}
