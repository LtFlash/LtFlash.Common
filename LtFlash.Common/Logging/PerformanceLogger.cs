using System;
using System.IO;
using System.Collections.Generic;
using Rage;

namespace LtFlash.Common.Logging
{
    //TODO: 
    // - delegate? GetTime + enum to determine the output string
    //   of elapsed time
    // - logging avg time of exec for given id
    public class PerformanceLogger
    {
        private string TimeStamp
            => $"[{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} | {DateTime.Now.TimeOfDay}]";

        private int FPS => (int)Game.FrameRate;

        private Dictionary<string, BenchmarkData> performance 
            = new Dictionary<string, BenchmarkData>();

        private string path;
        private List<string> linesBuffer = new List<string>();
        private int BUFFER_LENGHT = 100;

        public PerformanceLogger(string pathToSaveTo, int bufLen)
        {
            path = pathToSaveTo;
            BUFFER_LENGHT = bufLen;
        }

        public void LogPerformanceStart(string id)
        {
            if (!performance.ContainsKey(id))
                performance.Add(id, new BenchmarkData());

            performance[id].StartTimer();
        }

        public void LogPerformanceStop(string id, string description, int attempts)
        {
            performance[id].StopTimer();

            if(performance[id].Attempts == attempts)
            {
                LogPerformance(description, performance[id].Time.ToString(), performance[id].Attempts);
                performance[id] = new BenchmarkData();
            }
        }

        private void LogPerformance(string descr, string timeElapsed, int attempts)
        {
            string text = $"{TimeStamp} FPS: {FPS} | {descr} | x{attempts} | {timeElapsed} |";

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
