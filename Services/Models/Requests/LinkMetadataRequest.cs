using IcddWebApp.Services.Models.Enums;

namespace IcddWebApp.Services.Models.Requests
{
    public class LinkMetadataRequest
    {
        LinkType LinkType { get; set; }
        public string DocumentId1 { get; set; }
        public string DocumentId2 { get; set; }
        public string Identifier1 { get; set; }
        public string Identifier2 { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public bool IsUri1 { get; set; }
        public bool IsUri2 { get; set; }

        public LinkMetadataRequest() { }

        public LinkMetadataRequest(LinkType linkType, string documentId1, string documentId2, string identifier1, string identifier2, string field1, string field2, bool isUri1 = false, bool isUri2 = false)
        {
            LinkType = linkType;
            DocumentId1 = documentId1;
            DocumentId2 = documentId2;
            Identifier1 = identifier1;
            Identifier2 = identifier2;
            Field1 = field1;
            Field2 = field2;
            IsUri1 = isUri1;
            IsUri2 = isUri2;
        }
    }
}
