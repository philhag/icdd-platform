using System.ComponentModel;

namespace IcddWebApp.Services.Models.Enums
{
    public enum ContainerType
    {
        [Description("ICDD Standard")]
        ICDD,
        [Description("ICDD AMS Inspection")]
        ICDDAmsInspection,
        [Description("ICDD AMS Maintenance")]
        ICDDAmsMaintenance
    }
}
