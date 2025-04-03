using System.ComponentModel;
using devbuddy.common.Enums;
using devbuddy.common.ExtensionMethods;

namespace devbuddy.common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DynamicallyLoadableAttribute : Attribute
    {
        public DynamicallyLoadableAttribute(ModulesItems navNode, ModulesItems parent = ModulesItems.Unparented)
        {
            this.NavNode = navNode;
            this.NavDescription = navNode.AttributeValueOrDefault<DescriptionAttribute, string>(attr => attr.Description) ?? navNode.ToString();
            this.NavParent = parent;
        }

        public ModulesItems NavNode { get; set; }
        public string NavDescription { get; init; }
        public ModulesItems? NavParent { get; set; } = null;
    }
}
