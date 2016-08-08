using System;
using Rage;

namespace LtFlash.Common.ScriptManager.ScriptStartController
{
    public class TimeRangeStartController : IScriptStartController
    {
        private TimeSpan HourStart { get; set; }
        private TimeSpan HourEnd { get; set; }
        //TimeSpan.Parse("22:00"); // 10 PM
        public TimeRangeStartController(TimeSpan hourStart, TimeSpan hourEnd)
        {
            HourStart = hourStart;
            HourEnd = hourEnd;
        }

        public bool CanBeStarted()
        {
            TimeSpan now = World.DateTime.TimeOfDay; //DateTime.Now.TimeOfDay;

            if (HourStart <= HourEnd)
            {
                // HourStart and stop times are in the same day
                return now >= HourStart && now <= HourEnd;
            }
            else
            {
                // HourStart and stop times are in different days
                return now >= HourStart || now <= HourEnd;
            }
        }
    }
}
