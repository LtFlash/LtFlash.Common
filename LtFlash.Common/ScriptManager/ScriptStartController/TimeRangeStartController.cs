using System;
using Rage;

namespace LtFlash.Common.ScriptManager.ScriptStartController
{
    public class TimeRangeStartController : IScriptStartController
    {
        private TimeSpan HourStart { get; set; }
        private TimeSpan HourEnd { get; set; }

        public TimeRangeStartController(TimeSpan hourStart, TimeSpan hourEnd)
        {
            HourStart = hourStart;
            HourEnd = hourEnd;
        }

        public bool CanBeStarted()
        {
            TimeSpan start = HourStart; //TimeSpan.Parse("22:00"); // 10 PM
            TimeSpan end = HourEnd; // TimeSpan.Parse("02:00");   // 2 AM
            TimeSpan now = World.DateTime.TimeOfDay; //DateTime.Now.TimeOfDay;

            if (start <= end)
            {
                // start and stop times are in the same day
                if (now >= start && now <= end)
                {
                    // current time is between start and stop
                    return true;
                }
            }
            else
            {
                // start and stop times are in different days
                if (now >= start || now <= end)
                {
                    // current time is between start and stop
                    return true;
                }
            }
            return false;
        }
    }
}
