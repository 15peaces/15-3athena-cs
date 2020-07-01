// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15peaces 2017
// For more information, see LICENCE in the main folder
using System.Reflection;
using System.Linq;

using showmsg;


namespace n_core
{
    public class core
    {
        /*======================================
        * Display title
        *--------------------------------------*/
        public static void display_title()
        {
            console.message("\n", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=)     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (            15-3athena Development Team presents             )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (         ______  __    __                                    )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (        /\\  _  \\/\\ \\__/\\ \\                                   )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (      __\\ \\ \\_\\ \\ \\ ,_\\ \\ \\___      __    ___      __        )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (    /'__`\\ \\  __ \\ \\ \\/\\ \\  _ `\\  /'__`\\/' _ `\\  /'__`\\      )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (   /\\  __/\\ \\ \\/\\ \\ \\ \\_\\ \\ \\ \\ \\/\\  __//\\ \\/\\ \\/\\ \\_\\.\\_    )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (   \\ \\____\\\\ \\_\\ \\_\\ \\__\\\\ \\_\\ \\_\\ \\____\\ \\_\\ \\_\\ \\__/.\\_\\   )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (    \\/____/ \\/_/\\/_/\\/__/ \\/_/\\/_/\\/____/\\/_/\\/_/\\/__/\\/_/   )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (     _   _   _   _   _   _   _     _   _   _   _   _   _     )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (    / \\ / \\ / \\ / \\ / \\ / \\ / \\   / \\ / \\ / \\ / \\ / \\ / \\    )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (   ( e | n | g | l | i | s | h ) ( A | t | h | e | n | a )   )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (    \\_/ \\_/ \\_/ \\_/ \\_/ \\_/ \\_/   \\_/ \\_/ \\_/ \\_/ \\_/ \\_/    )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (                                                             )     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=)     ", console.e_color.CL_BLUE, console.e_color.CL_CYAN);
            console.message("         (=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=)     ", console.e_color.CL_DARK_READ, console.e_color.CL_CYAN);
            console.message("         (     -enhanced with 3rd class modification (15-3athena)      )     ", console.e_color.CL_DARK_READ, console.e_color.CL_CYAN);
            console.message("         (                  -re-written in modern C#                   )     ", console.e_color.CL_DARK_READ, console.e_color.CL_CYAN);
            console.message("         (=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=)     ", console.e_color.CL_DARK_READ, console.e_color.CL_CYAN);
            console.info("Git Hash: " + GetGitHash());
        }

        // Won't do anything if runned alone...
        static int Main()
        {
            return 0;
        }

        /// <summary> Gets the git hash value from the assembly
        /// or null if it cannot be found. </summary>
        public static string GetGitHash()
        {
            var asm = typeof(core).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            return attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;
        }
    }
}