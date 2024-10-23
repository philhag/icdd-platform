using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.PageModels.Container;
using IcddWebApp.PageModels.Error;
using IcddWebApp.Services;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.DTOs;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.Services.Models.Requests;
using IcddWebApp.Services.Models.Update;
using IcddWebApp.WebApplication.Environment;
using IIB.ICDD.Model;
using IIB.ICDD.Model.Container;
using IIB.ICDD.Model.Container.Document;
using IIB.ICDD.Model.Container.ExtendedDocument;
using IIB.ICDD.Model.PayloadTriples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace IcddWebApp.Controllers
{
    [Authorize]
    public class ContainerController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IProjectService _projectService;
        private readonly IContainerService _containerService;
        private readonly IContentService _contentService;
        private readonly ILinksetService _linksetService;
        private readonly IAuthService _authService;
        private readonly IQueryService _queryService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;

        public ContainerController(IWebHostEnvironment environment, IProjectService projectService,
            IContainerService containerService, IContentService contentService, ILinksetService linksetService,
            IAuthService authService, IQueryService queryService, UserManager<User> userManager,
            IConfiguration configuration)
        {
            _environment = environment;
            _projectService = projectService;
            _containerService = containerService;
            _contentService = contentService;
            _linksetService = linksetService;
            _authService = authService;
            _queryService = queryService;
            _userManager = userManager;
            _configuration = configuration;
            _workfolderPath = _configuration["WorkfolderPath"];
        }

        [HttpGet]
        public async Task<IActionResult> Index(string projId, string id, string containerVersion)
        {
            ViewBag.Project = projId;
            ViewBag.Container = id;

            if (string.IsNullOrEmpty(projId) || string.IsNullOrEmpty(id))
            {
                return RedirectToAction("List", "Project");
            }

            var Project = await _projectService.GetProject(projId, User.Identity.Name);
            if (Project == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find project.",
                    new FormatException(), $"~/Project/List"));

            var ContainerMetadata =
                await _containerService.GetContainerMetadata(projId, ContainerType.ICDD, id, containerVersion);
            if (ContainerMetadata == null)
                return RedirectToAction("Index", "Error",
                    new { errorCode = 404, errorTitle = "NotFound", errorMessage = "Container not found" });

            try
            {
                DateTime timeStart = DateTime.Now;
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, containerVersion);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                        "Could not find project containers.",
                        new FormatException(), $"~/Project/List"));

                var model = new ContainerPageModel(Project, ContainerMetadata, Container);
                _ = model.ContainerModelContext.CreateWexbimAsync();
                DateTime timeFinished = DateTime.Now;
                TimeSpan timeSpan = timeFinished - timeStart;
                Logger.Log("Zeitspanne zum Anzeigen des Containers: " + timeSpan.TotalMilliseconds + "ms",
                    Logger.MsgType.Info, "ContainerController.Details(" + Container.ContainerGuid + ")");
                return View(model);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error",
                    new { errorCode = 404, errorTitle = "NotFound", errorMessage = "Container not found. " + e });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MaintenanceDetails(string projId, string id, string containerVersion)
        {
            ViewBag.Project = projId;
            ViewBag.Container = id;

            if (string.IsNullOrEmpty(projId) || string.IsNullOrEmpty(id))
            {
                return RedirectToAction("List", "Project");
            }

            var Project = await _projectService.GetProject(projId, User.Identity.Name);
            if (Project == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find project.",
                    new FormatException(), $"~/Project/List"));

            var ContainerMetadata =
                await _containerService.GetContainerMetadata(projId, ContainerType.ICDD, id, containerVersion);
            if (ContainerMetadata == null)
                return RedirectToAction("Index", "Error",
                    new { errorCode = 404, errorTitle = "NotFound", errorMessage = "Container not Found" });

            try
            {
                DateTime timeStart = DateTime.Now;
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, containerVersion);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                        new FormatException(), $"~/Project/List"));

                var model = new ContainerPageModel(Project, ContainerMetadata, Container);
                _ = model.ContainerModelContext.CreateWexbimAsync();
                DateTime timeFinished = DateTime.Now;
                TimeSpan timeSpan = timeFinished - timeStart;
                Logger.Log("Zeitspanne zum Anzeigen des Maintenance Containers: " + timeSpan.TotalMilliseconds + "ms",
                    Logger.MsgType.Info, "ContainerController.MaintenanceDetails(" + Container.ContainerGuid + ")");
                return View(model);
            }
            catch
            {
                return RedirectToAction("Index", "Error",
                    new { errorCode = 404, errorTitle = "NotFound", errorMessage = "Container not Found" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> InspectionDetails(string projId, string id, string containerVersion)
        {
            ViewBag.Project = projId;
            ViewBag.Container = id;

            if (string.IsNullOrEmpty(projId) || string.IsNullOrEmpty(id))
            {
                return RedirectToAction("List", "Project");
            }

            var Project = await _projectService.GetProject(projId, User.Identity.Name);
            if (Project == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find project.",
                    new FormatException(), $"~/Project/List"));

            var ContainerMetadata =
                await _containerService.GetContainerMetadata(projId, ContainerType.ICDD, id, containerVersion);
            if (ContainerMetadata == null)
                return RedirectToAction("Index", "Error",
                    new { errorCode = 404, errorTitle = "NotFound", errorMessage = "Container not Found" });

            try
            {
                DateTime timeStart = DateTime.Now;
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, containerVersion);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                        new FormatException(), $"~/Project/List"));

                var model = new ContainerPageModel(Project, ContainerMetadata, Container);
                _ = model.ContainerModelContext.CreateWexbimAsync();
                DateTime timeFinished = DateTime.Now;
                TimeSpan timeSpan = timeFinished - timeStart;
                Logger.Log("Zeitspanne: " + timeSpan.TotalMilliseconds + "ms", Logger.MsgType.Info,
                    "InspectionDetails.Details(" + Container.ContainerGuid + ")");
                return View(model);
            }
            catch
            {
                return RedirectToAction("Index", "Error",
                    new { errorCode = 404, errorTitle = "NotFound", errorMessage = "Container not Found" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> Delete(string projId, string id, string containerVersion)
        {
            ViewBag.Project = projId;
            ViewBag.Container = id;

            if (string.IsNullOrEmpty(projId) || string.IsNullOrEmpty(id))
            {
                return RedirectToAction("List", "Project");
            }

            var containerMetadata =
                await _containerService.GetContainerMetadata(projId, ContainerType.ICDD, id, containerVersion);
            var project = await _projectService.GetProject(projId, User.Identity.Name);

            var model = new ContainerDeletePageModel(project, containerMetadata);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(string projId, ContainerMetadataRequest containerMetadataRequest)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("Details", "Project", new { projId });
            }

            await _containerService.PostEmptyContainer(User.Identity.Name, projId, ContainerType.ICDD,
                containerMetadataRequest);

            return RedirectToAction("Details", "Project", new { id = projId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync(string projId, string id, string containerVersion)
        {

            if (string.IsNullOrEmpty(id))
                return RedirectToRoute("default", new { controller = "Project", action = "Details", id = projId });

            var projectStatus =
                await _containerService.DeleteContainer(User.Identity.Name, projId, ContainerType.ICDD, id, containerVersion);

            if (projectStatus)
                return RedirectToRoute("default", new { controller = "Project", action = "Details", id = projId });
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete container.",
                    new InvalidOperationException(), $"~/Project/Details/" + projId));
        }

        [HttpGet]
        public async Task<IActionResult> Upload(string projId)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            var Project = await _projectService.GetProject(projId, User.Identity.Name);
            if (Project == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not upload container.",
                    new FormatException(), $"~/Project/Details/" + projId));
            var model = new ContainerUploadPageModel(Project);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAsync(string projId, IFormFile uploadFile,
            ContainerStatus containerStatus, ContainerSuitability containerSuitability)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            var Project = await _projectService.GetProject(projId, User.Identity.Name);
            if (Project == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find project.",
                    new FormatException(), $"~/Project/Details/" + projId));

            var timeStart = DateTime.Now;
            if (uploadFile?.FileName == null)
            {
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "No file chosen.",
                    new FormatException(), $"~/Project/Details/" + projId));
            }
            else if (!uploadFile.FileName.Split('.').Last().Equals("icdd"))
            {
                return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                    "Your file is not an *.icdd file. Please fix your file or try a different one.",
                    new FormatException(), $"~/Project/Details/" + projId));
            }
            else
            {
                var newContainer = await _containerService.PostContainer(User.Identity.Name, projId, ContainerType.ICDD,
                    new ContainerMetadataFileRequest()
                    { File = uploadFile, Suitability = containerSuitability, Status = containerStatus });
                if (newContainer != null)
                {
                    Logger.Log("Speicherort: " + _workfolderPath + "ms", Logger.MsgType.Info, "UploadFile");
                }
                else
                {
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                        "Cannot upload duplicates. Container already exists.",
                        new FormatException(), $"~/Project/Details/" + projId));
                }
            }

            var timeFinished = DateTime.Now;
            var timeSpan = timeFinished - timeStart;
            Logger.Log("Zeitspanne: " + timeSpan.TotalMilliseconds + "ms", Logger.MsgType.Info, "UploadFile");

            return Redirect("~/Project/Details/" + projId);
        }

        [HttpPost]
        public async Task<IActionResult> NextVersionAsync(string projectId, string containerId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }
            var Project = await _projectService.GetProject(projectId, User.Identity.Name);
            if (Project == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find project.",
                            new FormatException(), $"~/Project/Details/" + projectId));

            var result = await _containerService.PostContainerVersion(User.Identity.Name, projectId, ContainerType.ICDD, containerId, null);
            if (result != null)
                return Redirect("~/Project/Details/" + projectId);
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not create new container version. Please try again.",
                            new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateLinksetAsync(string projId, string id, string containerVersion, string linkset)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(linkset))
            {
                var username = User.Identity.Name;
                var newLinkset = await _linksetService.PostContainerLinkset(username, projId, ContainerType.ICDD, id,
                    containerVersion, linkset);

                if (newLinkset != null)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not create linkset.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Container ID or linkset ID is empty.",
                new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLinksetAsync(string projectId, string containerId, string containerVersion,
            string linksetId, LinksetMetadataUpdate linksetMetadataUpdate)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }
            var username = User.Identity.Name;
            var result = await _linksetService.UpdateContainerLinkset(projectId, ContainerType.ICDD, containerId,
                containerVersion, linksetId, username, linksetMetadataUpdate);

            return result != null ? Ok() : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateInternalDocumentAsync(string projId, string id, string containerVersion,
            IFormFile UploadFile, string selectType, string description, string? schema, string? schemaVersion,
            string? schemaSubset, string? encryptionAlgorithm, string? checksum, string? checksumAlgorithm)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(description) && UploadFile != null)
            {
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, null);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));

                var username = User.Identity.Name;
                CtDocument newDoc = null;

                if (selectType == "encrypted")
                {
                    newDoc = await _contentService.PostContainerContentEncrypted(username, projId, ContainerType.ICDD,
                        id, containerVersion,
                        new EncryptedDocumentRequest(UploadFile, encryptionAlgorithm, schema, schemaVersion,
                            schemaSubset));
                }
                else if (selectType == "secured")
                {
                    newDoc = await _contentService.PostContainerContentSecured(username, projId, ContainerType.ICDD, id,
                        containerVersion,
                        new SecuredDocumentRequest(UploadFile, checksum, checksumAlgorithm, schema, schemaVersion,
                            schemaSubset));

                }
                else if (selectType == "internal")
                {
                    newDoc = await _contentService.PostContainerContentInternal(username, projId, ContainerType.ICDD,
                        id, containerVersion,
                        new InternalDocumentRequest(UploadFile, description, schema, schemaVersion, schemaSubset));
                }
                else
                {
                    return NotFound();
                }

                if (newDoc != null)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not create document.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                "Container ID or description or file is empty.",
                new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequirementDocumentAsync(string projId, string id, string containerVersion,
             string description, string? schema, string? schemaVersion, string? schemaSubset, string filename, string mimeType)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(description))
            {
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, null);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));

                //if(MimeMapping.GetMimeMapping(filename) != mimeType)
                //    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Mime Type of document does not match File Extension",
                //        new FormatException(),
                //        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));

                var username = User.Identity.Name;
                var newDoc = await _contentService.PostContainerContent(username, projId, ContainerType.ICDD,
                    id, containerVersion, new DocumentRequest(description, filename, filename.Split('.').Last(), mimeType, schema, schemaVersion, schemaSubset));

                if (newDoc != null)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not create document.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                "Container ID or description or file is empty.",
                new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpPost]
        public async Task<IActionResult> AddRequestedDocumentAsync(string projectId, string containerId, string containerVersion, string contentId,
            IFormFile uploadFile, string changeTypeString, string newFileExt, string newMimeType)
        {
            var changeType = Convert.ToBoolean(changeTypeString);

            if (string.IsNullOrEmpty(projectId))
                return BadRequest();

            var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
            var content = container.GetDocument(contentId);

            InformationContainer Container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, null);
            if (Container == null)
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                    new FormatException(),
                    $"~/Project/" + projectId + "/Container/" + containerId + "/" + containerVersion + "/Index"));

            var contentType = uploadFile.ContentType;
            var fileExtension = System.IO.Path.GetExtension(uploadFile.FileName);

            if (changeType && (contentType != newMimeType))
                return BadRequest();
            else if (changeType && ("." + newFileExt != fileExtension))
                return BadRequest();
            if ((!changeType) && (content.FileFormat != contentType) && ("." + content.FileType != fileExtension))
                return BadRequest();

            if (!string.IsNullOrEmpty(containerId) && uploadFile != null)
            {
                var username = User.Identity.Name;
                var addDoc = await _contentService.AddRequestedDocument(username, projectId, ContainerType.ICDD, containerId,
                    containerVersion, contentId, uploadFile, changeType, newFileExt, newMimeType);

                if (addDoc != null)
                    return Ok();
                else
                    return BadRequest();
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateExternalDocumentAsync(string projId, string id, string containerVersion,
            string documentUri, string description, string? schema, string? schemaVersion, string? schemaSubset)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(description) &&
                Uri.IsWellFormedUriString(documentUri, UriKind.Absolute))
            {
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, null);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));

                var username = User.Identity.Name;

                //var newFileData = new MemoryStream(new WebClient().DownloadData(documentUri));
                var fileName = Path.GetFileName(documentUri);
                var fileExtension = Path.GetExtension(documentUri).Replace(".", string.Empty);
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string mimeType);

                if (!fileName.Contains(".") || fileExtension == "" || fileExtension == null || mimeType == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad input",
                        "The URL you have entered is not a file.",
                        new FormatException(), $"~/Project/{projId}/Container/{id}/{containerVersion}/Index"));


                var newDoc = await _contentService.PostContainerContentExternal(username, projId, ContainerType.ICDD,
                    id, containerVersion,
                    new ExternalDocumentRequest(documentUri, description, fileName, fileExtension, mimeType, schema,
                        schemaVersion, schemaSubset));

                if (newDoc != null)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad input", "The URL you have entered is not a file.",
                new FormatException(), $"~/Project/{projId}/Container/{id}/{containerVersion}/Index"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolderDocumentAsync(string projId, string id, string containerVersion,
            string folderName, string? schema, string? schemaVersion, string? schemaSubset)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id))
            {
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, null);
                if (Container == null)
                    return NotFound();

                var username = User.Identity.Name;

                var newDoc = await _contentService.PostContainerContentFolder(username, projId, ContainerType.ICDD, id,
                    containerVersion, new FolderDocumentRequest(folderName, schema, schemaVersion, schemaSubset));

                if (newDoc != null)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad input", "The URL you have entered is not a file.",
                new FormatException(), $"~/Project/{projId}/Container/{id}/{containerVersion}/Index"));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateContentAsync(string projectId, string containerId, string containerVersion,
            string contentId, ContentMetadataUpdate contentMetadataUpdate)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }
            var username = User.Identity.Name;
            var result = await _contentService.UpdateContainerContent(projectId, ContainerType.ICDD, containerId,
                containerVersion, contentId, username, contentMetadataUpdate);

            return result != null ? Ok() : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDatabaseLinkAsync(string projId, string id, string containerVersion,
            string connectionString, string databaseName, string databaseType, string queryLanguage, string? schema,
            string? schemaVersion, string? schemaSubset, string? mappingFile)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(connectionString) &&
                !string.IsNullOrEmpty(databaseName))
            {
                InformationContainer Container =
                    await _containerService.GetContainer(projId, ContainerType.ICDD, id, null);
                if (Container == null)
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not find container.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));

                var username = User.Identity.Name;
                var newDoc = await _contentService.PostContainerContentDatabase(username, projId, ContainerType.ICDD,
                    id, containerVersion,
                    new DatabaseRequest(connectionString, databaseName, databaseType, queryLanguage, schema,
                        schemaVersion, schemaSubset, mappingFile));

                if (newDoc != null)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                        "Could not create database link.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                "Container ID or connection string or database name is empty.",
                new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpPost]
        public async Task<IActionResult> AddOntologyAsync(string projId, string id, string selectTypeOntology,
            string containerVersion, string WebUrl, IFormFile UploadFile)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id))
            {
                bool result;
                if (selectTypeOntology == "url")
                {
                    result = await _containerService.PostOntologyToContainer(User.Identity.Name, projId, ContainerType.ICDD, id,
                        containerVersion, WebUrl, null);
                }
                else if (selectTypeOntology == "file")
                {
                    result = await _containerService.PostOntologyToContainer(User.Identity.Name, projId, ContainerType.ICDD, id,
                        containerVersion, null, UploadFile);
                }
                else
                {
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add ontology.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
                }

                if (result)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add ontology.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }
            else
            {
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Container ID is empty.",
                    new FormatException(), $"~/Project/Details/" + projId));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPayloadTriplesAsync(string projId, string id, string containerVersion,
            string selectTypePayload, string WebUrl, IFormFile UploadFile)
        {
            if (string.IsNullOrEmpty(projId))
                return RedirectToAction("List", "Project");

            if (!string.IsNullOrEmpty(id))
            {
                bool result;
                if (selectTypePayload == "url")
                {
                    result = await _containerService.PostPayloadTriplesToContainer(User.Identity.Name, projId, ContainerType.ICDD, id,
                        containerVersion, WebUrl, null);
                }
                else if (selectTypePayload == "file")
                {
                    result = await _containerService.PostPayloadTriplesToContainer(User.Identity.Name, projId, ContainerType.ICDD, id,
                        containerVersion, null, UploadFile);
                }
                else
                {
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                        "Could not add payload triples.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
                }

                if (result)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                        "Could not add payload triples.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }
            else
            {
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Container ID is empty.",
                    new FormatException(), $"~/Project/Details/" + projId));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPartyAsync(string projId, string id, string containerVersion, string Name,
            string Description, int Type)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description))
            {
                var result = await _containerService.AddParticipantToContainer(User.Identity.Name, projId, ContainerType.ICDD, id,
                    containerVersion, Type, Name, Description);

                if (result)
                    return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not add party.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAsync(string projId, string id, string containerVersion)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            if (!string.IsNullOrEmpty(id))
            {
                var timeStart = DateTime.Now;
                var file = await _containerService.GetContainerAsFile(projId, ContainerType.ICDD, id, containerVersion);
                var timeFinished = DateTime.Now;
                var timeSpan = timeFinished - timeStart;
                Logger.Log("Zeitspanne: " + timeSpan.TotalMilliseconds + "ms", Logger.MsgType.Info, "GetContainerFile");

                if (file != null)
                    return File(file.Content, file.ContentType, file.FileName);
                else
                    return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                        "Could not download container file.",
                        new FormatException(),
                        $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Container ID is empty.",
                new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpGet]
        public async Task<IActionResult> AddBinaryLinkAsync(string projectId, string containerId,
            string containerVersion, string linksetId, string jsonLinkElements)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            var username = User.Identity.Name;
            var linkElements = JsonConvert.DeserializeObject<BinaryLinkDTO>(jsonLinkElements);

            var result = await _linksetService.PostContainerLinksetBinaryLink(username, projectId, ContainerType.ICDD,
                containerId, containerVersion, linksetId, linkElements);

            if (result != null)
            {
                var project = await _projectService.GetProject(projectId, username);

                var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD,
                    containerId, containerVersion);
                if (containerMetadata != null && project != null)
                {
                    var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId,
                        containerVersion);
                    return Ok();
                }

                return BadRequest();

            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> AddDirectedLinkAsync(string projectId, string containerId,
            string containerVersion, string linksetId, string jsonLinkElements)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            var username = User.Identity?.Name;
            var linkElements = JsonConvert.DeserializeObject<DirectedLinkDTO>(jsonLinkElements);

            var result = await _linksetService.PostContainerLinksetDirectedLink(username, projectId, ContainerType.ICDD,
                containerId, containerVersion, linksetId, linkElements);

            if (result != null)
            {
                var project = await _projectService.GetProject(projectId, username);

                var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD,
                    containerId, containerVersion);
                if (containerMetadata != null && project != null)
                {
                    var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId,
                        containerVersion);
                    return Ok();
                }

                return BadRequest();

            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> AddDirectedBinaryLinkAsync(string projectId, string containerId,
            string containerVersion, string linksetId, string jsonLinkElements)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            var username = User.Identity?.Name;
            var linkElements = JsonConvert.DeserializeObject<DirectedBinaryLinkDTO>(jsonLinkElements);

            var result = await _linksetService.PostContainerLinksetDirectedBinaryLink(username, projectId,
                ContainerType.ICDD, containerId, containerVersion, linksetId, linkElements);

            if (result != null)
            {
                var project = await _projectService.GetProject(projectId, username);

                var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD,
                    containerId, containerVersion);
                if (containerMetadata != null && project != null)
                {
                    var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId,
                        containerVersion);
                    return Ok();
                }

                return BadRequest();

            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> AddDirected1ToNLinkAsync(string projectId, string containerId,
            string containerVersion, string linksetId, string jsonLinkElements)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            var username = User.Identity?.Name;
            var linkElements = JsonConvert.DeserializeObject<Directed1ToNLinkDTO>(jsonLinkElements);

            var result = await _linksetService.PostContainerLinksetDirected1ToNLink(username, projectId,
                ContainerType.ICDD, containerId, containerVersion, linksetId, linkElements);

            if (result != null)
            {
                var project = await _projectService.GetProject(projectId, username);

                var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD,
                    containerId, containerVersion);
                if (containerMetadata != null && project != null)
                {
                    var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId,
                        containerVersion);
                    return Ok();
                }

                return BadRequest();

            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> QueryContainer(string projectId, string containerId, string containerVersion,
            string query, bool applyInference)
        {

            if (string.IsNullOrEmpty(projectId))
                return Json(JsonConvert.SerializeObject(new ArgumentException()));

            if (!string.IsNullOrEmpty(containerId))
            {
                var result = await _queryService.GetQueryContainer(query, projectId, ContainerType.ICDD, containerId,
                    containerVersion, applyInference);

                if (!string.IsNullOrEmpty(result))
                    return Json(result);
                else
                    return Json(JsonConvert.SerializeObject(new BadHttpRequestException("Malformed query")));
            }
            else
            {
                return Json(JsonConvert.SerializeObject(new ArgumentNullException()));
            }
        }


        [HttpPost]
        public async Task<IActionResult> ShaclContainer([FromForm] string projectId, [FromForm] string containerId, [FromForm] string containerVersion,
            [FromForm] bool applyInference, [FromForm] IFormFile file)
        {

            if (string.IsNullOrEmpty(projectId))
                return Json(JsonConvert.SerializeObject(new ArgumentException()));

            if (!string.IsNullOrEmpty(containerId))
            {
                if (file == null)
                {
                    return Json(JsonConvert.SerializeObject(new ArgumentNullException()));
                }

                Stream stream = file.OpenReadStream();
                StreamReader reader = new StreamReader(stream);
                string shapes = await reader.ReadToEndAsync();
                var result = await _queryService.GetShaclContainer(shapes, projectId, ContainerType.ICDD, containerId,
                    containerVersion, applyInference);

                if (!string.IsNullOrEmpty(result))
                    return Json(result);
                else
                    return Json(JsonConvert.SerializeObject(new BadHttpRequestException("Malformed query")));
            }
            else
            {
                return Json(JsonConvert.SerializeObject(new ArgumentNullException()));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostContainerQuery(string projectId, string containerId, string containerVersion,
            string query, string queryName, bool applyInference)
        {
            if (string.IsNullOrEmpty(projectId))
                return BadRequest();

            if (!string.IsNullOrEmpty(containerId))
            {
                var result = await _queryService.PostContainerQuery(projectId, ContainerType.ICDD, containerId, containerVersion, query, queryName, applyInference);

                if (result)
                    return Ok(result);
                else
                    return BadRequest();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteLinksetAsync(string projectId, string containerId, string containerVersion,
            string linksetId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            var result = await _linksetService.DeleteContainerLinkset(projectId, ContainerType.ICDD, containerId,
                containerVersion, linksetId);
            if (result)
            {
                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLinkAsync(string projectId, string containerId, string containerVersion,
            string linksetId, string linkId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            var result = await _linksetService.DeleteContainerLinksetLink(projectId, ContainerType.ICDD, containerId,
                containerVersion, linksetId, linkId);
            if (result)
            {
                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOntologyAsync(string projectId, string containerId, string containerVersion,
            string ontologyName)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }
            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD,
                containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId,
                    containerVersion);
                return container.DeleteOntology(ontologyName) ? Ok() : NotFound();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> DeletePayloadTriplesAsync(string projectId, string containerId, string containerVersion,
            string payloadName)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }
            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD,
                containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId,
                    containerVersion);
                return container.DeletePayloadTriple(payloadName) ? Ok() : NotFound();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateContainerAsync(string projId, string id, string containerVersion,
            ContainerMetadata containerMetadata)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            containerMetadata.Modifier = user.UserName;
            containerMetadata.Modified = DateTime.Now;
            if (containerMetadata.Status == ContainerStatus.PUBLISHED)
                containerMetadata.Publisher = user.Organisation;
            var result = await _containerService.UpdateContainer(User.Identity.Name, projId, ContainerType.ICDD, id, containerVersion,
                containerMetadata);

            if (result != null)
                return Redirect("~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index");
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not update container.",
                    new FormatException(),
                    $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadContentAsync(string projId, string id, string containerVersion,
            string contentId)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            if (!string.IsNullOrEmpty(id))
            {
                var timeStart = DateTime.Now;
                var file = await _contentService.GetContainerContentAsFile(projId, ContainerType.ICDD, id,
                    containerVersion, contentId);
                var timeFinished = DateTime.Now;
                var timeSpan = timeFinished - timeStart;
                Logger.Log("Zeitspanne: " + timeSpan.TotalMilliseconds + "ms", Logger.MsgType.Info, "GetContainerFile");

                if (file != null)
                    return File(file.Content, file.ContentType, file.FileName);
                else
                    return View("ExtendedError", new ErrorPageModel(404, "File not found",
                        "The file you requested for download could not be found.",
                        new FileNotFoundException(), $"~/Project/{projId}/Container/{id}/{containerVersion}/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not download document.",
                new FormatException(),
                $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPayloadTriplesAsync(string projId, string id, string containerVersion,
            string payloadFileName)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            if (!string.IsNullOrEmpty(id))
            {
                var timeStart = DateTime.Now;
                var file = await _containerService.GetContainerPayloadTriplesAsFile(projId, ContainerType.ICDD, id,
                    containerVersion, payloadFileName);
                var timeFinished = DateTime.Now;
                var timeSpan = timeFinished - timeStart;
                Logger.Log("Zeitspanne: " + timeSpan.TotalMilliseconds + "ms", Logger.MsgType.Info, "GetContainerPayloadTriplesFile");

                if (file != null)
                    return File(file.Content, file.ContentType, file.FileName);
                else
                    return View("ExtendedError", new ErrorPageModel(404, "File not found",
                        "The file you requested for download could not be found.",
                        new FileNotFoundException(), $"~/Project/{projId}/Container/{id}/{containerVersion}/Index"));
            }

            return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not download payload triples.",
                new FormatException(),
                $"~/Project/" + projId + "/Container/" + id + "/" + containerVersion + "/Index"));
        }

        [HttpGet]
        public async Task<IActionResult> ConvertDbAsTriples(string projectId, string containerId,
            string containerVersion, string contentId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return RedirectToAction("List", "Project");
            }

            if (!string.IsNullOrEmpty(projectId))
            {

                InformationContainer container =
                    await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);

                if (container == null) return BadRequest();


                CtDocument doc = container.GetDocument(contentId);
                if (doc?.GetType() != typeof(ExtDatabaseLink))
                    return BadRequest();

                if (doc is ExtDatabaseLink docDb)
                {
                    if (docDb.ConvertToPayload(out IcddPayloadTriples convertedTriples))
                    {
                        var user = await _userManager.FindByNameAsync(User.Identity?.Name);
                        var lsName = "auto-generated.rdf";
                        CtLinkset ls = container.Linksets.Find(ls => ls.FileName == lsName) ?? await _linksetService.PostContainerLinkset(user.UserName, projectId, ContainerType.ICDD, containerId, containerVersion, lsName);
                        ls.CreateBinaryLink(ls.CreateLinkElement(doc), ls.CreateLinkElement(convertedTriples.Proxy));
                        return Ok(convertedTriples.Individuals.Count);
                    }
                }
                return BadRequest();
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteContentAsync(string projectId, string containerId, string containerVersion, string contentId)
        {
            if (string.IsNullOrEmpty(projectId))
                return RedirectToAction("List", "Project");
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            var result = await _contentService.DeleteContent(user.UserName, projectId, ContainerType.ICDD, containerId, containerVersion, contentId);

            if (result)
                return Redirect("~/Project/" + projectId + "/Container/" + containerId + "/" + containerVersion + "/Index");
            else
                return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Could not delete document.",
                    new FormatException(), $"~/Project/" + projectId + "/Container/" + containerId + "/" + containerVersion + "/Index"));
        }
    }
}