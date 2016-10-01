using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LtFlash.Common.Logging
{
    public class BenchmarkData
    {
        //PUBLIC	
        public string Id { get; private set; }
        public TimeSpan Time => GetExecTime();
        public int Attempts => times.Count;
        //PRIVATE
        private List<TimeSpan> times = new List<TimeSpan>();
        private Stopwatch sw = new Stopwatch();

        public BenchmarkData()
        {
        }

        public BenchmarkData(string id)
        {
            Id = id;
        }

        public void AddExecTime(TimeSpan ts) => times.Add(ts);

        public void StartTimer() => sw.Restart();

        public void StopTimer()
        {
            sw.Stop();
            //TODO: test w/o creating a new obj
            AddExecTime(new TimeSpan(sw.Elapsed.Ticks));
        }

        private TimeSpan GetExecTime()
        {
            if (times.Count == 0) return new TimeSpan();
            else
            {
                double av = times.Average(t => t.Ticks);
                long avt = Convert.ToInt64(av);
                return new TimeSpan(avt);
            }
        }
    }
}
