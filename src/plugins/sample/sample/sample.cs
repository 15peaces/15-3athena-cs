// Sample Athena plugin - c#

using System;
using n_plugins_inc;
using RGiesecke.DllExport;
using System.Runtime.InteropServices;

namespace sample
{
    class SampleClass
    {
        static plugin_inc.PluginInfo info;

        ////// Constructor ////////
        public SampleClass()
        {
            ////// Plugin information ////////
            info = new plugin_inc.PluginInfo();

            // change only the following area [
            info.name = "Test";                         // Plugin name
            info.server = plugin_inc.PLUGIN_SERVER.ALL; // Which servers is this plugin for
            info.version = "0.2";                       // Plugin version
            info.min_ver = plugin_inc.PLUGIN_VERSION;   // Minimum plugin engine version to run
            info.desc = "A sample plugin";              // Short description of plugin
            // ]
            return;
        }

        [DllExport("GetPluginInfo", CallingConvention = CallingConvention.Cdecl)]
        public static void GetPluginInfo(ref plugin_inc.PluginInfo iStruct)
        {
            iStruct = info;
            return;
        }

        public void Output(string s)
        {
            Console.WriteLine(s);
        }
    }
}
