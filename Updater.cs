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
using ICSharpCode.SharpZipLib.Zip;
using osu_common.Helpers;
using osu_common.Libraries;
using osu_common.Libraries.NetLib;
using osu_common.Updater;

namespace Updater
{
    public partial class Updater : FormHelper
    {
        private string primaryUpdateUrl = "http://osu.ppy.sh/release/";
        private string backupUpdateUrl = "http://update.ppy.sh/release/";
        
        static List<DownloadItem> Extras = new List<DownloadItem>();
        static List<DownloadItem> Files = new List<DownloadItem>();
        static List<String> Checksums;
        
        private int autoStartTick = 0;
        private int filesCompleted = 0;
        private int filesProcessing = 0;
        private double progress = 0;

        private bool checkedForUpdates = false;
        private bool extraTabVisible = false;
        
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
            
            extraTabVisible = argument == "-extra";
            testBuild.Visible = argument == "-test";
            testBuild.Checked = argument == "-test";

            if (extraTabVisible)
            {
                this.extrasTabWrapper.SelectTab(1);
            }
        }

        public void CheckUpdates()
        {
            CommonUpdater.Cleanup();
            Invoke(delegate { this.statusUpdater.Start(); });

            if (!Directory.Exists("Songs"))
                Directory.CreateDirectory("Songs");

            if (ConfigManagerCompact.Configuration.ContainsKey("u_UpdaterAutoStart"))
                Invoke(delegate { autoStart.Checked = ConfigManagerCompact.Configuration["u_UpdaterAutoStart"] == "1"; });

            try
            {
                Invoke(delegate { this.changelogBrowser.Navigate("http://osu.ppy.sh/p/changelog?updater=" + (testBuild.Checked ? "2" : "1")); });
            }
            catch
            {
                MessageBox.Show("Couldn't load changelog. It seems your IE6 install is corrupt.");
            }

            if (Program.KillProcess("osu!") && (!File.Exists("osu!test.exe") || Program.KillProcess("osu!test")))
            {
                Invoke(delegate { Enabled = true; });
                
                string response;
                try
                {
                    StringNetRequest request = new StringNetRequest(backupUpdateUrl + "update?time=" + DateTime.Now.Ticks);
                    response = request.BlockingPerform();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred retrieving the latest update information:\n" + ex);
                    return;
                }
                
                if (response.Length != 0)
                {
                    try
                    {
                        base.Invoke(delegate { extrasTabWrapper.Enabled = true; });
                        string[] lines = response.Split('\n');
                        
                        foreach (string line in lines)
                        {
                            if (line.Length != 0)
                            {
                                string[] lineContents = line.Split(' ');
                                string remoteFile = lineContents[0];
                                string localFilename = lineContents[4].Replace('/', '\\');
                                string description = lineContents[2].Replace('-', ' ');
                                bool isDiff = false;
                                
                                if (lineContents.Length >= 4)
                                {
                                    bool isExtra = false;
                                    string[] actions = lineContents[3].Split(new char[1] { ',' });
                                    
                                    foreach (string action in actions)
                                    {
                                        switch (action)
                                        {
                                            case "extra":
                                                if (!File.Exists(lineContents[4]))
                                                {
                                                    if (localFilename == "osu!test.exe")
                                                        Invoke(() => testBuild.Enabled = true);
                                                    
                                                    AddExtra(new DownloadItem(null, remoteFile, description, localFilename));
                                                    isExtra = true;
                                                }
                                                break;
                                            case "diff":
                                                isDiff = true;
                                                break;
                                            case "noup":
                                                if (!File.Exists(remoteFile))
                                                    continue;
                                                break;
                                            case "del":
                                                if (File.Exists(remoteFile))
                                                {
                                                    File.Delete(remoteFile);
                                                }
                                                break;
                                        } 
                                    }
                                    
                                    if (isExtra)
                                        continue;
                                }
                                
                                bool localFileExists = File.Exists(localFilename);
                                string remoteChecksum = lineContents[1];
                                string localChecksum = GetMd5Cached(localFilename);
                                
                                if (!localFileExists || localChecksum != remoteChecksum)
                                {
                                    if (localFileExists && isDiff)
                                    {
                                        try
                                        {
                                            int index = 1;
                                            
                                            if (Checksums == null)
                                            {
                                                StringNetRequest request = new StringNetRequest(primaryUpdateUrl + "patches.php");
                                                Checksums = new List<string>(request.BlockingPerform().Split('\n'));
                                            }
                                            
                                            if (Checksums.Count > 0)
                                            {
                                                while (localChecksum != remoteChecksum)
                                                {
                                                    string patchFile = Checksums.Find(item => item.Contains(localChecksum + "_"));
                                                    
                                                    if (patchFile == null)
                                                        break;
                                                    
                                                    string patchFilename = localFilename + "_patch";
                                                    FileNetRequest request = new FileNetRequest(patchFilename, backupUpdateUrl + patchFile);
                                                    DownloadItem downloadItem = new DownloadItem(request, patchFilename + " (" + index++ + ")", description, localFilename);
                                                    
                                                    lock (Files)
                                                        Files.Add(downloadItem);
                                                    
                                                    filesProcessing++;
                                                    request.onUpdate += OnDownloadUpdated;
                                                    request.onFinish += delegate(string path, Exception e)
                                                    {
                                                        if (e != null)
                                                            request.m_url = request.m_url.Replace(backupUpdateUrl, primaryUpdateUrl);
                                                    };
                                                    
                                                    int retries = 3;
                                                    while (retries-- > 0)
                                                    {
                                                        request.Perform();
                                                        
                                                        if (File.Exists(patchFilename))
                                                            break;
                                                        
                                                        Thread.Sleep(1000);
                                                    }
                                                    
                                                    if (!File.Exists(patchFilename))
                                                    {
                                                        MessageBox.Show("Unable to download " + patchFilename + ". Please check your connection and/or try again later");
                                                    }
                                                    
                                                    downloadItem.Patching = true;
                                                    downloadItem.Progress = 0.0;

                                                    BSPatcher patcher = new BSPatcher();
                                                    patcher.OnProgress += delegate(object sender, long current, long total)
                                                    {
                                                        downloadItem.Progress = ((float)current / total) * 100;
                                                    };
                                                    
                                                    patcher.Patch(localFilename, localFilename + "_new", patchFilename, Compression.BZip2);
                                                    File.Delete(patchFilename);
                                                    
                                                    localChecksum = GetMd5(localFilename + "_new");
                                                    if (!patchFile.Contains(localChecksum))
                                                    {
                                                        lock (Files)
                                                            Files.Remove(downloadItem);
                                                        filesCompleted++;
                                                        break;
                                                    }
                                                    
                                                    File.Delete(localFilename + "_diff");
                                                    MoveFile(localFilename + "_new", localFilename);
                                                    
                                                    lock (Files)
                                                        Files.Remove(downloadItem);
                                                    filesCompleted++;
                                                }
                                                
                                                if (localChecksum == remoteChecksum)
                                                {
                                                    ConfigManagerCompact.Configuration["h_" + localFilename] = localChecksum;
                                                    continue;
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            MessageBox.Show("error occured: " + e);
                                        }
                                    }
                                    
                                    if (File.Exists(remoteFile.Replace('/', '\\') + "_new"))
                                        File.Delete(remoteFile.Replace('/', '\\') + "_new");
                                    
                                    FileNetRequest netRequest = new FileNetRequest(remoteFile.Replace('/', '\\') + "_new", backupUpdateUrl + remoteFile + "?v=" + remoteChecksum);
                                    
                                    lock (Files)
                                        Files.Add(new DownloadItem(netRequest, remoteFile.Replace('/', '\\'), description, localFilename));
                                    
                                    filesProcessing++;
                                    netRequest.onFinish += OnDownloadFinished;
                                    netRequest.onUpdate += OnDownloadUpdated;
                                    
                                    string previousUrl = string.Empty;
                                    do
                                    {
                                        previousUrl = netRequest.m_url;
                                        netRequest.Perform();
                                    }
                                    while (netRequest.m_url != previousUrl);
                                    
                                    if (remoteFile == "_osume.exe")
                                        break;
                                }
                            }
                        }
                        
                        checkedForUpdates = true;
                        return;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error\n" + e);
                        return;
                    }
                }
                
                MessageBox.Show("An error occurred retrieving the latest update information");
            }
            else
            {
                Invoke(delegate
                {
                    MessageBox.Show(this, "Couldn't close osu!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                });
            }
        }

        public void StartGame()
        {
            ConfigManagerCompact.SaveConfig();
            Process.Start(testBuild.Checked ? "osu!test.exe" : "osu!.exe");
            Application.Exit();
        }
        
        private void MoveFile(string sourcePath, string dstPath)
        {
            int retries = 5;

            while (retries-- > 0)
            {
                try { File.Delete(dstPath); }
                catch { }

                try
                {
                    File.Move(sourcePath, dstPath);
                    return;
                }
                catch
                {
                    try
                    {
                        File.Copy(sourcePath, dstPath, true);
                        File.Delete(sourcePath);
                        return;
                    }
                    catch { }
                }
                Thread.Sleep(600);
            }
            MessageBox.Show("Unable to relpace file " + dstPath + ". Please check this file isn't still open then try running the updater again.");
            Environment.Exit(-1);
        }

        private static string GetMd5Cached(string filename)
        {
            try
            {
                if (!ConfigManagerCompact.Configuration.ContainsKey("h_" + filename) || filename == "osume.exe")
                    ConfigManagerCompact.Configuration["h_" + filename] = GetMd5(filename);

                return ConfigManagerCompact.Configuration["h_" + filename]; 
            }
            catch
            {
                MessageBox.Show("Error getting Md5sum of file " + filename);
                return "fail";
            }
        }

        private static string GetMd5(string filepath)
        {
            try
            {
                Stream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                MD5 md5 = MD5.Create();
                byte[] array = md5.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(array[i].ToString("x2"));
                }
                stream.Close();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error\n" + ex);
                return "fail";
            }
        }

        private void AddExtra(DownloadItem item)
        {
            Extras.Add(item);
            Invoke(() => { extrasCheckBoxList.Items.Add(item); });
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
            try
            {
                if (this.extrasCheckBoxList.SelectedItems.Count > 0)
                {
                    DownloadItem item = (DownloadItem)this.extrasCheckBoxList.Items[
                        this.extrasCheckBoxList.SelectedIndex
                    ];

                    if (item == null)
                        return;

                    if (File.Exists(item.Filename + "_new"))
                        File.Delete(item.Filename + "_new");

                    FileNetRequest nr = new FileNetRequest(
                        item.Filename + "_new",
                        backupUpdateUrl + item.Filename
                    );

                    item.NetRequest = nr;

                    lock (Files)
                        Files.Add(item);

                    nr.onFinish += OnDownloadFinished;
                    nr.onUpdate += OnDownloadUpdated;

                    NetManager.AddRequest(nr);
                    this.filesProcessing++;

                    if (this.extrasCheckBoxList.Items.Contains(item))
                        this.extrasCheckBoxList.Items.Remove(item);

                    this.extrasCheckBoxList.SelectedItems.Clear();
                }
                if (this.filesProcessing == this.filesCompleted)
                {
                    if (!checkedForUpdates)
                    {
                        this.statusText.Text = "Checking for updates...";
                        this.buttonStart.Visible = false;
                    }
                    else
                    {
                        if (this.statusText.Text != "All done!")
                        {
                            this.statusText.Text = "All done!";
                            this.buttonStart.Visible = true;
                            this.filesCompleted = 0;
                            this.filesProcessing = 0;

                            if (File.Exists("osu!test.exe"))
                                this.testBuild.Text = "Use test build";

                            this.buttonStart.Focus();
                        }
                        if (
                            autoStart.Checked &&
                            extrasTabWrapper.SelectedIndex != 1 &&
                            !extraTabVisible &&
                            autoStartTick++ > 10
                        )
                            StartGame();
                    }
                }
                else
                {
                    lock (Files)
                    {
                        if (Files.Count <= 0)
                            return;

                        buttonStart.Visible = false;
                        if (Files[0].Patching)
                        {
                            this.statusText.Text = string.Format(
                                "Patching {0}... {1:0}%",
                                Files[0].Filename,
                                Files[0].Progress
                            );
                        }
                        else
                        {
                            this.statusText.Text = string.Format(
                                "Downloading {3} {2:0}%", new object[]
                                {
                                    this.filesCompleted + 1,
                                    this.filesProcessing,
                                    Files[0].Progress,
                                    Files[0].Filename
                                }
                            );
                        }

                        progressBar.Style = ProgressBarStyle.Continuous;
                        int newProgress = (int)Files[0].Progress;

                        if (newProgress >= 0 && newProgress <= 100)
                            progressBar.Value = newProgress;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnDownloadFinished(string location, Exception e)
        {
            string filename = location.Replace("_new", "");

            DownloadItem item = Files.Find(i => i.Filename == filename);

            if (e != null)
            {
                if (item.NetRequest.m_url.Contains(backupUpdateUrl))
                {
                    item.NetRequest.m_url = item.NetRequest.m_url.Replace(backupUpdateUrl, primaryUpdateUrl);
                    return;
                }

                MessageBox.Show(
                    string.Format(
                        "An error was encountered while downloading the file: {0}\n\nPlease restart the updater and report this error.\n{1}",
                        location, e.ToString()
                    )
                );
                return;
            }

            try
            {
                if (item == null)
                {
                    MessageBox.Show(
                        string.Format(
                            "Internal error on file:{0}\n\nPlease restart the updater and report this error.",
                            filename
                        )
                    );
                }
                else
                {
                    if (File.Exists(filename))
                        File.Delete(filename);

                    MoveFile(location, filename);

                    if (Path.GetExtension(filename) == ".zip")
                    {
                        if (File.Exists(item.CheckFilename))
                            File.Delete(item.CheckFilename);

                        new FastZip().ExtractZip(
                            filename,
                            ".\\",
                            FastZip.Overwrite.Always,
                            null,
                            ".*",
                            ".*",
                            false
                        );

                        File.Delete(filename);
                    }

                    if (filename == "_osume.exe")
                    {
                        Process.Start("_osume.exe");
                        Application.Exit();
                    }
                    else
                    {
                        ConfigManagerCompact.Configuration["h_" + filename] = GetMd5(filename);

                        lock (Files)
                            Files.Remove(item);

                        this.filesCompleted++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        "An error was encountered while downloading the file:{0}\n\nPlease report this error:\n{1}",
                        location,
                        ex
                    )
                );
            }
        }

        private void OnDownloadUpdated(object sender, long current, long total)
        {
            try
            {
                DownloadItem item = Files.Find(i => i.NetRequest == sender);

                if (item != null)
                {
                    progress = (float)current / total * 100;
                    item.Progress = progress;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error\n" + ex);
            }
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