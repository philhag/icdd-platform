using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IIB.ICDD.Model;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Shacl;
using Xbim.Ifc4.Interfaces;
using Path = System.IO.Path;

namespace IcddWebApp.PageModels.Shapes
{
    public class ShapesPageModel
    {
        public readonly string ShapesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shapefiles");
        public DirectoryInfo ShapesFolderInfo;
        public FileInfo[] ShapeFileInfos;
        public List<ShapeFile> ShapeFiles = new List<ShapeFile>();
        public List<ShapeInfo> Shapes = new List<ShapeInfo>();

        public ShapesPageModel()
        {
            Refresh();

        }

        public void Refresh()
        {
            ShapesFolderInfo = new DirectoryInfo(ShapesFolder);
            if (!ShapesFolderInfo.Exists)
                Directory.CreateDirectory(ShapesFolder);
            ShapeFileInfos = ShapesFolderInfo.GetFiles();
            foreach (var elem in ShapeFileInfos)
            {
                ShapeFiles.Add(new ShapeFile(elem, ShapesFolder));
            }
            Shapes = ShapeFileInfos.ToList().Select(ShapeInfo.CreateShapeInfo).ToList();
        }
    }

    public class ShapeFile
    {
        public FileInfo FileInfo;
        public string RawData;

        public ShapeFile(FileInfo fileInfo, string shapesFolder)
        {
            FileInfo = fileInfo;
            RawData = System.IO.File.ReadAllText(Path.Combine(shapesFolder, FileInfo.Name));
        }
    }

    public class ShapeInfo
    {
        public FileInfo FileInfo;
        public ShapesGraph ShapesGraph;
        public List<Individual> Shapes;
        public string RawData;
        public ShapeInfo()
        {

        }

        public static ShapeInfo CreateShapeInfo(FileInfo info)
        {
            if (info is not { Exists: true } || !info.Extension.Contains("ttl"))
            {
                return null;
            }

            ShapeInfo shape = new ShapeInfo
            {
                FileInfo = info,
                RawData = File.ReadAllText(info.FullName)
            };

            try
            {
                var graph = new Graph();
                FileLoader.Load(graph, shape.FileInfo.FullName);
                shape.ShapesGraph = new ShapesGraph(graph);
                shape.Shapes = shape.ShapesGraph.AllShapes();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return shape;

        }



    }

    public static class ShapesExtensions
    {

        public static List<Individual> AllShapes(this ShapesGraph ob)
        {
            var onto = new OntologyGraph();
            onto.BaseUri = ob.BaseUri;
            onto.NamespaceMap.Import(ob.NamespaceMap);
            ob.Triples.ToList().ForEach(t => onto.Assert(t));
            return onto.GetIndividuals().FindAll(m => m.Classes.Contains(new OntologyClass(onto.CreateUriNode("sh:NodeShape"), onto)) || m.Classes.Contains(new OntologyClass(onto.CreateUriNode("sh:PropertyShape"), onto)));
        }

        public static List<Individual> NodeShapes(this ShapesGraph ob)
        {
            var onto = new OntologyGraph();
            onto.BaseUri = ob.BaseUri;
            onto.NamespaceMap.Import(ob.NamespaceMap);
            ob.Triples.ToList().ForEach(t => onto.Assert(t));
            return onto.GetIndividuals().FindAll(m => m.Classes.Contains(new OntologyClass(onto.CreateUriNode("sh:NodeShape"), onto)));
        }

        public static List<Individual> PropertyShapes(this ShapesGraph ob)
        {
            var onto = new OntologyGraph();
            onto.BaseUri = ob.BaseUri;
            onto.NamespaceMap.Import(ob.NamespaceMap);
            ob.Triples.ToList().ForEach(t => onto.Assert(t));
            return onto.GetIndividuals().FindAll(m => m.Classes.Contains(new OntologyClass(onto.CreateUriNode("sh:PropertyShape"), onto)));
        }

        public static List<Individual> GetIndividuals(this OntologyGraph ob)
        {
            return ob.GetTriplesWithPredicate(new Uri(RdfSpecsHelper.RdfType))
                .Select(trip => ob.CreateIndividual(trip.Subject)).Distinct(new IndividualComparer()).ToList();
        }
    }
    internal class IndividualComparer : IEqualityComparer<Individual>
    {
        public bool Equals(Individual x, Individual y)
        {
            if (x != null && y != null && x.Resource.Equals(y.Resource))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Individual obj)
        {
            return obj.Resource.GetHashCode();
        }
    }
}
