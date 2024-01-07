﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using osu_common.Helpers;
using osu_common.Libraries;
using osu_common.Libraries.NetLib;

namespace Updater
{
    public partial class Updater : Form
    {
        private string updateUrl = "http://osu.ppy.sh/release/";
        private string backupUpdateUrl = "http://update.ppy.sh/release/";
        
        static List<DownloadItem> Extras = new List<DownloadItem>();
        static List<DownloadItem> Files = new List<DownloadItem>();
        
        private int autoStartTick = 0;
        private int filesCompleted = 0;
        private int filesProcessing = 0;
        
        // https://learn.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/ms537168(v=vs.85)
        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int featureEntry, 
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable
        );
        
        public Updater(string argument)
        {
            InitializeComponent();
            ConfigManagerCompact.LoadConfig();
            
            bool extraTabVisible = argument == "-extra";
            testBuild.Visible = argument == "-test";
            testBuild.Checked = argument == "-test";

            if (extraTabVisible)
            {
                this.extrasTabWrapper.SelectTab(1);
            }
        }
        
        public void CheckUpdates()
        {
            // TODO: ...
        }

        public void StartGame()
        {
            ConfigManagerCompact.SaveConfig();
            Process.Start(testBuild.Checked ? "osu!test.exe" : "osu!.exe");
            Application.Exit();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.extrasCheckBoxList.Sorted = true;
            
            if (!File.Exists("osu!test.exe"))
            {
                this.testBuild.Text = "Download and use test build (supporters only)";
                if (ConfigManagerCompact.Configuration.ContainsKey("u_UpdaterTestBuild"))
                {
                    ConfigManagerCompact.Configuration.Remove("u_UpdaterTestBuild");
                }
                this.testBuild.Enabled = false;
            }
            
            CoInternetSetFeatureEnabled(
                21, // FEATURE_DISABLE_NAVIGATION_SOUNDS
                2, // SET_FEATURE_ON_PROCESS
                true
            );
            
            this.bgWorker.RunWorkerAsync();
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            ConfigManagerCompact.SaveConfig();
        }

        private void OnStatusUpdateTick(object sender, EventArgs e)
        {
            // TODO: ...
        }

        private void OnBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString().StartsWith("http://osu.ppy.sh/p/changelog?updater"))
                return;

            Process.Start(e.Url.ToString());
            e.Cancel = true;
        }

        private void OnStartButtonClick(object sender, EventArgs e)
        {
            StartGame();
        }

        private void OnAutoStartToggled(object sender, EventArgs e)
        {
            autoStartTick = 0;
            ConfigManagerCompact.Configuration["u_UpdaterAutoStart"] = (autoStart.Checked ? "1" : "0");
        }

        private void OnTestBuildToggled(object sender, EventArgs e)
        {
            if (testBuild.Checked && !File.Exists("osu!test.exe"))
            {
                DownloadItem item = Extras.Find(i => i.CheckFilename == "osu!test.exe");
                
                if (item == null)
                {
                    testBuild.Checked = false;
                    return;
                }

                if (extrasCheckBoxList.Items.IndexOf(item) < 0)
                    return;

                extrasCheckBoxList.SelectedItems.Add(item);
            }

            ConfigManagerCompact.Configuration["u_UpdaterTestBuild"] = (testBuild.Checked ? "1" : "0");
            changelogBrowser.Navigate("http://osu.ppy.sh/p/changelog?updater=" + (testBuild.Checked ? "2" : "1"));
        }
        
        private class DownloadItem
        {
            public readonly string CheckFilename;
            public readonly string DisplayName;
            public readonly string Filename;
            public NetRequest NetRequest;
            public double Progress;
            public bool Patching;

            public DownloadItem(NetRequest nr, string filename, string displayName, string checkFilename)
            {
                this.DisplayName = displayName;
                this.Filename = filename;
                this.CheckFilename = checkFilename;
                this.NetRequest = nr;
            }

            public override string ToString() => this.DisplayName;
        }
    }
}