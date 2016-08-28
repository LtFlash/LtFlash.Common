using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Rage;
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
		private Stopwatch sw;

		public BenchmarkData(string id)
		{
		}

		private TimeSpan GetExecTime()
		{
			if(times.Count == 0) return new TimeSpan();
			if(times.Count == 1) return times[0];

			return TimeSpan.FromMilliseconds(times.Average(l => l.Milliseconds));
		}
	}


    //TODO: 
    // - delegate? GetTime + enum to determine the output string
    //   of elapsed time
    // - logging avg time of exec for given id
    public class PerformanceLogger
    {
        private string TimeStamp
            => $"[{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} | {DateTime.Now.TimeOfDay}]";

        private int FPS => (int)Game.FrameRate;

        private Dictionary<string, Stopwatch> performance 
            = new Dictionary<string, Stopwatch>();

        private string path;
        private List<string> linesBuffer = new List<string>();
        private const int BUFFER_LENGHT = 300;

        //TODO: use in new Benchmark class
        private TimeSpan GetAvgTime(List<TimeSpan> list)
            => TimeSpan.FromMilliseconds(list.Average(l => l.Milliseconds));

        public PerformanceLogger(string pathToSaveTo)
        {
            path = pathToSaveTo;
        }

        public void LogPerformanceStart(string id)
        {
            if (performance.ContainsKey(id))
            {
                performance[id].Restart();
            }
            else
            {
                var sw = Stopwatch.StartNew();
                performance.Add(id, sw);
            }
        }

        public void LogPerformanceStop(string id, string description)
        {
            if (!performance.ContainsKey(id)) return;

            performance[id].Stop();
            LogPerformance(description, performance[id].Elapsed.ToString());
        }

        private void LogPerformance(string descr, string timeElapsed)
        {
            string text = $"{TimeStamp} FPS: {FPS} | {descr} | {timeElapsed} |";

            linesBuffer.Add(text);

            if (linesBuffer.Count < BUFFER_LENGHT) return;

            linesBuffer.Add("----------SAVE TO FILE-----------");

            SaveToFile(path, linesBuffer);

            linesBuffer.Clear();
        }

        private void SaveToFile(string path, List<string> lines)
        {
            using (StreamWriter w = File.AppendText(path))
            {
                for (int i = 0; i < lines.Count; ++i)
                {
                    w.WriteLine(lines[i]);
                }
            }
        }
    }
}
