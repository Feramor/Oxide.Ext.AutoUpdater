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
    public class AutoUpdater : CSPlugin
    {
        public AutoUpdater()
        {
            Name = "AutoUpdater";
            Title = "AutoUpdater";
            Author = "Feramor";
            Version = new VersionNumber(1, 0, 0);
        }

        [HookMethod("OnServerInitialized")]
        private void OnServerInitialized()
        {
            Oxide.Ext.AutoUpdater.Libraries.AutoUpdater _AutoUpdater = Interface.Oxide.GetLibrary<Oxide.Ext.AutoUpdater.Libraries.AutoUpdater>("AutoUpdater");
            _AutoUpdater.StartWork();
        }
    }
}
