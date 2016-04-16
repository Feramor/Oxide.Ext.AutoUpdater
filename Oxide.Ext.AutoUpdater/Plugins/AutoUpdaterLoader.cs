using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

//Oxide
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Oxide.Ext.AutoUpdater.Plugins
{
    public class AutoUpdaterLoader : PluginLoader
    {
        public override Type[] CorePlugins => new[] { typeof(AutoUpdater) };
    }
}
