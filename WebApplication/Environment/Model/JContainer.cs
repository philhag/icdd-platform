using System;
using System.IO;

namespace IcddWebApp.WebApplication.Environment.Model
{
    public class JContainer
    {
        public string ProjectRoot;
        public string GUID;
        public string EXTGUID;
        public string Filename;
        public DateTime Created;
        public DateTime Modified;

        public JContainer() { }
        public JContainer(JProject project, string filename)
        {
            ProjectRoot = project.GetRepository();
            GUID = GuidEncoder.Encode(Guid.NewGuid());
            Filename = filename;
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }

        public string GetRepository()
        {
            var directory = Path.Combine(ProjectRoot, GUID);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }
        public string GetIcddRepository()
        {
            var directory = Path.Combine(GetRepository(), EXTGUID);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }
    }
}
