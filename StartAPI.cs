using System;
using System.Diagnostics;

namespace RandomTGBot
{
    public class StartAPI
    {
        private Process _process;

        public void Start()
        {
            if (_process == null || _process.HasExited)
            {
                _process = new Process();
                _process.StartInfo.FileName = @"D:\VisualStudioCSharp\RandomTgAPI\start.bat";
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.CreateNoWindow = true;
                _process.Start();
            }
            else
            {
                Console.WriteLine("Process is already running.");
            }
        }

        public void Stop()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
                _process.WaitForExit();
                _process.Dispose();
                _process = null;
            }
            else
            {
                Console.WriteLine("Process is not running.");
            }
        }
    }
}
