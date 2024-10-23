using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IcddWebApp.PageModels.Container;
using IcddWebApp.PageModels.Error;
using IcddWebApp.Services;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.Enums;
using IcddWebApp.WebApplication.Environment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using IcddWebApp.PageModels.Shapes;

namespace IcddWebApp.Controllers
{
    [Authorize]
    public class PartialsController : Controller
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


        public PartialsController(IWebHostEnvironment environment, IProjectService projectService, IContainerService containerService, IContentService contentService, ILinksetService linksetService, IAuthService authService, IQueryService queryService, UserManager<User> userManager, IConfiguration configuration)
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
        public async Task<IActionResult> TreeContent(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/_TreeView", new ContainerPageModel(project, containerMetadata, container));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> SPARQL(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/_QueryContainer", new ContainerPageModel(project, containerMetadata, container));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> SavedSPARQLqueries(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/_SavedQueries", new ContainerPageModel(project, containerMetadata, container));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> SHACL(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/_SHACLContainer", new ContainerPageModel(project, containerMetadata, container));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> SavedSHACLfiles(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/_SavedSHACL", new ShapesPageModel());
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> IndexContent(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/Index/_indexContentView", new ContainerPageModel(project, containerMetadata, container));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> IndexProperties(string projectId, string containerId, string containerVersion)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/Index/_indexPropertyView", new ContainerPageModel(project, containerMetadata, container));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> DocumentContent(string projectId, string containerId, string containerVersion, string contentId)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(contentId))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/Documents/_documentContentView", new DocumentPageModel(project, containerMetadata, container, container.GetDocument(contentId)));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> DocumentProperties(string projectId, string containerId, string containerVersion, string contentId)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(contentId))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/Documents/_documentPropertyView", new DocumentPageModel(project, containerMetadata, container, container.GetDocument(contentId)));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> LinksetContent(string projectId, string containerId, string containerVersion, string linksetId)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(linksetId))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Linkset/Partials/_linksetContentView", new LinksetPageModel(project, containerMetadata, container, container.GetLinkset(linksetId), _workfolderPath));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> LinkContent(string projectId, string containerId, string containerVersion, string linksetId, string linkId)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(linksetId) || string.IsNullOrEmpty(linkId))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Linkset/Partials/_linksetLinkDetailsView", new LinkPageModel(project, containerMetadata, container, container.GetLinkset(linksetId), linkId, _workfolderPath));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> LinksetProperties(string projectId, string containerId, string containerVersion, string linksetId)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(linksetId))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Linkset/Partials/_linksetPropertyView", new LinksetPageModel(project, containerMetadata, container, container.GetLinkset(linksetId), _workfolderPath));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> Content(string projectId, string containerId, string containerVersion, string guid)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(guid))
            {
                return PartialView("Error", new ErrorPageModel(404, "Not found", "This item does not exist.",
                new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);
            guid = guid.TrimStart('#');
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                var document = container.Documents.Find(dc => dc.Guid == guid);
                if (document != null)
                {
                    return PartialView("../Container/Partials/Documents/_documentContentView", new DocumentPageModel(project, containerMetadata, container, document));
                }
                var linkset = container.Linksets.Find(dc => dc.Guid == guid);
                if (linkset != null)
                {
                    return PartialView("../Container/Partials/Documents/_linksetContentView", new LinksetPageModel(project, containerMetadata, container, linkset, _workfolderPath));
                }
                var linksetforlink = container.Linksets.Find(dc => dc.HasLinks.Any(x=>x.Guid==guid));
                if (linksetforlink != null)
                {
                    return PartialView("../Linkset/Partials/_linksetLinkDetailsView", new LinkPageModel(project, containerMetadata, container, linksetforlink, guid, _workfolderPath));
                }
                
            }

            return PartialView("Error", new ErrorPageModel(404, "Not found", "This item does not exist.",
               new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> Properties(string projectId, string containerId, string containerVersion, string guid)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(guid))
            {
                return PartialView("Error", new ErrorPageModel(404, "Not found", "This item does not exist.",
                new FormatException(), $"~/Project/List"));
            }
            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);
            guid = guid.TrimStart('#');
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                var document = container.Documents.Find(dc => dc.Guid == guid);
                if (document != null)
                {
                    return PartialView("../Container/Partials/Documents/_documentPropertyView", new DocumentPageModel(project, containerMetadata, container, document));
                }
                var linkset = container.Linksets.Find(dc => dc.Guid == guid);
                if (linkset != null)
                {
                    return PartialView("../Linkset/Partials/_linksetPropertyView", new LinksetPageModel(project, containerMetadata, container, linkset, _workfolderPath));
                }
                var linksetforlink = container.Linksets.Find(dc => dc.HasLinks.Any(x => x.Guid == guid));
                if (linksetforlink != null)
                {                    
                    return PartialView("../Linkset/Partials/_linksetPropertyView", new LinksetPageModel(project, containerMetadata, container, linksetforlink, _workfolderPath));
                }
                
            }

            return PartialView("Error", new ErrorPageModel(404, "Not found", "This item does not exist.",
               new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> Viewer(string projId, string id, string containerVersion, string contentId)
        {
            if (string.IsNullOrEmpty(projId))
            {
                return RedirectToAction("List", "Project");
            }

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(containerVersion) && !string.IsNullOrEmpty(contentId))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                var configuration = builder.Build();
                string actionUrl = configuration["geometryServerMapping"];
                try
                {


                    var timeStart = DateTime.Now;
                    var file = await _contentService.GetContainerContentAsFile(projId, ContainerType.ICDD, id, containerVersion, contentId);
                    var timeFinished = DateTime.Now;
                    var timeSpan = timeFinished - timeStart;
                    Logger.Log("Zeitspanne: " + timeSpan.TotalMilliseconds + "ms", Logger.MsgType.Info, "GetContainerFile");


                    if (file != null)
                    {
                        Stream paramFileStream = file.Content;
                        HttpContent fileStreamContent = new StreamContent(paramFileStream);
                        using (var client = new HttpClient())
                        using (var formData = new MultipartFormDataContent())
                        {
                            string username = "adesso";
                            string password = "{`zFD>7Wc!?n%!F*";

                            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
                            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Basic " + svcCredentials);
                            formData.Add(fileStreamContent, "files", file.FileName);
                            var response = await client.PostAsync(actionUrl, formData);
                            if (!response.IsSuccessStatusCode)
                            {
                                Logger.Log("WexBIM Datei konnte nicht vom Server " + actionUrl + " generiert werden! ErrorCode: " + response.StatusCode.ToString(), Logger.MsgType.Error, "ExploreContainerModel.PostIfc()");
                                return null;
                            }
                            Logger.Log("WexBIM Datei konnte erfolgreich vom Server " + actionUrl + " generiert werden!", Logger.MsgType.Info, "ExploreContainerModel.PostIfc()");

                            var result = await response.Content.ReadAsStringAsync();
                            return Json(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("WexBIM Datei konnte nicht vom Server " + actionUrl + " generiert werden! Exception: " + ex, Logger.MsgType.Error, "ExploreContainerModel.PostIfc()");
                    return BadRequest(ex);
                }
            }
            return View("ExtendedError", new ErrorPageModel(400, "Bad request", "Container ID or containerVersion or document ID is empty.",
                            new FormatException(), $"~/Project/Details/" + projId));
        }

        [HttpGet]
        public async Task<IActionResult> Graph(string projectId, string containerId, string containerVersion, string linksetId, string linkId)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(linksetId) || string.IsNullOrEmpty(linkId))
            {
                return BadRequest();
            }

            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                var link = container.GetLinkset(linksetId).GetLink(linkId);
                var result = link.GetGraphJson();
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> OntologyContent(string projectId, string containerId, string containerVersion, string ontologyName)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(ontologyName))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/Ontologies/_ontologyContentView", new OntologyPageModel(project, containerMetadata, container, ontologyName));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> OntologyProperties(string projectId, string containerId, string containerVersion, string ontologyName)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(ontologyName))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/Ontologies/_ontologyPropertyView", new OntologyPageModel(project, containerMetadata, container, ontologyName));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> PayloadTriplesContent(string projectId, string containerId, string containerVersion, string payloadName)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(payloadName))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/PayloadTriples/_triplesContentView", new PayloadTriplesPageModel(project, containerMetadata, container, payloadName));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

        [HttpGet]
        public async Task<IActionResult> PayloadTriplesProperties(string projectId, string containerId, string containerVersion, string payloadName)
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(containerId) || string.IsNullOrEmpty(containerVersion) || string.IsNullOrEmpty(payloadName))
            {
                return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                    new FormatException(), $"~/Project/List"));
            }


            var username = User.Identity?.Name;
            var project = await _projectService.GetProject(projectId, username);

            var containerMetadata = await _containerService.GetContainerMetadata(projectId, ContainerType.ICDD, containerId, containerVersion);
            if (containerMetadata != null && project != null)
            {
                var container = await _containerService.GetContainer(projectId, ContainerType.ICDD, containerId, containerVersion);
                return PartialView("../Container/Partials/PayloadTriples/_triplesPropertyView", new PayloadTriplesPageModel(project, containerMetadata, container, payloadName));
            }

            return PartialView("Error", new ErrorPageModel(400, "Bad request", "Container ID or party description or party name is empty.",
                new FormatException(), $"~/Project/Details/" + projectId));
        }

    }
}
