using System.ComponentModel;

namespace IcddWebApp.Services.Models.Enums
{
    public enum ContainerStatus 
    {
        [Description("WORK IN PROGRESS")]
        WORK_IN_PROGRESS,
        [Description("SHARED")]
        SHARED,
        [Description("PUBLISHED")]
        PUBLISHED,
        [Description("ARCHIVED")]
        ARCHIVED
    }
}
