using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Requests;

namespace IcddWebApp.PageModels.Container
{
    public class ContainerUploadPageModel
    {
        public string UploadResultMessage = "";
        public UploadResult UploadResult = UploadResult.None;
        public string ProjectGuid { get; set; }
        public Services.Models.Project Project { get; set; }
        public ContainerMetadata ContainerMetadata { get; set; }
        public IFormFile Upload { get; set; }
        public ContainerMetadataFileRequest Request { get; set; }

        public ContainerUploadPageModel(Services.Models.Project project)
        {
            Project = project;
        }
    }
    public enum UploadResult
    {
        NoFile,
        Invalid,
        Success,
        None
    }
}
