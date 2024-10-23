using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IcddWebApp.WebApplication.IfcClasses;
using IIB.ICDD.Model;
using IIB.ICDD.Model.Container.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace IcddWebApp.WebApplication.Environment
{
    public class IfcContext
    {

        public Dictionary<CtDocument, string> WexbimFiles { get; set; } = new Dictionary<CtDocument, string>();
        public string WexbimFolder { get; set; }
        public InformationContainer Container { get; set; }
        private IConfiguration Configuration { get; set; }

        public IfcContext(InformationContainer container)
        {
            Container = container;
            WexbimFolder = Path.Combine("wwwroot/downloads/wexbim/", Container.ContainerGuid);
            Directory.CreateDirectory(WexbimFolder);
        }

        public async Task CreateWexbimAsync()
        {


            try
            {
                var ifcdocs = Container.Documents.FindAll(m => m.FileType.Contains("ifc") || m.FileType.Contains("IFC"));
                foreach (var document in ifcdocs)
                {
                    var fullPathInputFile = Path.Combine(Container.PathToContainer, "Payload Documents", document.Name);
                    var fullPathOutputFile = Path.ChangeExtension(Path.Combine(WexbimFolder, document.Name), "wexbim");

                    if (!System.IO.File.Exists(fullPathInputFile) || System.IO.File.Exists(fullPathOutputFile))
                    {
                        if (System.IO.File.Exists(fullPathOutputFile))
                        {
                            WexbimFiles.Add(document, fullPathOutputFile);
                        }
                        continue;
                    }


                    var inputStream = await PostIfc(fullPathInputFile);
                    if (inputStream != null)
                    {
                        using (FileStream output = new FileStream(fullPathOutputFile, FileMode.Create))
                        {
                            inputStream.CopyTo(output);
                            output.Close();
                        }
                    }
                    if (System.IO.File.Exists(fullPathOutputFile))
                    {
                        WexbimFiles.Add(document, fullPathOutputFile);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("konnte keine verbindung zum Wexbim server herstellen:" + e.ToString(), Logger.MsgType.Error, "CreateWexbim()");
            }
        }

        public async Task<Stream> PostIfc(string filename)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            Stream paramFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            string actionUrl = Configuration["geometryServer"];
            HttpContent fileStreamContent = new StreamContent(paramFileStream);

            using var client = new HttpClient();
            using var formData = new MultipartFormDataContent();

            string username = "adesso";
            string password = "{`zFD>7Wc!?n%!F*";

            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Basic " + svcCredentials);
            formData.Add(fileStreamContent, "files", filename);
            var response = await client.PostAsync(actionUrl, formData);
            if (!response.IsSuccessStatusCode)
            {
                Logger.Log("WexBIM Datei konnte nicht vom Server " + actionUrl + " generiert werden! ErrorCode: " + response.StatusCode.ToString(), Logger.MsgType.Error, "ExploreContainerModel.PostIfc()");
                return null;
            }
            Logger.Log("WexBIM Datei konnte erfolgreich vom Server " + actionUrl + " generiert werden!", Logger.MsgType.Info, "ExploreContainerModel.PostIfc()");
            return await response.Content.ReadAsStreamAsync();
        }

        public string GetWexbimFile(HttpRequest server, CtDocument doc)
        {
            if (!WexbimFiles.TryGetValue(doc, out var filepath)) return null;
            //FileInfo f = new FileInfo(filepath);
            //string name = f.FullName;
            var path = filepath.Replace("wwwroot", "");
            if (server.IsHttps)
            {
                path = "https://" + server.Host.ToUriComponent() +"/ui"+ path;
            }
            else
            {
                path = "http://" + server.Host.ToUriComponent() + path;
            }
            return JsonConvert.SerializeObject(path);
        }

        public List<IfcObject> GetIfcObjects(CtDocument doc)
        {
            var fileName = Path.Combine(Container.PathToContainer, "Payload Documents", doc.Name);
            if (!doc.FileType.ToLower().Contains("ifc") || !System.IO.File.Exists(fileName))
                return new List<IfcObject>();
            return IfcManager.GetIfcObjects(fileName);
        }

        public Dictionary<string, List<IfcObject>> GetIfcObjectsWithSpatialRelation(CtDocument doc)
        {
            var fileName = Path.Combine(Container.PathToContainer, "Payload Documents", doc.Name);
            if (!doc.FileType.ToLower().Contains("ifc") || !System.IO.File.Exists(fileName))
                return new Dictionary<string, List<IfcObject>>();
            return IfcManager.GetIfcObjectsWithSpatialElement(fileName);
        }
    }
}
