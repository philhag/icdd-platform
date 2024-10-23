using System;
using System.IO;
using IcddWebApp.WebApplication.Environment.Model;
using Newtonsoft.Json;

namespace IcddWebApp.WebApplication.Environment
{
    public static class JModel
    {
        public static JProject GetProject(string root, string guid)
        {
            string file = GetProjectPath(root, guid);

            if (!File.Exists(file))
            {
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(file);
                JProject container = JsonConvert.DeserializeObject<JProject>(jsonString);
                return container;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static string GetProjectPath(string root, string guid)
        {
            string projectsPath = Path.Combine(root,"wwwroot", "projects");
            return Path.Combine(projectsPath, guid + ".json");
        }



    }
}
