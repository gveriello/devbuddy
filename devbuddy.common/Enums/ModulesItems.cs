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
        Base64,

        [ModuleKey("53253590-e83c-4f9c-ab3e-8a4cdc25b8fb")]
        QRCode,

        [ModuleKey("49641eb4-10d5-459b-aafc-f1ab50281360")]
        CodeMetricsAnalyzer,

        [ModuleKey("753857f1-b31d-4e80-9c8e-f3a2dc123e68")]
        CronExpression,

        [ModuleKey("874e9d5f-a2b3-48c7-9d5e-8b6f21c0e379")]
        YamlFormatter,

        [ModuleKey("62e8bbea-9b10-4687-a65e-4fc5633db014")]
        XmlFormatter,

        [ModuleKey("9ac57f2b-6d8e-4b1c-9a0f-854c7c3b5e1a")]
        DiagramEditor,

        [ModuleKey("38b6793f-3f2a-49ce-9583-ef98c811399c")]
        Notes,

        [ModuleKey("ef65f3ff-e74f-4e9a-8c54-bd3c19f91a85")]
        Markdown,

        [ModuleKey("ca5b8a9e-6e5f-4a3d-8c1e-0f7e97b4ab2d")]
        LoremIpsum,

        [ModuleKey("5502bed8-22f5-412a-8a3b-ad5009ff1117")]
        SqlFormatter,

        [ModuleKey("659cba2c-3307-4f34-82c6-ce8048fb236c")]
        Regex,
    }
}
