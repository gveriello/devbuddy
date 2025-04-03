using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace devbuddy.common.ExtensionMethods
{
    public static class EnumExtensionMethods
    {
        public static TValue AttributeValueOrDefault<TAttribute, TValue>(this Enum enumValue, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<TAttribute>();
            return attribute != null ? valueSelector(attribute) : default;
        }

        public static bool IsRequired(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<RequiredAttribute>();
            return attribute != null;
        }
    }
}
