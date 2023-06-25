using DocuRISE.Data.Enums;
using DocuRISE.Data.Models;
using DocuRISE.Server.Extensions;
using DocuRISE.Server.Paging;
using DocuRISE.Server.RequestFeatures;
using DocuRISE.Server.Services.Contracts;
using DocuRISE.Shared.Models.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using DocuRISE.Shared.Models.User;
using static DocuRISE.Common.GlobalConstants;
using DocuRISE.Client.Pages.FileManagment;
using System.Text.Json;

namespace DocuRISE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly IFileService fileService;
        private readonly IUserService userService;

        public FilesController(
            IWebHostEnvironment env,
            IFileService fileService,
            IUserService userService)
        {
            this.env = env;
            this.fileService = fileService;
            this.userService = userService;
        }

        [HttpPost]
        [Route("upload")]
        [Authorize(Roles = FacilityManagerRoleName)]
        public async Task<string> UploadFile([FromForm] IFormFile file)
        {
            if (file != null)
            {
                try
                {
                    var rootPath = env.ContentRootPath;
                    string trustedFileName = Path.GetRandomFileName();
                    var saveLocation = Path.Combine(rootPath, "SavedFiles", trustedFileName);
                    using (var fileStream = new FileStream(saveLocation, FileMode.CreateNew))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var shortenedPath = Path.Combine("SavedFiles", trustedFileName);
                    return shortenedPath;
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500;
                    await Response.WriteAsync("Upload failed");
                }
            }

            return string.Empty;
        }


        [HttpPost]
        [Authorize(Roles = FacilityManagerRoleName)]
        public async Task<IActionResult> AddFile(FileUploadModel document)
        {
            if (document == null)
            {
                return BadRequest("A problem occured with document.");
            }

            var response = await fileService.AddFile(document);

            return Ok(response);
        }



        [HttpGet]
        [Route("documentTypes")]
        public async Task<IEnumerable<DocumentTypeServiceModel>> GetDocumentTypes()
        {
            var types = await fileService.GetDocumentTypes();

            return types;
        }


        [HttpPost]
        [Route("all")]
        public async Task<IActionResult> FilteredFiles([FromQuery] EntityParameters entityParameters, FilterModel filter)
        {
            var query = fileService.GetAllDocuments();

            if (!User.IsFacilityManager() && !User.IsFacilityAccountant())
            {
                query = await ChangeQueryForOtherCompaniesAsync(query);
            }

            if (User.IsFacilityAccountant())
            {
                query = fileService.ChangeQueryForFacilityAccountant(query, filter);
            }

            if (User.IsFacilityManager() || User.IsCompanyManager())
            {
                if (filter.FilteredByStatus != string.Empty)
                {
                    query = query.Where(d => d.Status == (Status)Enum.Parse(typeof(Status), filter.FilteredByStatus));
                }
                if (filter.FilteredByType != string.Empty)
                {
                    query = query.Where(d => d.DocumentType.Name == filter.FilteredByType);
                }
            }

            if (filter.FilteredByFileName != string.Empty)
            {
                query = fileService.ChangeQueryFilterByFileName(query, filter);
            }

            if (filter.FilteredByCompanyName != string.Empty && (User.IsFacilityManager() || User.IsFacilityAccountant()))
            {
                query = fileService.ChangeQueryFilterByCompanyName(query, filter);
            }

            var files = await query.Select(d => new FileListingModel()
            {
                Id = d.Id,
                FileName = d.Name,
                CompanyName = d.Company.Name,
                DocumentType = d.DocumentType.Name,
                Status = d.Status.ToString()
            })
                .OrderBy(on => on.FileName)
                .Skip((entityParameters.PageNumber - 1) * entityParameters.PageSize)
                .Take(entityParameters.PageSize)
                .ToListAsync();

            var files2 = await query.Select(d => new FileListingModel()
            {
                Id = d.Id,
                FileName = d.Name,
                CompanyName = d.Company.Name,
                DocumentType = d.DocumentType.Name,
                Status = d.Status.ToString()
            })
               .ToListAsync();

            var pagedFiles = PagedList<FileListingModel>.ToPagedList(files2.OrderBy(on => on.FileName),
                entityParameters.PageNumber,
                entityParameters.PageSize);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize<MetaData>(pagedFiles.MetaData));

            return Ok(Tuple.Create(pagedFiles,files2));
        }


        private async Task<IQueryable<Document>> ChangeQueryForOtherCompaniesAsync(IQueryable<Document> query)
        {
            var userCompany = await userService.GetCompanyByUserEmail(User.Identity.Name);

            query = query.Where(d => d.Company.Name == userCompany.Name);

            if (User.IsCompanyStaff())
            {
                query = query.Where(d => d.Status == Status.Approved && d.DocumentType.Name == "Contract");
            }

            return query;
        }

        [HttpPost]
        [Route("setDone")]
        [Authorize(Roles = FacilityAccountantRoleName)]
        public async Task<IActionResult> SetInvoiceDone([FromBody] string fileName)
        {
            if (!this.User.IsFacilityAccountant())
            {
                return Unauthorized();
            }

            await fileService.SetInvoiceStatusToDone(fileName);

            return Ok();
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var returnedFile = await fileService.GetDocument(fileName);

            var path = Path.Combine(env.ContentRootPath, returnedFile.Path);

            var memoryStream = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;

            return File(memoryStream, MediaTypeNames.Application.Pdf, Path.GetFileName(path));
        }

        [HttpPost]
        [Route("approve")]
        public async Task<IActionResult> ApproveDocument([FromBody] string fileName)
        {
            await fileService.ApproveDocument(fileName);

            return Ok();
        }

        [HttpPost]
        [Route("reject")]
        public async Task<IActionResult> RejectDocument([FromBody] string fileName)
        {
            await fileService.RejectDocument(fileName);

            return Ok();
        }


        [HttpGet]
        [Route("edit/{id}")]
        [Authorize(Roles = FacilityManagerRoleName)]
        public async Task<IActionResult> EditFile(string id)
        {
            if (!this.User.IsInRole(FacilityManagerRoleName))
            {
                return Unauthorized();
            }

            var file = await fileService.GetDocumentById(id);

            if (file == null)
            {
                var errors = new List<string>
                {
                    "The file not exists!"
                };
                return BadRequest(new EditResult { Successful = false, Errors = errors });
            }

            if (!this.User.IsInRole(FacilityManagerRoleName))
            {
                var errors = new List<string>
                {
                    "Not authorized!"
                };
                return Unauthorized(new EditResult { Successful = false, Errors = errors });
            }

            var fileToEdit = new FileEditFormModel
            {
                Id = file.Id,
                DocumentNumber = file.DocumentNumber,
                Grounds = file.Grounds,
                IssueDate = file.IssueDate,
                CreatedOn = file.CreatedOn,
            };

            return Ok(fileToEdit);
        }

        [HttpPut]
        [Authorize(Roles = FacilityManagerRoleName)]
        public async Task<IActionResult> EditFile(FileEditFormModel fileModel)
        {
            if (fileModel == null)
            {
                var errors = new List<string>
                {
                    "The file not exists!"
                };
                return BadRequest(new EditResult { Successful = false, Errors = errors });
            }

            if (!this.User.IsInRole(FacilityManagerRoleName))
            {
                var errors = new List<string>
                {
                    "Not authorized!"
                };
                return Unauthorized(new EditResult { Successful = false, Errors = errors });
            }

            if (fileModel == null)
            {
                var errors = new List<string>
                {
                    "The file not found!"
                };
                return NotFound(new EditResult { Successful = false, Errors = errors });
            }

            var isEdited = await fileService.EditFile(fileModel);

            if (!isEdited)
            {
                return BadRequest();
            }

            var success = new List<string> { "You successfully editing a file!" };
            return Ok(new EditResult { Successful = true, Success = success});
        }

        [HttpGet]
        [Route("exists/{fileName}")]
        public async Task<IActionResult> CheckIfExists(string fileName)
        {

            var foundFile = await fileService.GetDocument(fileName);

            if (foundFile == null)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("pdf/{fileName}")]
        public async Task<IActionResult> GetPdf(string fileName)
        {
            try
            {
                var returnedFile = await fileService.GetDocument(fileName);

                if (returnedFile == null)
                {
                    return NotFound();
                }

                var path = Path.Combine(env.ContentRootPath, returnedFile.Path);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(path);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
    }
}
