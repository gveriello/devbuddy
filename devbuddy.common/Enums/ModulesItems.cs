using System.ComponentModel.DataAnnotations;
using devbuddy.common.Attributes;

namespace devbuddy.common.Enums
{
    public enum ModulesItems
    {
        Unparented,

        [Required]
        [Icon("fa-house")]
        Home,

        [ModuleKey("5ef20b73-22f7-4da1-a56a-e7a86853aa39")]
        Settings,

        [ModuleKey("45e7bf11-4192-40c0-b4ea-b2701fa345f4")]
        JsonFormatters,

        [ModuleKey("642746dc-cc2a-45dc-b2f7-d6d67e8696e4")]
        QueryOptimizer,

        [ModuleKey("f84a2729-4c37-4eb3-ac89-3911752b414e")]
        Base64Encoder,

        [ModuleKey("53253590-e83c-4f9c-ab3e-8a4cdc25b8fb")]
        QRcodeEncoder,
    }
}
