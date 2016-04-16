using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

//Oxide
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Oxide.Ext.AutoUpdater.Libraries
{
    public class AutoUpdater : Library
    {
        private bool _running = true;
        private List<Core.Plugins.Plugin> LoadedPlugins;
        private List<Core.Plugins.Plugin> UpdateNeeded;
        private Thread WorkerThread = null;
        private WebRequests webRequests;

        private bool ServerInitialized;
        private void OnPluginAdded(Core.Plugins.Plugin plugin)
        {
            if (!LoadedPlugins.Contains(plugin))
            {
                string PluginType = GetType(plugin.Filename);
                if (ServerInitialized)
                    Interface.Oxide.LogInfo(string.Format("[AutoUpdater] Plugin Detected {0} : {1} Type : {2}", plugin.Name, plugin.Version.ToString(), PluginType));
                if ((PluginType != "Not Supported") && (plugin.ResourceId != 0))
                    LoadedPlugins.Add(plugin);
            }
        }
        private void OnPluginRemoved(Core.Plugins.Plugin plugin)
        {
            if (LoadedPlugins.Contains(plugin))
            {
                LoadedPlugins.Remove(plugin);
            }
        }

        internal void Initialize()
        {
            Interface.Oxide.LogInfo("[AutoUpdater] Initialized");
            ServerInitialized = false;
            LoadedPlugins = new List<Core.Plugins.Plugin>();
            UpdateNeeded = new List<Core.Plugins.Plugin>();

            Interface.Oxide.RootPluginManager.OnPluginAdded += OnPluginAdded;
            Interface.Oxide.RootPluginManager.OnPluginRemoved += OnPluginRemoved;

            webRequests = Interface.Oxide.GetLibrary<WebRequests>();

            if ((WorkerThread == null) || (!WorkerThread.IsAlive))
            {
                WorkerThread = new Thread(Worker);
                WorkerThread.IsBackground = true;
            }
        }
        internal void StartWork()
        {
            foreach (Core.Plugins.Plugin CurrentPlugin in LoadedPlugins)
            {
                string PluginType = GetType(CurrentPlugin.Filename);
                Interface.Oxide.LogInfo(string.Format("[AutoUpdater] Plugin Detected {0} : {1} Type : {2}", CurrentPlugin.Name, CurrentPlugin.Version.ToString(), PluginType));
            }
            ServerInitialized = true;
            if ((WorkerThread != null) || (!WorkerThread.IsAlive))
            {
                WorkerThread.Start();
            }
        }

        private void Worker()
        {
            Interface.Oxide.LogInfo("[AutoUpdater] Starting Updater.");
            do
            {
                Interface.Oxide.LogInfo("[AutoUpdater] Checking for plugin updates.");
                UpdateNeeded.Clear();
                int ResponseCount = 0;
                foreach (Core.Plugins.Plugin CurrentPlugin in LoadedPlugins)
                {
                    Thread.Sleep(100);
                    webRequests.EnqueueGet($"http://oxidemod.org/plugins/{CurrentPlugin.ResourceId}/", (code, response) =>
                    {
                        if (code == 200 && response != null)
                        {
                            Version latest = new Version("0.0.0.0");

                            Match version = new Regex(@"<h3>Version (.*?)<\/h3>").Match(response);
                            if (version.Success)
                            {
                                string _Version = version.Groups[1].ToString();
                                while (_Version.Count(c => c == '.') < 3)
                                    _Version += ".0";
                                latest = new Version(_Version);
                            }
                            if (latest > new Version(CurrentPlugin.Version.Major,CurrentPlugin.Version.Minor,CurrentPlugin.Version.Patch,0))
                            {
                                UpdateNeeded.Add(CurrentPlugin);
                                Interface.Oxide.LogWarning("[AutoUpdater] Update needed for {0} (Current : {1},Latest : {2})",CurrentPlugin.Name, new Version(CurrentPlugin.Version.Major, CurrentPlugin.Version.Minor, CurrentPlugin.Version.Patch, 0), latest);
                            }
                        }
                        ResponseCount++;
                    }, null);
                }
                do
                {
                    Thread.Sleep(1000);
                } while (ResponseCount < LoadedPlugins.Count);
                if (UpdateNeeded.Count > 0)
                {
                    Interface.Oxide.LogInfo("[AutoUpdater] Update started for {0} plugin(s).", UpdateNeeded.Count);
                    foreach (Core.Plugins.Plugin CurrentPlugin in UpdateNeeded)
                    {
                        using (WebClient client = new WebClient())
                        {
                            byte[] _plugin = client.DownloadData("http://rust.feramor.gen.tr/OxideUpdate.php?ResourceID=" + CurrentPlugin.ResourceId);
                            string fileName = string.Empty;
                            string contentDisposition = client.ResponseHeaders["Content-Disposition"];

                            string _UpdateDirectory = Path.Combine(Interface.Oxide.PluginDirectory, "Update");
                            if (!Directory.Exists(_UpdateDirectory))
                                Directory.CreateDirectory(_UpdateDirectory);

                            Match FileName = new Regex("attachment; filename=\"(.*?)\"").Match(contentDisposition);

                            if (FileName.Success)
                            {
                                Interface.Oxide.LogInfo("[AutoUpdater] Updating {0}.", FileName.Groups[1].ToString());
                                File.WriteAllBytes(Path.Combine(_UpdateDirectory, string.Format("{0}.UpdatedNow", FileName.Groups[1].ToString())), _plugin);
                                File.Copy(CurrentPlugin.Filename, string.Format("{0}.V.{1}.bak", CurrentPlugin.Filename,(new Version(CurrentPlugin.Version.Major, CurrentPlugin.Version.Minor, CurrentPlugin.Version.Patch, 0)).ToString()));
                                File.Delete(CurrentPlugin.Filename);
                                File.Move(Path.Combine(_UpdateDirectory, string.Format("{0}.UpdatedNow", FileName.Groups[1].ToString())), Path.Combine(Interface.Oxide.PluginDirectory, string.Format("{0}", FileName.Groups[1].ToString())));
                            }
                        }
                    }
                }
                else
                {
                    Interface.Oxide.LogInfo("[AutoUpdater] All of the plugins are up to date.", UpdateNeeded.Count);
                }
                Thread.Sleep(1200000);
            } while (_running);
        }

        internal void Shutdown()
        {
            Interface.Oxide.LogInfo("[AutoUpdater] Stopped");
            Interface.Oxide.RootPluginManager.OnPluginAdded -= OnPluginAdded;
            Interface.Oxide.RootPluginManager.OnPluginRemoved -= OnPluginRemoved;
            _running = false;
        }
        private string GetType(string FileName)
        {
            try
            {
                switch (System.IO.Path.GetExtension(FileName).ToLower())
                {
                    case ".cs":
                        return "C#";
                    case ".py":
                        return "Python";
                    case ".js":
                        return "JavaScript";
                    case ".lua":
                        return "Lua";
                    case ".coffee":
                        return "Coffee Script";
                    default:
                        return string.Format("Not Supported ({0})", System.IO.Path.GetExtension(FileName).ToLower());
                }
            }
            catch
            {
                return "Not Supported";
            }
        }
    }
}
