using devbuddy.common.Enums;

namespace devbuddy.common.Attributes
{
    public class NavParentAttribute(ModulesItems parent = ModulesItems.Unparented) : Attribute
    {
        public ModulesItems Parent { get; set; } = parent;
    }
}
