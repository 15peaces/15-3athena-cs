// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder
using System;
using System.IO;

using showmsg;

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
        const string LOGIN_CONF_NAME = "conf/login_athena.conf";

        static string SERVER_NAME = null;
        static version.e_server_type SERVER_TYPE = version.e_server_type.NONE;
        static timer timer;
        static db db;
        static plugins plugins;

        struct Login_Config
        {

        };

        /// <summary>
        /// MAINROUTINE
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

            do_init((args.Length >= 1)?args[1] : LOGIN_CONF_NAME);

            return 0;
        }

        /// <summary>
        /// Login server initialization
        /// </summary>
        static private void do_init(string conffile)
        {
            set_defaults();
            config_read(conffile);
            return;
        }

        static private void set_defaults()
        {
            return;
        }

        /// <summary>
        /// Reading login servers main configuration file
        /// </summary>
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
                    default:
                        break;
                }
            }
            return;
        }
    }
}