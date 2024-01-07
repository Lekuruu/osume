using System;
using osu_common.Updater;
using osu_common.Libraries;
using osu_common.Libraries.NetLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        public static string[] Arguments;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            Arguments = args;
            
            if (args.Length > 0 && args[0] == "-completeupdate")
            {
                new Thread(() =>
                {
                    if (KillProcess("osu!") && (!File.Exists("osu!test.exe") || KillProcess("osu!test")))
                    {
                        if (CommonUpdater.MoveInPlace())
                        {
                            string filename = "osu!.exe";
                            
                            if (args.Length > 1 && File.Exists(args[1]))
                                filename = args[1];
                            else if (!File.Exists(filename))
                                filename = "osume.exe";
                            
                            try { Process.Start(filename); }
                            catch { }
                            Environment.Exit(-1);
                        }
                    }
                    // Restart osume.exe
                    try { Process.Start("osume.exe"); }
                    catch { }
                    Environment.Exit(-1);
                }).Start();
                
                Thread.Sleep(400);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Updater((args.Length > 0 ? args[0] : null)));
                return;
            }
            if (Environment.CommandLine.ToString().Contains("_osume.exe"))
            {
                UpdateOsume();
                return;
            }
            
            if (File.Exists("_osume.exe"))
                File.Delete("_osume.exe");

            try
            {
                if (Program.KillProcess("osume", 2))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Updater((args.Length > 0 ? args[0] : null)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        "Sorry, it seems an error has occured :(\nPlease report this on the forums along with as much detail as possible of how it happened.\n{0}\n\n{1}",
                        ex.Message,
                        ex.TargetSite
                    ), 
                    "oops...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand
                );
            }
        }

        internal static bool KillProcess(string processName, int processCount = 1)
        {
            bool killed = Process.GetProcessesByName(processName).Length < processCount;

            if (!killed)
            {
                CloseRunningProcesses(processName, false);
                
                int retries = 30;
                while (!killed && retries-- > 0)
                {
                    Thread.Sleep(200);
                    killed = Process.GetProcessesByName(processName).Length < processCount;
                }
            }

            if (!killed)
            {
                DialogResult action = MessageBox.Show(
                    processName + " is still running!\nWould you like to force-kill and proceed with update?",
                    processName,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Asterisk,
                    MessageBoxDefaultButton.Button1
                );
                
                if (action != DialogResult.OK)
                    return false;
                
                int tries = 0;
                while (tries++ < 15 && !killed)
                {
                    CloseRunningProcesses(processName, true);
                    Thread.Sleep(200);
                    killed = Process.GetProcessesByName(processName).Length < processCount;
                }
                
                if (!killed)
                {
                    MessageBox.Show(
                        "Killing failed.  Please manually kill the process using Task Manager.", 
                        processName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Hand,
                        MessageBoxDefaultButton.Button1
                    );
                    return false;
                }
            }
            
            return true;
        }
        
        private static void CloseRunningProcesses(string processName, bool force)
        {
            try
            {
                int processId = Process.GetCurrentProcess().Id;
                foreach (Process process in Process.GetProcessesByName(processName))
                {
                    if (process.Id != processId && !process.CloseMainWindow() && force)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void UpdateOsume()
        {
            if (!KillProcess("osume", 1))
            {
                return;
            }

            try
            {
                File.Delete("osume.exe");
                File.Copy("_osume.exe", "osume.exe");
                Process.Start("osume.exe");
            }
            catch (Exception)
            {
                Process.Start("_osume.exe");
            }
        }
    }
}