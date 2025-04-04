using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages.Plugins
{
    [DynamicallyLoadable(ModulesItems.Base64Encoder)]
    public sealed partial class Base64Encoder
    {
    }
}
