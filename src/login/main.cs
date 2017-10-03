// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder
using n_core;
using n_db;
using n_plugins;
using n_socket;
using n_timer;
using n_version;

namespace main
{
    class main
    {
        static string SERVER_NAME = null;
        static version.e_server_type SERVER_TYPE = version.e_server_type.NONE;
        static timer timer;
        static db db;
        static plugins plugins;

        /// <summary>
        /// MAINROUTINE for login server
        /// </summary>
        static int Main(string[] args)
        {
            // initialize program arguments
            if(args.Length >= 1)
                SERVER_NAME = args[0];

            SERVER_TYPE = version.e_server_type.LOGIN;
            core.display_title();

            db = new db();
            timer = new timer();
            socket.socket_init(timer);
            plugins = new plugins();

            return 0;
        }
    }
}