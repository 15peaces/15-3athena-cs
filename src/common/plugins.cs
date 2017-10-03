// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder

using System.Collections.Generic;

namespace n_plugins
{
    public class plugins
    {
        struct EVENT
        {
            public const string PLUGIN_INIT    = "Plugin_Init";  // Initialize the plugin
            public const string PLUGIN_FINAL   = "Plugin_Final"; // Finalize the plugin
            public const string ATHENA_INIT    = "Athena_Init";  // Server started
            public const string ATHENA_FINAL   = "Athena_Final"; // Server ended
        }

        enum E_SYMBOL
        {
            SERVER_TYPE,
            SERVER_NAME,
            ARG_C,
            ARG_V,
            RUNFLAG,
            GETTICK,
            GET_SVN_REVISION,
            ADD_TIMER,
            ADD_TIMER_INTERVAL,
            ADD_TIMER_FUNC_LIST,
            DELETE_TIMER,
            GET_UPTIME,
            ADDR,
            FD_MAX,
            SESSION,
            DELETE_SESSION,
            WFIFOSET,
            RFIFOSKIP,
            FUNC_PARSE_TABLE,
            // 1.03
            PARSE_CONSOLE
        }

        string PLUGIN_CONF_FILENAME = @"conf\plugin_athena.conf";

        static List<Plugin_Event_List> PluginEventList;

        struct Plugin_Event_List
        {
            public string name;
        }

        static List<Plugin_Call_Table> PluginCallTable;

        struct Plugin_Call_Table
        {
            public string var;
            public string[] args;
            public E_SYMBOL offset;
        }



        static void register_plugin_func(string name)
        {
            if (name != "")
            {
                PluginEventList.Add(new Plugin_Event_List { name = name });
            }
        }

        ////// Plugins Call Table Functions /////////

        static void export_symbol(string var, string[] args, E_SYMBOL offset)
        {
            // add to the end of the list
            if (offset < 0)
                offset = (E_SYMBOL)PluginCallTable.Count;

            PluginCallTable.Add(new Plugin_Call_Table { var = var, args = args, offset = offset });
        }

        static plugins()
        {
            // Sugested functionality:
            // add atcommands/script commands [Borf]
            PluginEventList = new List<Plugin_Event_List>();

            register_plugin_func(EVENT.PLUGIN_INIT);
            register_plugin_func(EVENT.PLUGIN_FINAL);
            register_plugin_func(EVENT.ATHENA_INIT);
            register_plugin_func(EVENT.ATHENA_FINAL);

            PluginCallTable = new List<Plugin_Call_Table>();

            // networking
            export_symbol("RFIFOSKIP", new string[2], E_SYMBOL.RFIFOSKIP);
            export_symbol("WFIFOSET", new string[2], E_SYMBOL.WFIFOSET);
            export_symbol("do_close", new string[1], E_SYMBOL.DELETE_SESSION);
            export_symbol("session", null, E_SYMBOL.SESSION);
            export_symbol("fd_max", null, E_SYMBOL.FD_MAX);
            export_symbol("addr_", null, E_SYMBOL.ADDR);
            // timers
            export_symbol("get_uptime", null, E_SYMBOL.GET_UPTIME);
            export_symbol("delete_timer", new string[2], E_SYMBOL.DELETE_TIMER);
            export_symbol("add_timer_func_list", new string[2], E_SYMBOL.ADD_TIMER_FUNC_LIST);
            export_symbol("add_timer", new string[4], E_SYMBOL.ADD_TIMER);
            export_symbol("get_svn_revision", null, E_SYMBOL.GET_SVN_REVISION);
            export_symbol("gettick", null, E_SYMBOL.GETTICK);
        }
    }
}
