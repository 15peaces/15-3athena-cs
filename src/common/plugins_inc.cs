// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder

// Common plugin system include.

namespace n_plugins_inc
{
    public class plugin_inc
    {
        // Plugin version <major version>.<minor version>
        // * <major version> is increased and <minor version> reset when at least one 
        //   export of the previous version becomes incompatible
        // * <minor version> is increased if the previous version remains compatible
        // 
        // Compatible plugins have:
        // - equal major version
        // - lower or equal minor version
        public const string PLUGIN_VERSION = "1.04";

        public struct PluginInfo
        {
            public string name;
            public PLUGIN_SERVER server;
            public string version;
            public string min_ver;
            public string desc;
        }

        public enum PLUGIN_SERVER
        {
            ALL,
            LOGIN,
            CHAR,
            MAP,
            CORE
        };
    }
}
