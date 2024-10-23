using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace IcddWebApp.WebApplication.IfcClasses
{
    public class IfcManager
    {
        public static List<IfcObject> GetIfcObjects(string fileName)
        {
            var res = new List<IfcObject>();
            using (var model = IfcStore.Open(fileName))
            {
                var project = model.Instances.FirstOrDefault<IIfcProject>();
                res.AddRange(PrintHierarchy(project, 0));
            }
            return res;
        }

        public static Dictionary<string, List<IfcObject>> GetIfcObjectsWithSpatialElement(string fileName)
        {
            var res = new Dictionary<string, List<IfcObject>>();
            using (var model = IfcStore.Open(fileName))
            {
                var project = model.Instances.FirstOrDefault<IIfcProject>();
                var newSpatial = GetSpatialStructureElement(project, 0);
                foreach (var keyValuePair in newSpatial)
                {
                    if (!res.ContainsKey(keyValuePair.Key))
                        res.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return res;
        }

        public static Dictionary<string, List<IfcObject>> GetSpatialStructureElement(IIfcObjectDefinition o, int level)
        {
            var res = new Dictionary<string, List<IfcObject>>();

            var spatialElement = o as IIfcBuilding;
            if (spatialElement != null)
            {
                foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
                {
                    List<IfcObject> objects = PrintHierarchy(item, level);
                    res.Add(item.Name, objects);
                }
            }
            if(level <= 2)
            {
                foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
                {
                    var newSpatials = GetSpatialStructureElement(item, level + 1);
                    foreach (var kvp in newSpatials)
                        res.Add(kvp.Key, kvp.Value);
                }
            }
            return res;
        }

        private static List<IfcObject> PrintHierarchy(IIfcObjectDefinition o, int level)
        {
            List<IfcObject> objects = new List<IfcObject>();
            Console.WriteLine($"{GetIndent(level)}{o.Name} [{o.GetType().Name}]");

            var spatialElement = o as IIfcSpatialStructureElement;
            if (spatialElement != null)
            {
                var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                foreach (var element in containedElements)
                {
                    var newObj = new IfcObject(element.GlobalId.ToString(), element.Name, element.GetType().Name);
                    var mat = element.Material as IIfcMaterial;
                    if (element.Material != null && mat!=null)
                    {                       
                        newObj.SetMaterial(mat.Name);
                    }

                    objects.Add(newObj);
                }
            }

            foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
                objects.AddRange(PrintHierarchy(item, level + 1));

            return objects;
        }

        private static string GetIndent(int level)
        {
            var indent = "";
            for (int i = 0; i < level; i++)
                indent += "  ";
            return indent;
        }
    }
}
