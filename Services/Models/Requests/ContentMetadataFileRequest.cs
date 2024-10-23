//using Microsoft.AspNetCore.Http;
//using IcddWebApp.Models.ContainerMetadataEnums;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace IcddWebApp.Models.Requests
//{
//    public class ContentMetadataFileRequest
//    {
//        public string? Schema { get; set; }
//        public string? SchemaVersion { get; set; }
//        public string? SchemaSubset { get; set; }
//        public DocumentType DocumentType { get; set; }
//        public InternalDocumentRequest? InternalDocumentRequest { get; set; }
//        public ExternalDocumentRequest? ExternalDocumentRequest { get; set; }
//        public EncryptedDocumentRequest? EncryptedDocumentRequest { get; set; }
//        public FolderDocumentRequest? FolderDocumentRequest { get; set; }
//        public SecuredDocumentRequest? SecuredDocumentRequest { get; set; }


//        public ContentMetadataFileRequest () { }

//        public ContentMetadataFileRequest(DocumentType documentType, string? schema, string? schemaVersion, string? schemaSubset, InternalDocumentRequest? internalDataRequest,
//            ExternalDocumentRequest? externalDataRequest, EncryptedDocumentRequest? encryptedDocumentRequest, FolderDocumentRequest? folderDocumentRequest, SecuredDocumentRequest? securedDocumentRequest)
//        {
//            DocumentType = documentType;

//            if (schema != null)
//                Schema = schema;

//            if (schemaVersion != null)
//                SchemaVersion = schemaVersion;

//            if (schemaSubset != null)
//                SchemaSubset = schemaSubset;

//            if (internalDataRequest != null)
//                InternalDocumentRequest = internalDataRequest;

//            if (externalDataRequest != null)
//                ExternalDocumentRequest = externalDataRequest;

//            if (encryptedDocumentRequest != null)
//                EncryptedDocumentRequest = encryptedDocumentRequest;

//            if (folderDocumentRequest != null)
//                FolderDocumentRequest = folderDocumentRequest;

//            if (securedDocumentRequest != null)
//                SecuredDocumentRequest = securedDocumentRequest;
//        }
//    }
//}
