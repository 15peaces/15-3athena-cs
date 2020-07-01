// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15-3athena Dev Team 2017
// For more information, see LICENCE in the main folder
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using n_common;
using n_mmo;
using n_timer;
using showmsg;

namespace n_socket
{
    public class c_socket
    {
        /// Use a shortlist of sockets instead of iterating all sessions for sockets 
        /// that have data to send or need eof handling.
        const bool SEND_SHORTLIST = true;

        const uint FD_SETSIZE = 4096;

        // initial recv buffer size (this will also be the max. size)
        // biggest known packet: S 0153 <len>.w <emblem data>.?B -> 24x24 256 color .bmp (0153 + len.w + 1618/1654/1756 bytes)
        const int RFIFO_SIZE = (2 * 1024);
        // initial send buffer size (will be resized as needed)
        const int WFIFO_SIZE = (16 * 1024);

        // Maximum size of pending data in the write fifo. (for non-server connections)
        // The connection is closed if it goes over the limit.
        const int WFIFO_MAX = (1 * 1024 * 1024);

        const int FIFOSIZE_SERVERLINK = (256 * 1024);

        static uint socket_max_client_packet;

        static short[] addr_;   // ip addresses of local host (host byte order)
        static int naddr_ = 0;   // # of ip addresses
        static long stall_time = 60;
        static int ip_rules = 1;

        static UInt64 last_tick;

        public socket_data[] session;

        int[] send_shortlist_array;
        int send_shortlist_count = 0;
        int[] send_shortlist_set;

        public class socket_data
        {
            public class c_flag
            {
                public bool eof;
                public bool server;
                char ping;
            }
            public c_flag flag;

            public IPAddress client_addr;

            public ushort rdata, wdata;
            public int max_rdata, max_wdata;
            public int rdata_size, wdata_size;
            public int rdata_pos;
            public UInt64 rdata_tick; // time of last recv (for detecting timeouts); zero when timeout is disabled

            public string func_recv;
            public string func_send;
            public string func_parse;

            static bool init;

            object session_data; // stores application-specific data related to the session

            static socket_data()
            {
                init = true;
            }

            public bool is_init()
            {
                return init;
            }
        }

        //////////////////////////////
        // IP rules and DDoS protection
        struct AccessControl
        {
            static bool init;
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

        public c_socket(timer timer)
        {
            socket_init(timer);
        }

        /// <summary>
        /// socket I/O macros
        /// </summary>
        public void WFIFOHEAD(int fd, int size)
        {
            if (fd > 0 && session[fd].wdata_size + (size) > session[fd].max_wdata)
                realloc_writefifo(fd, size);
            return;
        }
        public int RFIFOP(int fd, int pos) { return session[fd].rdata + session[fd].rdata_size + pos; }
        public int WFIFOP(int fd, int pos) { return session[fd].wdata + session[fd].wdata_size + pos; }
        public ushort RFIFOW(int fd, int pos) { return (ushort)RFIFOP(fd, pos); }
        public ushort WFIFOW(int fd, int pos) { return (ushort)WFIFOP(fd, pos); }

        /// <summary
        /// advance the WFIFO cursor (marking 'len' bytes for sending)
        /// </sumary>
        public void WFIFOSET(int fd, int len)
        {
            int newreserve;
            socket_data s = session[fd];

            if (!session_isValid(fd) || s.wdata <= 0)
                return;

            // we have written len bytes to the buffer already before calling WFIFOSET
            if (s.wdata_size + len > s.max_wdata)
            {   // actually there was a buffer overflow already
                IPAddress ip = s.client_addr;
                console.fatalerror("WFIFOSET: Write Buffer Overflow. Connection "+fd+" ("+ip.ToString()+") has written "+len+" bytes on a "+s.wdata_size+"/"+s.max_wdata+" bytes buffer.");
                console.debug("Likely command that caused it: 0x"+ (s.wdata + s.wdata_size).ToString("x"));
                // no other chance, make a better fifo model
                Environment.Exit(1);
            }

            if (len > 0xFFFF)
            {
                // dynamic packets allow up to UINT16_MAX bytes (<packet_id>.W <packet_len>.W ...)
                // all known fixed-size packets are within this limit, so use the same limit
                console.fatalerror("WFIFOSET: Packet 0x"+(s.wdata + s.wdata_size).ToString("x")+" is too big. (len="+len+", max="+0xFFFF+")");
                Environment.Exit(1);
            }
            else if (len == 0)
            {
                // abuses the fact, that the code that did WFIFOHEAD(fd,0), already wrote
                // the packet type into memory, even if it could have overwritten vital data
                // this can happen when a new packet was added on map-server, but packet len table was not updated
                console.warning("WFIFOSET: Attempted to send zero-length packet, most likely 0x"+ WFIFOW(fd, 0).ToString("x4")+" (please report this).");
                return;
            }

            if (!s.flag.server)
            {
                if (len > socket_max_client_packet)
                {// see declaration of socket_max_client_packet for details
                    console.error("WFIFOSET: Dropped too large client packet 0x" + WFIFOW(fd, 0).ToString("x4") + " (length=" + len + ", max=" + socket_max_client_packet + ").");
                    return;
                }
                if (s.wdata_size + len > WFIFO_MAX)
                {// reached maximum write fifo size
                    console.error("WFIFOSET: Maximum write buffer size for client connection "+fd+ " exceeded, most likely caused by packet 0x" + WFIFOW(fd, 0).ToString("x4") + " (len="+len+", ip="+s.client_addr.ToString()+").");
                    set_eof(fd);
                    return;
                }
            }

            s.wdata_size += len;
            //If the interserver has 200% of its normal size full, flush the data.
            if (s.flag.server && s.wdata_size >= 2 * FIFOSIZE_SERVERLINK)
                flush_fifo(fd);

            return;
        }

        /// <summary>
        /// CORE : Default processing functions
        /// </summary>
        int null_recv(int fd) { return 0; }
        int null_send(int fd) { return 0; }
        int null_parse(int fd) { return 0; }

        static string default_func_parse = "null_parse";
        public void set_defaultparse(string defaultparse)
        {
            default_func_parse = defaultparse;
        }

        /// <summary>
        ///	CORE : Socket Sub Function
        /// </summary>
        void set_eof(int fd)
        {
            if (session_isActive(fd))
            {
                if(SEND_SHORTLIST)
                    // Add this socket to the shortlist for eof handling.
                    send_shortlist_add_fd(fd);

                session[fd].flag.eof = true;
            }
        }

        /// <summary>
        /// Best effort - there's no warranty that the data will be sent.
        /// </summary>
        void flush_fifo(int fd)
        {
            if (session[fd] != null)
            {
                object[] t = new object[1]; 
                t[0] = fd;
                common.CallFunc(GetType(), this, session[fd].func_send, t);
            }
        }

        void create_session(int fd, string func_recv = "", string func_send = "", string func_parse = "")
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
                    console.info("socket.access_ipmask: Loaded IP: " + ip.ToString() + " mask:" + mask.ToString());
                }
            }
            ret.ip = ip;
            ret.mask = mask;

            return ret;
        }

        void realloc_writefifo(int fd, int addition)
        {
            int newsize;

            if (!session_isValid(fd)) // might not happen
                return;

            if (session[fd].wdata_size + addition > session[fd].max_wdata)
            {   // grow rule; grow in multiples of WFIFO_SIZE
                newsize = WFIFO_SIZE;
                while (session[fd].wdata_size + addition > newsize) newsize += WFIFO_SIZE;
            }
            else
            if (session[fd].max_wdata >= 2 * ((session[fd].flag.server) ? FIFOSIZE_SERVERLINK : WFIFO_SIZE)
                && (session[fd].wdata_size + addition) * 4 < session[fd].max_wdata)
            {   // shrink rule, shrink by 2 when only a quarter of the fifo is used, don't shrink below nominal size.
                newsize = session[fd].max_wdata / 2;
            }
            else // no change
                return;

            session[fd].max_wdata = newsize;

            return;
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

        public bool session_isValid(int fd)
        {
            return (fd > 0 && fd < FD_SETSIZE && session[fd].is_init());
        }

        public bool session_isActive(int fd)
        {
            return (session_isValid(fd) && !session[fd].flag.eof);
        }

        public void socket_init(timer timer)
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
            session[0] = new socket_data();

            // Get initial local ips
            addr_ = new short[16];
            naddr_ = getips(ref addr_, 16);

            if (SEND_SHORTLIST)
                send_shortlist_set = new int[(FD_SETSIZE + 31) / 32];

            config_read(SOCKET_CONF_FILENAME);

            // initialise last send-receive tick
            last_tick = timer.time(null);

            // session[0] is now currently used for disconnected sessions of the map server, and as such,
            // should hold enough buffer (it is a vacuum so to speak) as it is never flushed. [Skotlex]
            create_session(0);

            // Delete old connection history every 5 minutes
            connect_history = new ConnectHistory[0x10000];
            timer.add_timer_interval(timer.time(null) + 1000, "connect_check_clear", null , 0, 5 * 60 * 1000);

            console.info("Server supports up to '"+FD_SETSIZE+"' concurrent connections.");
        }

        /// <summary>
        /// Add a fd to the shortlist so that it'll be recognized as a fd that needs sending or eof handling.
        /// </summary>
        void send_shortlist_add_fd(int fd)
        {
            if (SEND_SHORTLIST)
            {
                int i;
                int bit;

                if (!session_isValid(fd))
                    return;// out of range

                i = fd / 32;
                bit = fd % 32;

                if ((send_shortlist_set[i] >> bit) == 1)
                    return;// already in the list

                if (send_shortlist_count >= send_shortlist_array.Length)
                {
                    console.debug("send_shortlist_add_fd: shortlist is full, ignoring... (fd="+fd+" shortlist.count="+send_shortlist_count+" shortlist.length="+send_shortlist_array.Length+")");
                    return;
                }

                // set the bit
                send_shortlist_set[i] |= 1 << bit;
                // Add to the end of the shortlist array.
                send_shortlist_array[send_shortlist_count++] = fd;
            }
        }
    }
}
