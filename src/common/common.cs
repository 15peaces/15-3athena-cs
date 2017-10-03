// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15-3athena Dev Team 2017
// For more information, see LICENCE in the main folder
using System;

namespace n_common
{
    public class common
    {
        //--------------------------------------------------
        // Return numerical value of a switch configuration
        // on/off, english, français, deutsch, español
        //--------------------------------------------------
        public static short config_switch(string str)
        {
            str = str.ToLower();

            if (str.Contains("on") || str.Contains("yes") || str.Contains("oui") || str.Contains("ja") || str.Contains("si"))
		        return 1;
	        if (str.Contains("off") || str.Contains("no") || str.Contains("non") || str.Contains("nein"))
		        return 0;

	        return Convert.ToInt16(str);
        }
    }
}