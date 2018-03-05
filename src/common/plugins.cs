// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder

using showmsg;
using n_plugins_inc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace n_plugins
{
    public class plugins
    {
        const string DLL_EXT = ".dll";

        static short auto_search = 0;

        static List<PluginList> Plugins;

        struct PluginList
        {
            ushort id;
            public Plugin plugin;
            public plugin_inc.PluginInfo info;
        }

        struct Plugin
        {
            public string filename;
            public short state;
            public IntPtr dll;
        }

        static plugins()
        {
            string PLUGIN_CONF_FILENAME = @"conf\plugin_athena.conf";

            Plugins = new List<PluginList>();

            config_read(PLUGIN_CONF_FILENAME);
        }

        static class DllFunc
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);


            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }

        static private void config_read(string cfgName)
        {
            string[] lines;
            string[] w;
            StreamReader fs;

            string file = Directory.GetCurrentDirectory() + @"\" + cfgName;

            if (!File.Exists(file))
            {
                console.error("File not found: " + cfgName);
                return;
            }

            fs = new StreamReader(file);
            lines = fs.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.GetLength(0); i++)
            {
                if (lines[i].StartsWith("//"))
                    continue;

                w = lines[i].Split(':');
                if (w.Length != 2)
                    continue;

                w[0] = w[0].Trim();
                w[1] = w[1].Trim();

                switch (w[0])
                {
                    case "auto_search":
                        auto_search = n_common.common.config_switch(w[1]);
                        break;
                    case "plugin":
                        plugin_load(@"plugins\" + w[1] + DLL_EXT);
                        break;
                    case "import":
                        config_read(w[1]);
                        break;
                    default:
                        break;
                }
            }
            return;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetPluginInfo(ref plugin_inc.PluginInfo iStruct);

        static Plugin? plugin_load(string filename)
        {
            Plugin plugin;
            plugin_inc.PluginInfo info;

            console.debug("plugin_load("+filename+")");

            // Check if the plugin has been loaded before
            for (int i = 0; i < Plugins.Count; i++)
            {
                // returns handle to the already loaded plugin
                if (Plugins[i].plugin.state > 0 && Plugins[i].plugin.filename == filename)
                {
                    console.warning("plugin_load: not loaded (duplicate) : '"+filename+"'");
                    return Plugins[i].plugin;
                }
            }

            plugin = new Plugin();
            info = new plugin_inc.PluginInfo();
            plugin.filename = filename;
            plugin.state = -1;  // not loaded

            plugin.dll = DllFunc.LoadLibrary(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename));
            if (plugin.dll == IntPtr.Zero)
            {
                console.warning("plugin_load: not loaded (invalid file) : '" + filename + "'");
                return null;
            }

            // Retrieve plugin information
            plugin.state = 0;  // initialising

            IntPtr GetInfoFunc = DllFunc.GetProcAddress(plugin.dll, "GetPluginInfo");
            if (GetInfoFunc == IntPtr.Zero)
            {
                console.debug("plugin_load: plugin_info not found");
                return null;
            }

            GetPluginInfo GetPluginInfo = (GetPluginInfo)Marshal.GetDelegateForFunctionPointer(GetInfoFunc,typeof(GetPluginInfo));
            GetPluginInfo(ref info);

            console.debug("plugin_load: Found plugin: "+info.name+" Version "+info.version+".");

            /*
            // For high priority plugins (those that are explicitly loaded from the conf file)
            // we'll ignore them even (could be a 3rd party dll file)
            if (!info)
            {// foreign plugin
             //ShowDebug("plugin_open: plugin_info not found\n");
                if (load_priority == 0)
                {// not requested
                 //ShowDebug("plugin_open: not loaded (not requested) : '"CL_WHITE"%s"CL_RESET"'\n", filename);
                    plugin_unload(plugin);
                    return NULL;
                }
            }
            else if (!plugin_iscompatible(info->req_version))
            {// incompatible version
                ShowWarning("plugin_open: not loaded (incompatible version '%s' -> '%s') : '"CL_WHITE"%s"CL_RESET"'\n", info->req_version, PLUGIN_VERSION, filename);
                plugin_unload(plugin);
                return NULL;
            }
            else if ((info->type != PLUGIN_ALL && info->type != PLUGIN_CORE && info->type != SERVER_TYPE) ||
              (info->type == PLUGIN_CORE && SERVER_TYPE != PLUGIN_LOGIN && SERVER_TYPE != PLUGIN_CHAR && SERVER_TYPE != PLUGIN_MAP))
            {// not for this server
             //ShowDebug("plugin_open: not loaded (incompatible) : '"CL_WHITE"%s"CL_RESET"'\n", filename);
                plugin_unload(plugin);
                return NULL;
            }
            */

            return plugin;
        }
    }
}
