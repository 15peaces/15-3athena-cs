/*****************************************************************************\
 *  Copyright (c) Athena Dev Teams - Licensed under GNU GPL                  *
 *  For more information, see LICENCE in the main folder                     *
 *  C# - Remake Copyright © 15peaces 2017                                    *
 *                                                                           *
 *  This file is separated in two sections:                                  *
 *  (1) public typedefs, enums, unions, structures and defines               *
 *  (2) public functions                                                     *
 *                                                                           *
 *  <B>Notes on the release system:</B>                                      *
 *  Whenever an entry is removed from the database both the key and the      *
 *  data are requested to be released.                                       *
 *  At least one entry is removed when replacing an entry, removing an       *
 *  entry, clearing the database or destroying the database.                 *
 *  What is actually released is defined by the release function, the        *
 *  functions of the database only ask for the key and/or data to be         *
 *  released.                                                                *
 *                                                                           *
 *  TODO:                                                                    *
 *  - create an enum for the data (with int, unsigned int and void *)        *
 *  - create a custom database allocator                                     *
 *  - see what functions need or should be added to the database interface   *
 *                                                                           *
 *  HISTORY:                                                                 *
 *    2007/11/09 - Added an iterator to the database.
 *    2.1 (Athena build #???#) - Portability fix                             *
 *      - Fixed the portability of casting to union and added the functions  *
 *        {@link DBMap#ensure(DBMap,DBKey,DBCreateData,...)} and             *
 *        {@link DBMap#clear(DBMap,DBApply,...)}.                            *
 *    2.0 (Athena build 4859) - Transition version                           *
 *      - Almost everything recoded with a strategy similar to objects,      *
 *        database structure is maintained.                                  *
 *    1.0 (up to Athena build 4706)                                          *
 *      - Previous database system.                                          *
 *                                                                           *
 * @version 2.1 (Athena build #???#) - Portability fix                       *
 * @author (Athena build 4859) Flavio @ Amazon Project                       *
 * @author (up to Athena build 4706) Athena Dev Teams                        *
 * @encoding US-ASCII                                                        *
 * @see common#db.c                                                          *
\*****************************************************************************/
using System.Runtime.InteropServices;

namespace n_db
{
    public class db
    {

        // Database creation and destruction macros
        /*****************************************************************************\
         *  (1) Section with public typedefs, enums, unions, structures and defines. *
         *  DBRelease    - Enumeration of release options.                           *
         *  DBType       - Enumeration of database types.                            *
         *  DBOptions    - Bitfield enumeration of database options.                 *
         *  DBKey        - Union of used key types.                                  *
         *  DBApply      - Format of functions applyed to the databases.             *
         *  DBMatcher    - Format of matchers used in DBMap::getall.                 *
         *  DBComparator - Format of the comparators used by the databases.          *
         *  DBHasher     - Format of the hashers used by the databases.              *
         *  DBReleaser   - Format of the releasers used by the databases.            *
         *  DBIterator   - Database iterator.                                        *
         *  DBMap        - Database interface.                                       *
        \*****************************************************************************/

        /**
        * Bitfield with what should be released by the releaser function (if the function supports it).
        * @public
        * @see #DBReleaser
        * @see #db_custom_release(DBRelease)
        */
        enum DBRelease
        {
            DB_RELEASE_NOTHING = 0,
            DB_RELEASE_KEY = 1,
            DB_RELEASE_DATA = 2,
            DB_RELEASE_BOTH = 3
        }

        /**
         * Supported types of database.
         * See {@link #db_fix_options(DBType,DBOptions)} for restrictions of the types of databases.
         * @param DB_INT Uses int's for keys
         * @param DB_UINT Uses unsigned int's for keys
         * @param DB_STRING Uses strings for keys.
         * @param DB_ISTRING Uses case insensitive strings for keys.
         * @public
         * @see #DBOptions
         * @see #DBKey
         * @see #db_fix_options(DBType,DBOptions)
         * @see #db_default_cmp(DBType)
         * @see #db_default_hash(DBType)
         * @see #db_default_release(DBType,DBOptions)
         * @see #db_alloc(const char *,int,DBType,DBOptions,unsigned short)
         */
        enum DBType
        {
            DB_INT,
            DB_UINT,
            DB_STRING,
            DB_ISTRING
        }

        /**
         * Bitfield of options that define the behaviour of the database.
         * See {@link #db_fix_options(DBType,DBOptions)} for restrictions of the types of databases.
         * @param DB_OPT_BASE Base options: does not duplicate keys, releases nothing and does not allow NULL keys or NULL data.
         * @param DB_OPT_DUP_KEY Duplicates the keys internally. If DB_OPT_RELEASE_KEY is defined, the real key is freed as soon as the entry is added.
         * @param DB_OPT_RELEASE_KEY Releases the key.
         * @param DB_OPT_RELEASE_DATA Releases the data whenever an entry is removed from the database.
         *          WARNING: for funtions that return the data (like DBMap::remove), a dangling pointer will be returned.
         * @param DB_OPT_RELEASE_BOTH Releases both key and data.
         * @param DB_OPT_ALLOW_NULL_KEY Allow NULL keys in the database.
         * @param DB_OPT_ALLOW_NULL_DATA Allow NULL data in the database.
         * @public
         * @see #db_fix_options(DBType,DBOptions)
         * @see #db_default_release(DBType,DBOptions)
         * @see #db_alloc(const char *,int,DBType,DBOptions,unsigned short)
         */
        public enum DBOptions
        {
            DB_OPT_BASE = 0,
            DB_OPT_DUP_KEY = 1,
            DB_OPT_RELEASE_KEY = 2,
            DB_OPT_RELEASE_DATA = 4,
            DB_OPT_RELEASE_BOTH = 6,
            DB_OPT_ALLOW_NULL_KEY = 8,
            DB_OPT_ALLOW_NULL_DATA = 16,
        }

        /**
         * Union of key types used by the database.
         * @param i Type of key for DB_INT databases
         * @param ui Type of key for DB_UINT databases
         * @param str Type of key for DB_STRING and DB_ISTRING databases
         * @public
         * @see #DBType
         * @see DBMap#get
         * @see DBMap#put
         * @see DBMap#remove
         */
        [StructLayout(LayoutKind.Explicit)]
        struct DBKey
        {
            [FieldOffset(0)]
            int i;
            [FieldOffset(0)]
            uint ui;
            [FieldOffset(0)]
            string str;
        }

        /**
         * Public interface of a database. Only contains funtions.
         * All the functions take the interface as the first argument.
         * @public
         * @see #db_alloc(const char*,int,DBType,DBOptions,unsigned short)
         */
        public struct DBMap
        {

        }

        public class db_alloc
        {
            public static unsafe DBMap* idb(DBOptions options)
            {
                return _db_alloc(__FILE__(), __LINE__(), DBType.DB_INT, options, int.MaxValue);
            }

            private static unsafe DBMap* _db_alloc(string file, int line, DBType type, DBOptions options, int maxlen)
            {
                return null;
            }

            private static int __LINE__([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
            {
                return lineNumber;
            }
            private static string __FILE__([System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
            {
                return fileName;
            }
        }
    }
}