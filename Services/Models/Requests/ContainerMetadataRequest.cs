using System.Collections.Generic;
using IcddWebApp.Services.Models.Enums;

namespace IcddWebApp.Services.Models.Requests
{
    public class ContainerMetadataRequest
    {
        public string? ContainerFileName { get; set; }
        public string? ContainerDescription { get; set; }
        public string? Revision { get; set; }
        public ContainerSuitability? Suitability { get; set; }
        public ContainerStatus? Status { get; set; }
        public string? MetadataSchema { get; set; }
        public List<AdditionalParameter>? AdditionalParameters { get; set; } = new List<AdditionalParameter>();


        public ContainerMetadataRequest() { }

        public ContainerMetadataRequest(string? fileName, string? description, string? revision, ContainerSuitability? suitability, ContainerStatus? status, string? schema, List<AdditionalParameter>? additionalParameters)
        {
            if (fileName != null && fileName.EndsWith(".icdd"))
                ContainerFileName = fileName;
            else if (fileName != null)
                ContainerFileName = $"{fileName}.icdd";

            if (description != null)
                ContainerDescription = description;

            if (revision != null)
                Revision = revision;

            if (suitability != null)
                Suitability = suitability;

            if (status != null)
                Status = status;

            if (schema != null)
                MetadataSchema = schema;

            if (additionalParameters != null)
              
                AdditionalParameters = additionalParameters;
        }
        public ContainerMetadataRequest(ContainerMetadataFileRequest fileRequest)
        {
            if (fileRequest.Revision != null)
                Revision = fileRequest.Revision;

            if (fileRequest.Suitability != null)
                Suitability = fileRequest.Suitability;

            if (fileRequest.Status != null)
                Status = fileRequest.Status;

            if (fileRequest.MetadataSchema != null)
                MetadataSchema = fileRequest.MetadataSchema;

            if (fileRequest.AdditionalParameters != null)
                AdditionalParameters = fileRequest.AdditionalParameters;
        }
    }
}

