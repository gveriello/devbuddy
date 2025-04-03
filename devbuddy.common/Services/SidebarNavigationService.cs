using System.Reflection;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.common.Services
{
    public class SidebarNavigationService
    {
        public ModulesItems SelectedNavNode { get; set; } = ModulesItems.Home;
        public string SelectedNavDescription => SelectedNavNode.GetType().GetCustomAttribute<DynamicallyLoadableAttribute>()?.NavDescription ?? SelectedNavNode.ToString();
        public ModulesItems? Parent { get; set; }
        public Type? FunctionalityType { get; private set; }
        public string Path { get; private set; }

        public event Action OnChange;
        private MemoryCacheService CacheService { get; set; }

        public SidebarNavigationService(MemoryCacheService memoryCacheService)
        {
            this.CacheService = memoryCacheService;
        }

        public void SetMenuItem(ModulesItems navNode, string navDescription)
        {
            SelectedNavNode = navNode;

            if (!CacheService.TryGetValueIfIsNotExpired(nameof(Assembly.GetExecutingAssembly), out List<Type> currentAssemblies))
            {
                return;
            }

            foreach (var assembly in currentAssemblies!)
            {
                var loadableComponent = assembly.GetCustomAttribute<DynamicallyLoadableAttribute>();
                if (loadableComponent?.NavNode == navNode && loadableComponent?.NavDescription == navDescription)
                {
                    FunctionalityType = assembly;
                    Parent = loadableComponent?.NavParent;
                    return;
                }
            }

            OnChange?.Invoke();
        }
    }
}
