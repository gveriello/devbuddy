using System.ComponentModel;
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
    }
}
