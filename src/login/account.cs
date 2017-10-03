// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder
using n_mmo;
using n_sql;

namespace n_account
{
    struct mmo_account
    {
        ulong account_id;
        string userid;
        string pass;        // 23+1 for plaintext, 32+1 for md5-ed passwords
        char sex;           // gender (M/F/S)
        string email;         // e-mail (by default: a@a.com)
        short level;              // GM level
        uint state;     // packet 0x006a value + 1 (0: compte OK)
        long unban_time;      // (timestamp): ban time limit of the account (0 = no ban)
        long expiration_time; // (timestamp): validity limit of the account (0 = unlimited)
        uint logincount;// number of successful auth attempts
        string lastlogin;     // date+time of last successful login
        string last_ip;       // save of last IP of connection
        string birthdate;   // assigned birth date (format: YYYY-MM-DD, default: 0000-00-00)
        int account_reg2_num;
        mmo.global_reg[] account_reg2; // account script variables (stored on login server)
    };

    unsafe struct AccountDB
    {
        static Sql* accounts;       // SQL accounts storage

        // global sql settings
        static string global_db_hostname;
        static ushort global_db_port;
        static string global_db_username;
        static string global_db_password;
        static string global_db_database;
        static string global_codepage;
        // local sql settings
        static string db_hostname;
        static ushort db_port;
        static string db_username;
        static string db_password;
        static string db_database;
        static string codepage;
        // other settings
        static bool case_sensitive;
        static string account_db;
        static string accreg_db;

        /// public constructor
        static AccountDB()
        {
            // initialize to default values
            accounts = null;
            // global sql settings
            global_db_hostname = "127.0.0.1";
            global_db_port = 3306;
            global_db_username = global_db_password = global_db_database = "ragnarok";
            // local sql settings
            db_port = 3306;
            // other settings
            case_sensitive = false;
            account_db = "login";
            accreg_db = "global_reg_value";

            return;
        }
    }
}