// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15-3athena Dev Team 2017
// For more information, see LICENCE in the main folder
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using n_mmo;
using n_timer;
using showmsg;

namespace n_socket
{
    public class socket
    {
        const uint FD_SETSIZE = 4096;

        // initial recv buffer size (this will also be the max. size)
        // biggest known packet: S 0153 <len>.w <emblem data>.?B -> 24x24 256 color .bmp (0153 + len.w + 1618/1654/1756 bytes)
        const int RFIFO_SIZE = (2 * 1024);
        // initial send buffer size (will be resized as needed)
        const int WFIFO_SIZE = (16 * 1024);

        static uint socket_max_client_packet;

        static short[] addr_;   // ip addresses of local host (host byte order)
        static int naddr_ = 0;   // # of ip addresses
        static long stall_time = 60;
        static int ip_rules = 1;

        static UInt64 last_tick;

        static socket_data[] session;

        int[] send_shortlist_array;
        int send_shortlist_count = 0;
        static uint[] send_shortlist_set;

        struct socket_data
        {
            struct flag
            {
                byte eof;
                byte server;
                char ping;
            }

            IPAddress client_addr;

            public int max_rdata, max_wdata;
            public int rdata_size, wdata_size;
            public int rdata_pos;
            public UInt64 rdata_tick; // time of last recv (for detecting timeouts); zero when timeout is disabled

            public string func_recv;
            public string func_send;
            public string func_parse;

            object session_data; // stores application-specific data related to the session            
        }

        //////////////////////////////
        // IP rules and DDoS protection
        struct AccessControl
        {
            public static bool init;
            public IPAddress ip;
            public IPAddress mask;

            static AccessControl()
            {
                init = true;
            }

            public bool is_init()
            {
                return init;
            }
        }

        struct ConnectHistory
        {
	        IPAddress ip;
            UInt64 tick;
            int count;
            byte ddos;
        }

        enum _aco
        {
            ACO_DENY_ALLOW,
            ACO_ALLOW_DENY,
            ACO_MUTUAL_FAILURE
        }

        static ConnectHistory[] connect_history;

        static int access_allownum = 0;
        static int access_denynum = 0;
        static int ddos_interval = 0;
        static int ddos_count = 0;
        static int ddos_autoreset = 0;

        static bool access_debug = true;
        static _aco access_order = _aco.ACO_DENY_ALLOW;
        private static AccessControl _tmp;
        static List<AccessControl> access_allow;
        static List<AccessControl> access_deny;

        static void create_session(int fd, string func_recv = "", string func_send = "", string func_parse = "")
        {
            session[fd].max_rdata = RFIFO_SIZE;
            session[fd].max_wdata = WFIFO_SIZE;
            session[fd].func_recv = func_recv;
            session[fd].func_send = func_send;
            session[fd].func_parse = func_parse;
            session[fd].rdata_tick = last_tick;
            return;
        }

        /// Parses the ip address and mask and puts it into acc.
        static AccessControl access_ipmask(string str)
        {
            IPAddress ip, mask;
            AccessControl ret = new AccessControl();
            string[] ip_mask;
            string[] ip_str;
            string[] mask_str;

            str = str.ToLower();

            if (str.Contains("all"))
            {
                ip = IPAddress.Any;
                mask = IPAddress.Any;
            }
            else
            {
                ip_mask = str.Split('/');
                ip_str = ip_mask[0].Split('.');

                if (ip_str.Length != 4)
                { // not an ip
                    return ret;
                }

                if (Convert.ToUInt16(ip_str[0]) > 255 || Convert.ToUInt16(ip_str[1]) > 255 || Convert.ToUInt16(ip_str[2]) > 255 || Convert.ToUInt16(ip_str[3]) > 255)
                { // invalid ip
                    return ret;
                }

                if (ip_mask.Length > 1)
                {
                    mask_str = ip_mask[1].Split('.');

                    if (mask_str.Length == 1)
                    {
                        if (Convert.ToUInt16(mask_str[0]) > 32)
                        { // invalid bit mask
                            return ret;
                        }

                        mask = IPAddress.Parse(str);
                    }
                    else if (mask_str.Length == 4)
                    {
                        if (Convert.ToUInt16(mask_str[0]) > 255 || Convert.ToUInt16(mask_str[1]) > 255 || Convert.ToUInt16(mask_str[2]) > 255 || Convert.ToUInt16(mask_str[3]) > 255)
                        { // invalid standard mask
                            return ret;
                        }

                        mask = IPAddress.Parse(ip_mask[1]);
                    }
                    else
                    { // not an standard mask || not an bit mask
                        return ret;
                    }
                }
                else
                {
                    mask = IPAddress.Parse("255.255.255.255");
                }

                ip = IPAddress.Parse(ip_mask[0]);

                if (access_debug)
                {
                    console.information("socket.access_ipmask: Loaded IP: " + ip.ToString() + " mask:" + mask.ToString());
                }
            }
            ret.ip = ip;
            ret.mask = mask;

            return ret;
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

            for(int i = 0; i < lines.GetLength(0); i++)
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
                    case "stall_time":
                        stall_time = Convert.ToInt64(w[1]);
                        if (stall_time < 3)
                        {
                            stall_time = 3;/* a minimum is required to refrain it from killing itself */
                        }
                        break;
                    case "enable_ip_rules":
                        ip_rules = n_common.common.config_switch(w[1]);
                        break;
                    case "order":
                        switch (w[1])
                        {
                            case "deny,allow":
                                access_order = _aco.ACO_DENY_ALLOW;
                                break;
                            case "allow,deny":
                                access_order = _aco.ACO_ALLOW_DENY;
                                break;
                            case "mutual-failure":
                                access_order = _aco.ACO_MUTUAL_FAILURE;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "allow":
                        _tmp = access_ipmask(w[1]);
                        if (_tmp.is_init())
                        {
                            ++access_allownum;
                            access_allow.Add(_tmp);
                        }
                        else
                        {
                            console.error("socket.config_read: Invalid ip or ip range '" + lines[i] + "'!");
                        }
                        console.debug("socket.config_read: Last-read AccessControl: IP: " + _tmp.ip.ToString() + " MASK: " + _tmp.mask.ToString());
                        break;
                    case "deny":
                        _tmp = access_ipmask(w[1]);
                        if (_tmp.is_init())
                        {
                            ++access_denynum;
                            access_deny.Add(_tmp);
                        }
                        else
                        {
                            console.error("socket.config_read: Invalid ip or ip range '" + lines[i] + "'!");
                        }
                        break;
                    case "ddos_interval":
                        ddos_interval = Convert.ToInt32(w[1]);
                        break;
                    case "ddos_count":
                        ddos_count = Convert.ToInt32(w[1]);
                        break;
                    case "ddos_autoreset":
                        ddos_autoreset = Convert.ToInt32(w[1]);
                        break;
                    case "access_debug":
                        access_debug = Convert.ToBoolean(w[1]);
                        break;
                    case "socket_max_client_packet":
                        socket_max_client_packet = Convert.ToUInt32(w[1]);
                        if (mmo.PACKETVER < 20131223)
                        {
                            if (socket_max_client_packet > 24576)
                            {
                                console.warning("socket_max_client_packet: Value " + socket_max_client_packet + " is to high. Defaulting to 24576.");
                                console.warning("socket_max_client_packet: If you want to use this value consider upgrading your client to 2013-12-23 or newer.");
                                socket_max_client_packet = 24576;
                            }
                        }
                        else
                        {
                            if (socket_max_client_packet > 65636)
                            {
                                console.warning("socket_max_client_packet: Value " + socket_max_client_packet + " is to high. Defaulting to 65636.");
                                socket_max_client_packet = 65636;
                            }
                        }
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

        /// Retrieve local ips in host byte order.
        /// Uses loopback if no address is found.
        static private int getips(ref short[] ips, int max)
        {
            int num = 0;
            string fullhost = "";
            IPHostEntry hent;
            IPAddress[] a;

            if (ips == null || max <= 0)
            {
                return 0;
            }

            fullhost = Dns.GetHostName();

            if (fullhost == "")
            {
                console.error("socket.getips: No hostname defined!");
                return 0;
            }
            else
            {
                hent = Dns.GetHostEntry(fullhost);
                if (hent == null)
                {
                    console.error("socket.getips: Cannot resolve our own hostname to an IP address\n");
                    return 0;
                }
                a = hent.AddressList;

                for ( ; num < a.Length && num < max; ++num)
                {

                    ips[num] = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(a[num].GetAddressBytes(), 0));
                }
            }

            // Use loopback if no ips are found
            if (num == 0)
            {
                ips[num++] = BitConverter.ToInt16(IPAddress.Loopback.GetAddressBytes(), 0);
            }

            return num;
        }

        public static void socket_init(timer timer)
        {
            // Maximum packet size in bytes, which the client is able to handle.
            // Larger packets cause a buffer overflow and stack corruption.
            if (mmo.PACKETVER < 20131223)
            {
                socket_max_client_packet = 24576;
            }
            else
            {
                socket_max_client_packet = 65636;
            }            

            string SOCKET_CONF_FILENAME = @"conf\packet_athena.conf";
            access_allow = new List<AccessControl>();
            access_deny = new List<AccessControl>();

            session = new socket_data[FD_SETSIZE];

            // Get initial local ips
            addr_ = new short[16];
            naddr_ = getips(ref addr_, 16);

            send_shortlist_set = new uint[(FD_SETSIZE + 31) / 32];

            config_read(SOCKET_CONF_FILENAME);

            // initialise last send-receive tick
            last_tick = timer.time(null);

            // session[0] is now currently used for disconnected sessions of the map server, and as such,
            // should hold enough buffer (it is a vacuum so to speak) as it is never flushed. [Skotlex]
            create_session(0);

            // Delete old connection history every 5 minutes
            connect_history = new ConnectHistory[0x10000];
            timer.add_timer_interval(timer.time(null) + 1000, "connect_check_clear", null , 0, 5 * 60 * 1000);

            console.information("Server supports up to '"+FD_SETSIZE+"' concurrent connections.\n");
        }
    }
}
