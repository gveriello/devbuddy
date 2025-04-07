using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages.Plugins
{
    [DynamicallyLoadable(ModulesItems.YamlFormatter)]
    public sealed partial class YamlFormatter : PageComponentsBase
    {
    }
}
