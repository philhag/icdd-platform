//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace IcddWebApp.Models.Requests
//{
//    public class ContentMetadataRequest
//    {
//        public string? Schema { get; set; }
//        public string? SchemaVersion { get; set; }
//        public string? SchemaSubset { get; set; }


//        public ContentMetadataRequest() { }

//        public ContentMetadataRequest(string? schema, string? schemaVersion, string? schemaSubset)
//        {
//            if (schema != null)
//                Schema = schema;

//            if (schemaVersion != null)
//                SchemaVersion = schemaVersion;

//            if (schemaSubset != null)
//                SchemaSubset = schemaSubset;
//        }

//        public ContentMetadataRequest(ContentMetadataFileRequest fileRequest)
//        {
//            if (fileRequest.Schema != null)
//                Schema = fileRequest.Schema;

//            if (fileRequest.SchemaVersion != null)
//                SchemaVersion = fileRequest.SchemaVersion;

//            if (fileRequest.SchemaSubset != null)
//                SchemaSubset = fileRequest.SchemaSubset;
//        }
//    }
//}
