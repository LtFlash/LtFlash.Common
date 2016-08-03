using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Rage;

namespace LtFlash.Common.Logging
{
    public class PerformanceLogger
    {
        private Dictionary<string, Stopwatch> _performance 
            = new Dictionary<string, Stopwatch>();

        private string TimeStamp
        {
            get
            {
                return $"[{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} | {DateTime.Now.TimeOfDay}]";
            }
        }

        private int FPS
        {
            get
            {
                return (int)Game.FrameRate;
            }
        }

        private string _path;

        private List<string> _linesBuffer = new List<string>();
        private const int BUFFER_LENGHT = 300;

        public PerformanceLogger(string pathToSaveTo)
        {
            _path = pathToSaveTo;
        }

        public void LogPerformanceStart(string id)
        {
            if (_performance.ContainsKey(id))
            {
                _performance[id].Restart();
            }
            else
            {
                var sw = Stopwatch.StartNew();
                _performance.Add(id, sw);
            }
        }

        public void LogPerformanceStop(string id, string description)
        {
            if (!_performance.ContainsKey(id)) return;

            _performance[id].Stop();
            LogPerformance(description, _performance[id].Elapsed.ToString());
        }

        private void LogPerformance(string descr, string timeElapsed)
        {
            string text = $"{TimeStamp} FPS: {FPS} | {descr} | {timeElapsed} |";

            _linesBuffer.Add(text);

            if (_linesBuffer.Count < BUFFER_LENGHT) return;

            _linesBuffer.Add("----------SAVE TO FILE-----------");

            SaveToFile(_path, _linesBuffer);

            _linesBuffer.Clear();
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