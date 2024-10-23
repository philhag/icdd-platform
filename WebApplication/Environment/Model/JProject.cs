using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IcddWebApp.WebApplication.Environment.Model
{
    public class JProject
    {
        public string Root;
        public string GUID;
        public string Name;
        public DateTime Created;
        public DateTime Modified;
        public List<JContainer> Container;

        public JProject() { }
        public JProject(string root, string name)
        {
            GUID = GuidEncoder.Encode(Guid.NewGuid());
            Name = name;
            Created = DateTime.Now;
            Modified = DateTime.Now;
            Container = new List<JContainer>();
            Root = root;
        }

        public async Task Commit()
        {
            string path = JModel.GetProjectPath(Root, GUID);
            try
            {
                await using StreamWriter outputFile = new StreamWriter(path);
                Modified = DateTime.Now;
                outputFile.Write(JsonConvert.SerializeObject(this));
                outputFile.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task AppendContainer(JContainer container)
        {
            Container.Add(container);
            await Commit();
        }
        public async Task RemoveContainer(JContainer container)
        {
            Container.Remove(container);
            await Commit();
        }

        public string GetRepository()
        {
            return Path.Combine(Root, GetRelativeProjectsPath(), GUID+"_"+Name);
        }

        public static string GetRelativeProjectsPath()
        {
            return Path.Combine("wwwroot", "projects");
        }
    }
}
