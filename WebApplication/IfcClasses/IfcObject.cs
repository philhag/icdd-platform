namespace IcddWebApp.WebApplication.IfcClasses
{
    public class IfcObject
    {
        protected string Guid;
        protected string Name;
        protected string Type;
        protected string Material;

        public IfcObject(string guid, string name, string type)
        {
            Name = name;
            Type = type;
            Guid = guid;
        }

        public override string ToString()
        {
            return Type + "[" + Guid + "]";
        }

        public void SetMaterial(string value)
        {
            Material = value;
        }
        public string GetMaterial()
        {
            return Material ?? "undefined";
        }

        public string GetGuid()
        {
            return Guid;
        }
        public string GetName()
        {
            return Name ?? "undefined";
        }
        public string GetObjectType()
        {
            return Type ?? "undefined";
        }

    }
}
