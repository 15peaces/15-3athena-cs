// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder

namespace n_mmo {
    public class mmo
    {
        // server->client protocol version
        //        0 - pre-?
        //        1 - ?                    - 0x196
        //        2 - ?                    - 0x78, 0x79
        //        3 - ?                    - 0x1c8, 0x1c9, 0x1de
        //        4 - ?                    - 0x1d7, 0x1d8, 0x1d9, 0x1da
        //        5 - 2003-12-18aSakexe+   - 0x1ee, 0x1ef, 0x1f0, ?0x1c4, 0x1c5?
        //        6 - 2004-03-02aSakexe+   - 0x1f4, 0x1f5
        //        7 - 2005-04-11aSakexe+   - 0x229, 0x22a, 0x22b, 0x22c
        // 20061023 - 2006-10-23aSakexe+   - 0x6b, 0x6d
        // 20070521 - 2007-05-21aSakexe+   - 0x283
        // 20070821 - 2007-08-21aSakexe+   - 0x2c5, 0x2c6
        // 20070918 - 2007-09-18aSakexe+   - 0x2d7, 0x2d9, 0x2da
        // 20071106 - 2007-11-06aSakexe+   - 0x78, 0x7c, 0x22c
        // 20080102 - 2008-01-02aSakexe+   - 0x2ec, 0x2ed , 0x2ee
        // 20081126 - 2008-11-26aSakexe+   - 0x1a2
        // 20090408 - 2009-04-08aSakexe+   - 0x44a (dont use as it overlaps with RE client packets)
        // 20080827 - 2008-08-27aRagexeRE+ - First RE Client
        // 20081126 - 2008-11-26aSakexe+   - 0x1a2, 0x441
        // 20081210 - 2008-12-10aSakexe+   - 0x442
        // 20081217 - 2008-12-17aRagexeRE+ - 0x6d (Note: This one still use old Char Info Packet Structure)
        // 20081218 - 2008-12-17bRagexeRE+ - 0x6d (Note: From this one client use new Char Info Packet Structure)
        // 20090603 - 2009-06-03aRagexeRE+ - 0x7d7, 0x7d8, 0x7d9, 0x7da
        // 20090617 - 2009-06-17aRagexeRE+ - 0x7d9
        // 20090715 - 2009-07-15aRagexeRE+ - 0x7e1
        // 20090922 - 2009-09-22aRagexeRE+ - 0x7e5, 0x7e7, 0x7e8, 0x7e9
        // 20091103 - 2009-11-03aRagexeRE+ - 0x7f7, 0x7f8, 0x7f9
        // 20100105 - 2010-01-05aRagexeRE+ - 0x133, 0x800, 0x801
        // 20100126 - 2010-01-26aRagexeRE+ - 0x80e
        // 20100223 - 2010-02-23aRagexeRE+ - 0x80f
        // 20100413 - 2010-04-13aRagexeRE+ - 0x6b
        // 20100629 - 2010-06-29aRagexeRE+ - 0x2d0, 0xaa, 0x2d1, 0x2d2
        // 20100721 - 2010-07-21aRagexeRE+ - 0x6b, 0x6d
        // 20100727 - 2010-07-27aRagexeRE+ - 0x6b, 0x6d
        // 20100803 - 2010-08-03aRagexeRE+ - 0x6b, 0x6d, 0x827, 0x828, 0x829, 0x82a, 0x82b, 0x82c, 0x842, 0x843
        // 20101124 - 2010-11-24aRagexeRE+ - 0x856, 0x857, 0x858
        // 20110111 - 2011-01-11aRagexeRE+ - 0x6b, 0x6d
        // 20110928 - 2011-09-28aRagexeRE+ - 0x6b, 0x6d
        // 20111025 - 2011-10-25aRagexeRE+ - 0x6b, 0x6d
        // 20120307 - 2012-03-07aRagexeRE+ - 0x970

        // public static uint PACKETVER = 20131223; //Stable client [15peaces]
        public static uint PACKETVER = 20141022; //Stable client [15peaces]

        // Set this line to false disable sc_data saving. [Skotlex]
        const bool ENABLE_SC_SAVING = true;
        // Set this line to false to disable server-side hot-key saving support [Skotlex]
        // Note that newer clients no longer save hotkeys in the registry!
        const bool HOTKEY_SAVING = true;

        const ushort MAX_MAP_PER_SERVER = 1500;
        const ushort MAX_INVENTORY = 100;
        // Max number of characters per account. Note that changing this setting alone is not enough if the client is not hexed to support more characters as well.
        const ushort MAX_CHARS = 12;
        // Number of slots carded equipment can have. Never set to less than 4 as they are also used to keep the data of forged items/equipment. [Skotlex]
        // Note: The client seems unable to receive data for more than 4 slots due to all related packets having a fixed size.
        const byte MAX_SLOTS = 4;
        // Max amount of a single stacked item
        const ushort MAX_AMOUNT = 30000;
        const int MAX_ZENY = 1000000000;
        const int MAX_BANK_ZENY = int.MaxValue; // CHECKME: Is it possible to increase this by using uint max? [15peaces]
        const uint MAX_FAME = 1000000000;
        const ushort MAX_CART = 100;
        const ushort MAX_SKILL = 5016;
        const ushort GLOBAL_REG_NUM = 256;
        const ushort ACCOUNT_REG_NUM = 64;
        const ushort ACCOUNT_REG2_NUM = 16;
        //Should hold the max of GLOBAL/ACCOUNT/ACCOUNT2 (needed for some arrays that hold all three)
        const ushort MAX_REG_NUM = 256;
        const ushort DEFAULT_WALK_SPEED = 150;
        const ushort MIN_WALK_SPEED = 0;
        const ushort MAX_WALK_SPEED = 1000;
        const ushort MAX_STORAGE = 600;
        const ushort MAX_GUILD_STORAGE = 600;
        const ushort MAX_PARTY = 12;
        const ushort MAX_GUILD = (16 + 10 * 6);	// increased max guild members +6 per 1 extension levels [Lupus]
        const ushort MAX_GUILDPOSITION = 20;	// increased max guild positions to accomodate for all members [Valaris] (removed) [PoW]
        const ushort MAX_GUILDEXPULSION = 32;
        const ushort MAX_GUILDALLIANCE = 16;
        const ushort MAX_GUILDSKILL = 17; // increased max guild skills because of new skills [15peaces]
        const ushort MAX_GUILDCASTLE = 34;	// Updated to include new entries for WoE:SE. [L0ne_W0lf]
        const ushort MAX_GUILDLEVEL = 50;
        const ushort MAX_GUARDIANS = 8;	// Local max per castle. [Skotlex]
        const ushort MAX_QUEST_DB = 2503; // Max quests that the server will load
        const ushort MAX_QUEST_OBJECTIVES = 3; // Max quest objectives for a quest
        const ushort MAX_PC_BONUS_SCRIPT = 20; // cydh bonus_script
        const ushort MAX_CLAN = 500;
        const ushort MAX_CLANALLIANCE = 6;

        // for produce
        const ushort MIN_ATTRIBUTE = 0;
        const ushort MAX_ATTRIBUTE = 4;
        const ushort ATTRIBUTE_NORMAL = 0;
        const ushort MIN_STAR = 0;
        const ushort MAX_STAR = 3;

        const ushort MAX_STATUS_TYPE = 5;

        const ushort WEDDING_RING_M = 2634;
        const ushort WEDDING_RING_F = 2635;

        // For character names, title names, guilds, maps, etc.
        // Includes null-terminator as it is the length of the array.
        const ushort NAME_LENGTH = (23 + 1);
        // NPC names can be longer than it's displayed on client (NAME_LENGTH).
        const ushort NPC_NAME_LENGTH = 50;
        // For item names, which tend to have much longer names.
        const ushort ITEM_NAME_LENGTH = 50;
        // For Map Names, which the client considers to be 16 in length including the .gat extension
        // FIXME: These are used wrong, they are supposed to be character strings of lengths 12/16,
        //        where only lengths of 11/15 and lower are zero-terminated, not always zero-
        //        terminated character strings of lengths 11+1/15+1! This is why mjolnir_04_1 cannot
        //        be used on eAthena. [Ai4rei]
        const ushort MAP_NAME_LENGTH = (11 + 1);
        const ushort MAP_NAME_LENGTH_EXT = (MAP_NAME_LENGTH + 4);

        const ushort MAX_FRIENDS = 40;
        const ushort MAX_MEMOPOINTS = 3;
        const ushort MAX_SKILLCOOLDOWN = 20;

        // Size of the fame list arrays.
        const ushort MAX_FAME_LIST = 10;

        // Limits to avoid ID collision with other game objects
        const ulong START_ACCOUNT_NUM = 2000000;
        const ulong END_ACCOUNT_NUM = 100000000; // CHECKME: Really needed? [15peaces]
        const ulong START_CHAR_NUM = 150000;

        // Guilds
        const ushort MAX_GUILDMES1 = 60;
        const ushort MAX_GUILDMES2 = 120;

        // Homunculus Skills
        const ushort HM_SKILLBASE = 8001;
        const ushort MAX_HOMUNSKILL = 43;
        const ushort MAX_HOMUNCULUS_CLASS = 16;	//[orn]
        const ushort HM_CLASS_BASE = 6001;
        const ushort HM_CLASS_MAX = (HM_CLASS_BASE + MAX_HOMUNCULUS_CLASS - 1);

        // Mail System
        const ushort MAIL_MAX_INBOX = 30;
        const ushort MAIL_TITLE_LENGTH = 40;
        const ushort MAIL_BODY_LENGTH = 200;

        // Mercenary System
        const ushort MC_SKILLBASE = 8201;
        const ushort MAX_MERCSKILL = 41;
        const ushort MAX_MERCENARY_CLASS = 45;

        // Elemental System
        const ushort EL_SKILLBASE = 8401;
        const ushort MAX_ELEMSKILL = 42;
        const ushort MAX_ELESKILLTREE = 3;
        const ushort MAX_ELEMENTAL_CLASS = 12;
        const ushort EL_CLASS_BASE = 2114;
        const ushort EL_CLASS_MAX = (EL_CLASS_BASE + MAX_ELEMENTAL_CLASS - 1);

        // 15-3athena
        // Will be needed in the future for keeping track of and saving cooldown times for skills. [15peaces]
        // const ushort MAX_SKILLCOOLDOWN = 20;

        enum item_types
        {
            IT_HEALING = 0,         //IT_HEAL		        = 0x00
            IT_UNKNOWN,     //1		//IT_SCHANGE	        = 0x01
            IT_USABLE,      //2		//IT_SPECIAL			= 0x02
            IT_ETC,         //3		//IT_EVENT				= 0x03
            IT_WEAPON,      //4		//IT_ARMOR				= 0x04
            IT_ARMOR,       //5		//IT_WEAPON				= 0x05
            IT_CARD,        //6		//IT_CARD				= 0x06
            IT_PETEGG,      //7		//IT_QUEST				= 0x07
            IT_PETARMOR,    //8		//IT_BOW				= 0x08
            IT_UNKNOWN2,    //9		//IT_BOTHHAND			= 0x09
            IT_AMMO,        //10	//IT_ARROW				= 0x0a
            IT_DELAYCONSUME,//11	//IT_ARMORTM			= 0x0b
                                    //IT_ARMORTB			= 0x0c
                                    //IT_ARMORMB			= 0x0d
                                    //IT_ARMORTMB			= 0x0e
                                    //IT_GUN				= 0x0f
                                    //IT_AMMO				= 0x10
            IT_THROWWEAPON = 17,    //IT_THROWWEAPON		= 0x11
            IT_CASH,                //IT_CASH_POINT_ITEM	= 0x12
            IT_CANNONBALL,          //IT_CANNONBALL			= 0x13
            IT_MAX
        }

        // Questlog states
        enum quest_state
        {
            Q_INACTIVE, // Inactive quest (the user can toggle between active and inactive quests)
            Q_ACTIVE,   // Active quest
            Q_COMPLETE, // Completed quest
        }

        /// Questlog entry
        unsafe struct quest
        {
            ushort quest_id;                            // Quest ID
            ushort time;                                // Expiration time
            fixed ushort count[MAX_QUEST_OBJECTIVES];   // Kill counters of each quest objective
            quest_state state;                          // Current quest state
        }

        unsafe struct item
        {
            uint id;
            ushort nameid;
            ushort amount;
            uint equip; // location(s) where item is equipped (using enum equip_pos for bitmasking)
            char identify;
            char refine;
            char attribute;
            fixed ushort card[MAX_SLOTS];
            uint expire_time;
            bool favorite;
            bool bound;
        }

        //Equip indexes constants. (eg: sd->equip_index[EQI_AMMO] returns the index
        //where the arrows are equipped)
        enum equip_index
        {
            EQI_ACC_L = 0,
            EQI_ACC_R,
            EQI_SHOES,
            EQI_GARMENT,
            EQI_HEAD_LOW,
            EQI_HEAD_MID,
            EQI_HEAD_TOP,
            EQI_ARMOR,
            EQI_HAND_L,
            EQI_HAND_R,
            EQI_AMMO,
            EQI_COSTUME_HEAD_TOP,
            EQI_COSTUME_HEAD_MID,
            EQI_COSTUME_HEAD_LOW,
            EQI_COSTUME_GARMENT,
            EQI_COSTUME_FLOOR,
            EQI_SHADOW_ARMOR,
            EQI_SHADOW_WEAPON,
            EQI_SHADOW_SHIELD,
            EQI_SHADOW_SHOES,
            EQI_SHADOW_ACC_R,
            EQI_SHADOW_ACC_L,
            EQI_MAX
        }

        struct point
        {
            ushort map;
            short x, y;
        }

        enum e_skill_flag
        {
            SKILL_FLAG_PERMANENT,
            SKILL_FLAG_TEMPORARY,
            SKILL_FLAG_PLAGIARIZED,
            SKILL_FLAG_REPLACED_LV_0, // temporary skill overshadowing permanent skill of level 'N - SKILL_FLAG_REPLACED_LV_0'
                                      //...
        }

        //OPTION: (EFFECTSTATE_)
        enum EFFECTSTATE
        {
            OPTION_NOTHING = 0x00000000,
            OPTION_SIGHT = 0x00000001,
            OPTION_HIDE = 0x00000002,
            OPTION_CLOAK = 0x00000004,
            OPTION_CART1 = 0x00000008,
            OPTION_FALCON = 0x00000010,
            OPTION_RIDING = 0x00000020,
            OPTION_INVISIBLE = 0x00000040,
            OPTION_CART2 = 0x00000080,
            OPTION_CART3 = 0x00000100,
            OPTION_CART4 = 0x00000200,
            OPTION_CART5 = 0x00000400,
            OPTION_ORCISH = 0x00000800,
            OPTION_WEDDING = 0x00001000,
            OPTION_RUWACH = 0x00002000,
            OPTION_CHASEWALK = 0x00004000,
            OPTION_FLYING = 0x00008000, //Note that clientside Flying and Xmas are 0x8000 for clients prior to 2007.
            OPTION_XMAS = 0x00010000,
            OPTION_TRANSFORM = 0x00020000,
            OPTION_SUMMER = 0x00040000,
            OPTION_DRAGON1 = 0x00080000,
            OPTION_WUG = 0x00100000,
            OPTION_WUGRIDER = 0x00200000,
            OPTION_MADOGEAR = 0x00400000,
            OPTION_DRAGON2 = 0x00800000,
            OPTION_DRAGON3 = 0x01000000,
            OPTION_DRAGON4 = 0x02000000,
            OPTION_DRAGON5 = 0x04000000,
            OPTION_HANBOK = 0x08000000,
            OPTION_OKTOBERFEST = 0x10000000,
            // compound constants
            OPTION_CART = OPTION_CART1 | OPTION_CART2 | OPTION_CART3 | OPTION_CART4 | OPTION_CART5,
            OPTION_DRAGON = OPTION_DRAGON1 | OPTION_DRAGON2 | OPTION_DRAGON3 | OPTION_DRAGON4 | OPTION_DRAGON5,
            OPTION_MASK = ~OPTION_INVISIBLE,
        }

        struct s_skill
        {
            ushort id;
            char lv;
            e_skill_flag flag;
        }

        public struct global_reg
        {
            string str;
            string value;
        };

        //Holds array of global registries, used by the char server and converter.
        struct accreg
        {
            ulong account_id, char_id;
            int reg_num;
            global_reg[] reg;
        }

        //For saving status changes across sessions. [Skotlex]
        struct status_change_data
        {
            ushort type; //SC_type
            long val1, val2, val3, val4, tick; //Remaining duration.
        }

        struct skill_cooldown_data
        {
            ushort skill_id;
            long tick;
        }

        const ushort MAX_BONUS_SCRIPT_LENGTH = 512;
        unsafe struct bonus_script_data
        {
            string script_str; // Script string
            ulong tick; // Tick
            short flag; // Flags @see enum e_bonus_script_flags
            ushort icon; // Icon SI
            byte type; // 0 - None, 1 - Buff, 2 - Debuff
        };

        unsafe struct storage_data
        {
            int storage_amount;
            item[] items;
        }

        struct guild_storage
        {
            int dirty;
            ulong guild_id;
            short storage_status;
            short storage_amount;
            item[] items;
	        ushort _lock;
        }

        struct s_pet
        {
            ulong account_id;
            ulong char_id;
            ulong pet_id;
            ushort class_;
            ushort level;
            short egg_id;//pet egg id
            short equip;//pet equip name_id
            ushort intimate;//pet friendly
            ushort hungry;//pet hungry
            string name;
            char rename_flag;
            char incuvate;
        }

        struct s_homunculus // [orn]
        { 
            string name;
            ulong hom_id;
            ulong char_id;
            ushort class_;
            int hp, max_hp, sp, max_sp;
            ushort intimacy;  //[orn]
            ushort hunger;
            s_skill[] hskill; //albator
	        short skillpts;
            ushort level;
            uint exp;
            short rename_flag;
            short vaporize; //albator
            int str;
            int agi;
            int vit;
            int int_;
            int dex;
            int luk;
        };

        struct s_mercenary
        {
            ulong mercenary_id;
            ulong char_id;
            ushort class_;
            int hp, sp;
            uint kill_count;
            int life_time;
        };

        struct s_elemental
        {
            ulong elemental_id;
            ulong char_id;
            ushort class_;
            int mode;
            int hp, sp, max_hp, max_sp, str, agi, vit, int_, dex, luk;
            int life_time;
        };

        struct s_friend
        {
            ulong account_id;
            ulong char_id;
            string name;
        };

#if HOTKEY_SAVING
        struct hotkey
        {
            ulong id;
            ushort lv;
            char type; // 0: item, 1: skill
        };
#endif

        struct mmo_charstatus
        {
            ulong char_id;
            ulong account_id;
            ulong partner_id;
            ulong father;
            ulong mother;
            ulong child;

            uint base_exp, job_exp;
            int zeny;

            ushort class_;
            uint status_point, skill_point;
            int hp, max_hp, sp, max_sp;
            uint option;
            short manner;
            char karma;
            short hair, hair_color, clothes_color, body;
            long party_id, guild_id, clan_id, pet_id, hom_id, mer_id, ele_id;
            int fame;

            // Mercenary Guilds Rank
            int arch_faith, arch_calls;
            int spear_faith, spear_calls;
            int sword_faith, sword_calls;

            short weapon; // enum weapon_type
            short shield; // view-id
            short head_top, head_mid, head_bottom;
            short robe;

            string name;
            uint base_level, job_level;
            short str, agi, vit, int_, dex, luk;
            char slot, sex;

            uint mapip;
            ushort mapport;

            point last_point, save_point;
            point[] memo_point;
	        item[] inventory, cart;
	        storage_data storage;
	        s_skill[] skill;

	        s_friend[] friends; //New friend system [Skotlex]
#if HOTKEY_SAVING
	        hotkey[] hotkeys;
#endif
	        bool show_equip;
            short rename;
            ushort slotchange;

            long delete_date;

            char hotkey_rowshift;
        }

        enum mail_status
        {
            MAIL_NEW,
            MAIL_UNREAD,
            MAIL_READ,
        }

        struct mail_message
        {
            int id;
            ulong send_id;
            string send_name;
            ulong dest_id;
            string dest_name;
            string title;
            string body;

            mail_status status;
            long timestamp; // marks when the message was sent

            int zeny;
            item item;
        }

        struct mail_data
        {
            short amount;
            bool full;
            short _unchecked, unread;
	        mail_message[] msg;
        }

        struct auction_data
        {
            uint auction_id;
            ulong seller_id;
            string seller_name;
            ulong buyer_id;
            string buyer_name;

            item item;
	        // This data is required for searching, as itemdb is not read by char server
	        string item_name;
            short type;

            ushort hours;
            int price, buynow;
            long timestamp; // auction's end time
            int auction_end_timer;
        }

        struct registry
        {
            int global_num, account_num, account2_num;
            global_reg[] global, account, account2;
        }

        struct party_member
        {
            ulong account_id;
            ulong char_id;
            string name;
            ushort class_;
            ushort map;
            ushort lv;
            ushort leader;
            ushort online;
        }

        struct party
        {
            int party_id;
            string name;
            char count; //Count of online characters.
            char exp;
            char item; //&1: Party-Share (round-robin), &2: pickup style: shared.
            party_member[] member;
        }
        struct map_session_data { }
        unsafe struct guild_member
        {
            ulong account_id, char_id;
            short hair, hair_color, gender, class_, lv;
            uint exp;
            int exp_payper;
            short online, position;
            string name;
            map_session_data *sd;
	        char modified;
        }

        struct guild_position
        {
            string name;
            int mode;
            int exp_mode;
            char modified;
        }

        struct guild_alliance
        {
            int opposition;
            int guild_id;
            string name;
        }

        struct guild_expulsion
        {
            string name;
            string mes;
            ulong account_id;
        }

        struct guild_skill
        {
            ushort id, lv;
        };

        struct guild
        {
            int guild_id;
            short guild_lv, connect_member, max_member, average_lv;
            uint exp;
            uint next_exp;
            int skill_point;
            string name, master;
            guild_member[] member;
	        guild_position[] position;
	        string mes1, mes2;
            int emblem_len, emblem_id;
            string emblem_data;
            guild_alliance[] alliance;
	        guild_expulsion[] expulsion;
	        guild_skill[] skill;

	        ushort save_flag; // for TXT saving
        }

        unsafe struct guild_castle
        {
            int castle_id;
            int mapindex;
            string castle_name;
            string castle_event;
            int guild_id;
            int economy;
            int defense;
            int triggerE;
            int triggerD;
            int nextTime;
            int payTime;
            int createTime;
            int visibleC;
            struct s_guardian
            {
                ushort visible;
                int id; // object id
            }
            s_guardian[] guardian;
	        int* temp_guardians; // ids of temporary guardians (mobs)
            int temp_guardians_max;
        }

        struct fame_list
        {
            ulong id;
            int fame;
            string name;
        }

        // Change Guild Infos
        enum e_guild_info
        { 
            GBI_EXP = 1,        // Guild Experience (EXP)
            GBI_GUILDLV,        // Guild level
            GBI_SKILLPOINT,     // Guild skillpoints
            GBI_SKILLLV,        // Guild skill_lv ?? seem unused
        };

        // Change Member Infos
        enum e_guild_member_info
        { 
            GMI_POSITION = 0,
            GMI_EXP,
            GMI_HAIR,
            GMI_HAIR_COLOR,
            GMI_GENDER,
            GMI_CLASS,
            GMI_LEVEL,
        };

        enum e_guild_skill
        {
            GD_SKILLBASE = 10000,
            GD_APPROVAL = 10000,
            GD_KAFRACONTRACT = 10001,
            GD_GUARDRESEARCH = 10002,
            GD_GUARDUP = 10003,
            GD_EXTENSION = 10004,
            GD_GLORYGUILD = 10005,
            GD_LEADERSHIP = 10006,
            GD_GLORYWOUNDS = 10007,
            GD_SOULCOLD = 10008,
            GD_HAWKEYES = 10009,
            GD_BATTLEORDER = 10010,
            GD_REGENERATION = 10011,
            GD_RESTORE = 10012,
            GD_EMERGENCYCALL = 10013,
            GD_DEVELOPMENT = 10014,
            GD_ITEMEMERGENCYCALL = 10015,
            GD_GUILD_STORAGE = 10016,
            GD_MAX,
        };


        // These mark the ID of the jobs, as expected by the client. [Skotlex]
        enum e_job
        {
            JOB_NOVICE = 0,
            JOB_SWORDMAN,
            JOB_MAGE,
            JOB_ARCHER,
            JOB_ACOLYTE,
            JOB_MERCHANT,
            JOB_THIEF,
            JOB_KNIGHT,
            JOB_PRIEST,
            JOB_WIZARD,
            JOB_BLACKSMITH,
            JOB_HUNTER,
            JOB_ASSASSIN,
            JOB_KNIGHT2,
            JOB_CRUSADER,
            JOB_MONK,
            JOB_SAGE,
            JOB_ROGUE,
            JOB_ALCHEMIST,
            JOB_BARD,
            JOB_DANCER,
            JOB_CRUSADER2,
            JOB_WEDDING,
            JOB_SUPER_NOVICE,
            JOB_GUNSLINGER,
            JOB_NINJA,
            JOB_XMAS,
            JOB_SUMMER,
            JOB_HANBOK,
            JOB_OKTOBERFEST,
            JOB_MAX_BASIC,

            JOB_NOVICE_HIGH = 4001,
            JOB_SWORDMAN_HIGH,
            JOB_MAGE_HIGH,
            JOB_ARCHER_HIGH,
            JOB_ACOLYTE_HIGH,
            JOB_MERCHANT_HIGH,
            JOB_THIEF_HIGH,
            JOB_LORD_KNIGHT,
            JOB_HIGH_PRIEST,
            JOB_HIGH_WIZARD,
            JOB_WHITESMITH,
            JOB_SNIPER,
            JOB_ASSASSIN_CROSS,
            JOB_LORD_KNIGHT2,
            JOB_PALADIN,
            JOB_CHAMPION,
            JOB_PROFESSOR,
            JOB_STALKER,
            JOB_CREATOR,
            JOB_CLOWN,
            JOB_GYPSY,
            JOB_PALADIN2,

            JOB_BABY,
            JOB_BABY_SWORDMAN,
            JOB_BABY_MAGE,
            JOB_BABY_ARCHER,
            JOB_BABY_ACOLYTE,
            JOB_BABY_MERCHANT,
            JOB_BABY_THIEF,
            JOB_BABY_KNIGHT,
            JOB_BABY_PRIEST,
            JOB_BABY_WIZARD,
            JOB_BABY_BLACKSMITH,
            JOB_BABY_HUNTER,
            JOB_BABY_ASSASSIN,
            JOB_BABY_KNIGHT2,
            JOB_BABY_CRUSADER,
            JOB_BABY_MONK,
            JOB_BABY_SAGE,
            JOB_BABY_ROGUE,
            JOB_BABY_ALCHEMIST,
            JOB_BABY_BARD,
            JOB_BABY_DANCER,
            JOB_BABY_CRUSADER2,
            JOB_SUPER_BABY,

            JOB_TAEKWON,
            JOB_STAR_GLADIATOR,
            JOB_STAR_GLADIATOR2,
            JOB_SOUL_LINKER,

            JOB_GANGSI,
            JOB_DEATH_KNIGHT,
            JOB_DARK_COLLECTOR,

            JOB_RUNE_KNIGHT = 4054,
            JOB_WARLOCK,
            JOB_RANGER,
            JOB_ARCH_BISHOP,
            JOB_MECHANIC,
            JOB_GUILLOTINE_CROSS,

            JOB_RUNE_KNIGHT_T,
            JOB_WARLOCK_T,
            JOB_RANGER_T,
            JOB_ARCH_BISHOP_T,
            JOB_MECHANIC_T,
            JOB_GUILLOTINE_CROSS_T,

            JOB_ROYAL_GUARD,
            JOB_SORCERER,
            JOB_MINSTREL,
            JOB_WANDERER,
            JOB_SURA,
            JOB_GENETIC,
            JOB_SHADOW_CHASER,

            JOB_ROYAL_GUARD_T,
            JOB_SORCERER_T,
            JOB_MINSTREL_T,
            JOB_WANDERER_T,
            JOB_SURA_T,
            JOB_GENETIC_T,
            JOB_SHADOW_CHASER_T,

            JOB_RUNE_KNIGHT2,
            JOB_RUNE_KNIGHT_T2,
            JOB_ROYAL_GUARD2,
            JOB_ROYAL_GUARD_T2,
            JOB_RANGER2,
            JOB_RANGER_T2,
            JOB_MECHANIC2,
            JOB_MECHANIC_T2,
            JOB_RUNE_KNIGHT3,
            JOB_RUNE_KNIGHT_T3,
            JOB_RUNE_KNIGHT4,
            JOB_RUNE_KNIGHT_T4,
            JOB_RUNE_KNIGHT5,
            JOB_RUNE_KNIGHT_T5,
            JOB_RUNE_KNIGHT6,
            JOB_RUNE_KNIGHT_T6,

            JOB_BABY_RUNE,
            JOB_BABY_WARLOCK,
            JOB_BABY_RANGER,
            JOB_BABY_BISHOP,
            JOB_BABY_MECHANIC,
            JOB_BABY_CROSS,
            JOB_BABY_GUARD,
            JOB_BABY_SORCERER,
            JOB_BABY_MINSTREL,
            JOB_BABY_WANDERER,
            JOB_BABY_SURA,
            JOB_BABY_GENETIC,
            JOB_BABY_CHASER,

            JOB_BABY_RUNE2,
            JOB_BABY_GUARD2,
            JOB_BABY_RANGER2,
            JOB_BABY_MECHANIC2,

            JOB_SUPER_NOVICE_E = 4190,
            JOB_SUPER_BABY_E,

            JOB_KAGEROU = 4211,
            JOB_OBORO,

            JOB_REBELLION = 4215,

            JOB_MAX,
        }

        enum e_sex
        {
            SEX_FEMALE = 0,
            SEX_MALE,
            SEX_SERVER,
            SEX_ACCOUNT = 99
        }

        enum e_party_member_withdraw
        {
            PARTY_MEMBER_WITHDRAW_LEAVE,      // /leave
            PARTY_MEMBER_WITHDRAW_EXPEL,      // Kicked
            PARTY_MEMBER_WITHDRAW_CANT_LEAVE, // TODO: Cannot /leave
            PARTY_MEMBER_WITHDRAW_CANT_EXPEL, // TODO: Cannot be kicked
        }

        struct clan_alliance
        {
            int opposition;
            int clan_id;
            string name;
        }

        unsafe struct clan
        {
            ushort id;
            string name;
            string master;
            string map;
            short max_member, connect_member;
            map_session_data*[] members;
	        clan_alliance[] alliance;
        }

        // Constructor
        public mmo()
        {
            // PACKETVER 8 and 9 checks
            if (PACKETVER == 8)
                PACKETVER = 20070521;
            else if (PACKETVER == 9)
                PACKETVER = 20071106;

            // sanity checks...
            if (MAX_ZENY > int.MaxValue || MAX_BANK_ZENY > int.MaxValue)
                throw new System.Exception("MAX_ZENY or MAX_BANK_ZENY in mmo.cs is to big!");

            if (PACKETVER < 20110817)
            {
#if PACKET_OBFUSCATION
#undef PACKET_OBFUSCATION
#endif
            }
        }
    }
}