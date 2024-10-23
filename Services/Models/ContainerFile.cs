using System.IO;

namespace IcddWebApp.Services.Models
{
    public class ContainerFile
    {
        public MemoryStream Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }

        public ContainerFile () { }
        public ContainerFile(MemoryStream content, string contentType, string fileName)
        {
            Content = content;
            ContentType = contentType;
            FileName = fileName;
        }
    }
}
