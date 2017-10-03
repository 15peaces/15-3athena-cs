// Copyright (c) Athena Dev Teams - Licensed under GNU GPL
// C# - Remake Copyright © 15-3athena Dev Team 2017
// For more information, see LICENCE in the main folder
using System;
using showmsg;
using System.Timers;
using System.Collections.Generic;
using System.Reflection;

namespace n_timer
{
    public class timer
    {
        const int INVALID_TIMER = -1;

        static List<TimerList> timers;

        struct TimerList
        {
            public int id;
            public Timer timer;
            public string function;
            public string[] args;
        }

        // server startup time
        static DateTime start_time;

        static timer()
        {
            start_time = DateTime.Now;
            timers = new List<TimerList>();
        }

        public static UInt64 DIFF_TICK(UInt64 a, UInt64 b)
        {
            return a - b;
        }

        public static UInt64 time(object time)
        {
            if(time == null)
            {
                return (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }else
            {
                // Todo.
                return 0;
            }
        }

        /// Starts a new timer that automatically restarts itself (infinite loop until manually removed).
        /// Returns the timer's id, or INVALID_TIMER if it fails.
        public int add_timer_interval(UInt64 tick, string func, string[] args, int id, int interval)
        {
            Timer t;

            if (interval < 1)
            {
                console.error("add_timer_interval: invalid interval (tick="+tick+" "+MethodBase.GetCurrentMethod().Name+" "+id+" diff_tick="+DIFF_TICK(tick, time(null))+")");
                return INVALID_TIMER;
            }

            // Create a timer with a ten second interval.
            t = new Timer(interval);

            timers.Add(new TimerList() { id = timers.Count + 1, timer = t, function = func, args = args});

            // Hook up the Elapsed event for the timer.
            t.Elapsed += (sender, e) => TimerElapsed(sender, e, func, args);
            t.Enabled = true;

            GC.KeepAlive(t);

            return timers.Count + 1;
        }

        /// <summary>
        /// Called when a timer elapsed. 
        /// This function calls a function after the timer is elapsed.
        /// </summary>
        private void TimerElapsed(object source, ElapsedEventArgs e, string func, string[] args)
        {
            Type thisType = GetType();
            MethodInfo theMethod = thisType.GetMethod(func);
            theMethod.Invoke(this, args);
        }
    }
}