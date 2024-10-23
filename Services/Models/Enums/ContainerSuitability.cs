using System.ComponentModel;

namespace IcddWebApp.Services.Models.Enums
{
    public enum ContainerSuitability
    {
        //nach PAS 1192
        [Description("Default container")]
        DEFAULT,
        [Description("Coordination (PAS 1192)")]
        SUITABLE_FOR_COORDINATION,
        [Description("Information (PAS 1192)")]
        SUITABLE_FOR_INFORMATION,
        [Description("Internal review and comment (PAS 1192)")]
        SUITABLE_FOR_INTERNAL_REVIEW_AND_COMMENT,
        [Description("Construction approval (PAS 1192)")]
        SUITABLE_FOR_CONSTRUCTION_APPROVAL,
        [Description("Manufacture (PAS 1192)")]
        SUITABLE_FOR_MANUFACTURE,
        [Description("PIM authorization (PAS 1192)")]
        SUITABLE_FOR_PIM_AUTHORIZATION,
        [Description("AIM authorization (PAS 1192)")]
        SUITABLE_FOR_AIM_AUTHORIZATION,
        [Description("Costing (PAS 1192)")]
        SUITABLE_FOR_COSTING,
        [Description("Tender (PAS 1192)")]
        SUITABLE_FOR_TENDER,
        [Description("Contractor design (PAS 1192)")]
        SUITABLE_FOR_CONTRACTOR_DESIGN,
        [Description("Manufacture procurement (PAS 1192)")]
        SUITABLE_FOR_MANUFACTURE_PROCUREMENT,
        [Description("Construction (PAS 1192)")]
        SUITABLE_FOR_CONSTRUCTION,
        [Description("AM Inspection container")]
        SUITABLE_FOR_AM_INSPECTION,
        [Description("AM Maintenance container")]
        SUITABLE_FOR_AM_MAINTENANCE,
        [Description("Requirements container")]
        SUITABLE_FOR_REQUIREMENTS
    }
}
