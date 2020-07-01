// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder
using System;
using System.IO;
using System.Collections.Generic;

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
        const int MAX_SERVERS = 1;
        const string LOGIN_CONF_NAME = "conf/login_athena.conf";
        const string LAN_CONF_NAME = "conf/subnet_athena.conf";

        static string SERVER_NAME = null;
        static version.e_server_type SERVER_TYPE = version.e_server_type.NONE;
        static c_socket socket;
        static timer timer;
        static db db;
        static plugins plugins;
        static Login_Config login_config;

        static mmo_char_server[] server; // char server data

        class Login_Config
        {
            public uint ip_sync_interval;
            public bool log_login;
            public bool ipban;
        }

        class mmo_char_server
        {
            public int fd;

            public mmo_char_server()
            {
                fd = -1;
            }
        }

        /// <summary>
        /// Auth database
        /// </summary>
        class auth_node
        {

        }
        static List<auth_node> auth_db;

        /// <summary>
        /// Online User Database
        /// </summary>
        class online_login_data
        {
            public int char_server;
        }
        static List<online_login_data> online_db;

        /// <summary>
        /// MAINROUTINE
        /// </summary>
        static int Main(string[] args)
        {
            // initialize program arguments
            if (args.Length >= 1)
                SERVER_NAME = args[0];

            SERVER_TYPE = version.e_server_type.LOGIN;
            core.display_title();

            //db = new db();
            timer = new timer();
            socket = new c_socket(timer);
            plugins = new plugins();

            do_init((args.Length >= 1) ? args[1] : LOGIN_CONF_NAME, (args.Length >= 2) ? args[2] : LAN_CONF_NAME);

            return 0;
        }

        /// <summary>
        /// Login server initialization
        /// </summary>
        static private void do_init(string login_conf, string lan_conf)
        {
            set_defaults();
            config_read(login_conf);
            lan_config_read(lan_conf);

            server = new mmo_char_server[MAX_SERVERS];

            // initialize logging
            if (login_config.log_login)
                loginlog_init();

            // initialize static and dynamic ipban system
            if (login_config.ipban)
                ipban_init();

            // Online user database init
            online_db = new List<online_login_data>();

            // Interserver auth init
            auth_db = new List<auth_node>();

            // set default parser as parse_login function
            socket.set_defaultparse("parse_login");

            // every 10 minutes cleanup online account db.
            timer.add_timer_interval(timer.time(null) + 600 * 1000, "online_data_cleanup", null, 0, 600 * 1000);

            // add timer to detect ip address change and perform update
            if (login_config.ip_sync_interval > 0)
                timer.add_timer_interval(timer.time(null) + login_config.ip_sync_interval, "sync_ip_addresses", null, 0, (int)login_config.ip_sync_interval);

            return;
        }

        static void loginlog_init()
        {
            return;
        }

        static void ipban_init()
        {
            return;
        }

        static private void set_defaults()
        {
            login_config = new Login_Config();

            login_config.ip_sync_interval = 0;
            login_config.log_login = true;
            login_config.ipban = true;
            return;
        }

        static void online_data_cleanup_sub()
        {

        }

        static public void online_data_cleanup()
        {
            int i;

            for(i = 0; i < online_db.Count; i++)
            {
                if (online_db[i].char_server == -2)
                    online_db.Remove(online_db[i]); 
            }
        }

        /// <summary>
        // Packet send to all char-servers, except one (wos: without our self)
        /// </summary>
        static int charif_sendallwos(int sfd, short buf, int len)
        {
            int i, c;

            for (i = 0, c = 0; i < server.Length; ++i)
            {
                int fd = server[i].fd;
                if (socket.session_isValid(fd) && fd != sfd)
                {
                    socket.WFIFOHEAD(fd, len);
                    socket.session[fd].wdata+=(ushort)buf; 
                    socket.WFIFOSET(fd, len);
                    ++c;
                }
            }

            return c;
        }

        /// <summary>
        /// periodic ip address synchronization
        /// </summary>
        static void sync_ip_addresses()
        {
            short buf;
            console.info("IP Sync in progress...\n");
            buf = 0x2735;
            charif_sendallwos(-1, buf, 2);
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
                console.error("Configuration file ("+cfgName+") not found.");
                return;
            }

            console.info("Reading configuration file "+cfgName+"...");

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
                    case "log_login":
                        login_config.log_login = n_common.common.config_switch_bool(w[1]);
                        break;
                    case "ipban.enable":
                        login_config.ipban = n_common.common.config_switch_bool(w[1]);
                        break;
                    case "ip_sync_interval":
                        login_config.ip_sync_interval = Convert.ToUInt32(w[1]);
                        break;
                    default:
                        break;
                }
            }
            fs.Close();
            console.info("Finished reading " + cfgName + ".");
            return;
        }

        /// <summary>
        /// Reading Lan Support configuration
        /// </summary>
        static private void lan_config_read(string cfgName)
        {
            string[] lines;
            string[] w;
            StreamReader fs;

            string file = Directory.GetCurrentDirectory() + @"\" + cfgName;

            if (!File.Exists(file))
            {
                console.warning("LAN Support configuration file is not found: " + cfgName);
                return;
            }

            console.info("Reading configuration file " + cfgName + "...");

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
            fs.Close();
            console.info("Finished reading " + cfgName + ".");
            return;
        }
    }
}