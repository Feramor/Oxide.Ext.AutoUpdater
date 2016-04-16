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

namespace Oxide.Ext.AutoUpdater
{
    public class AutoUpdater : Extension
    {
        public AutoUpdater(ExtensionManager manager) : base(manager) { }

        public override string Name => "AutoUpdater";

        public override VersionNumber Version => new VersionNumber(1, 0, 0);

        public override string Author => "Feramor";

        private Libraries.AutoUpdater _AutoUpdater;

        public override void Load()
        {
            Manager.RegisterLibrary("AutoUpdater", _AutoUpdater = new Libraries.AutoUpdater());
            Manager.RegisterPluginLoader(new Plugins.AutoUpdaterLoader());
        }
        public override void LoadPluginWatchers(string plugindir)
        {
            _AutoUpdater?.Initialize();
        }

        public override void OnShutdown()
        {
            _AutoUpdater?.Shutdown();
        }
        private void LoadAssembly(string Name, byte[] RawAssemblyByte)
        {
            try
            {
                Assembly.Load(RawAssemblyByte);
            }
            catch
            {
                Interface.Oxide.LogError("[AutoUpdater] Failed to load an Assembly ({0}).", Name);
            }
        }
    }
}
