using System;
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

namespace Updater
{
    public partial class Updater : Form
    {
        private string backupUpdateUrl = "http://osu.ppy.sh/release/";
        private string primaryUpdateUrl = "http://update.ppy.sh/release/";
        
        static List<DownloadItem> Extras = new List<DownloadItem>();
        static List<DownloadItem> Files = new List<DownloadItem>();
        
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
            // TODO: ...
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
						primaryUpdateUrl + item.Filename
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
	        // TODO
        }

        private void OnDownloadUpdated(object object_0, long long_0, long long_1)
        {
	        // TODO
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