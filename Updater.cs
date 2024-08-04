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
using osu_common.Updater;

namespace Updater
{
    public partial class Updater : FormHelper
    {
        private string primaryUpdateUrl = "http://osu.ppy.sh/release/";
        private string backupUpdateUrl = "http://update.ppy.sh/release/";
        
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
                        Delegate0 @delegate = method_12;
                        ((Control)this).Invoke((Delegate)@delegate);
                        string[] array = response.Split(new char[1] { '\n' });
                        string[] array2 = array;
                        foreach (string text2 in array2)
                        {
                            Class97 class2 = new Class97();
                            if (text2.Length != 0)
                            {
                                string[] array3 = text2.Split(new char[1] { ' ' });
                                string text3 = array3[0];
                                string text4 = array3[4].Replace('/', '\\');
                                string string_ = array3[2].Replace('-', ' ');
                                bool flag = false;
                                if (array3.Length >= 4)
                                {
                                    bool flag2 = false;
                                    string[] array4 = array3[3].Split(new char[1] { ',' });
                                    for (int j = 0; j < array4.Length; j++)
                                    {
                                        switch (array4[j])
                                        {
                                        case "extra":
                                            if (!File.Exists(array3[4]))
                                            {
                                                if (text4 == "osu!test.exe")
                                                {
                                                    if (val3 == null)
                                                    {
                                                        val3 = new MethodInvoker(method_13);
                                                    }
                                                    Invoke(val3);
                                                }
                                                method_5(new Class96(null, text3, string_, text4));
                                                flag2 = true;
                                            }
                                            break;
                                        case "diff":
                                            flag = true;
                                            break;
                                        case "noup":
                                        if (!File.Exists(text3))
                                        {
                                        }
                                        break;
                                    case "del":
                                        if (File.Exists(text3))
                                        {
                                            File.Delete(text3);
                                        }
                                        break;
                                    }
                                }
                                if (flag2)
                                {
                                    continue;
                                }
                            }
                            bool flag3 = File.Exists(text4);
                            string text5 = array3[1];
                            class2.string_0 = (flag3 ? smethod_0(text4) : string.Empty);
                            if (!flag3 || !(class2.string_0 == text5))
                            {
                                if (flag3 && flag)
                                {
                                    try
                                    {
                                        int num = 1;
                                        if (list_1 == null)
                                        {
                                            Class24 class3 = new Class24(string_0 + "patches.php");
                                            list_1 = new List<string>(class3.method_0().Split(new char[1] { '\n' }));
                                        }
                                        if (list_1.Count > 0)
                                        {
                                            while (class2.string_0 != text5)
                                            {
                                                string text6 = list_1.Find(class2.method_0);
                                                if (text6 == null)
                                                {
                                                    break;
                                                }
                                                Class98 class4 = new Class98();
                                                class4.class97_0 = class2;
                                                class4.updater_0 = this;
                                                string text7 = text4 + "_patch";
                                                class4.class23_0 = new Class23(text7, string_1 + text6);
                                                class4.class96_0 = new Class96(class4.class23_0, text7 + " (" + num++ + ")", string_, text4);
                                                lock (list_0)
                                                {
                                                    list_0.Add(class4.class96_0);
                                                }
                                                int_2++;
                                                class4.class23_0.method_0(method_3);
                                                class4.class23_0.method_1(class4.method_0);
                                                int num2 = 3;
                                                while (num2-- > 0)
                                                {
                                                    class4.class23_0.vmethod_0();
                                                    if (File.Exists(text7))
                                                    {
                                                        break;
                                                    }
                                                    Thread.Sleep(1000);
                                                }
                                                if (!File.Exists(text7))
                                                {
                                                    MessageBox.Show("Unable to download " + text7 + ". Please check your connection and/or try again later");
                                                }
                                                class4.class96_0.bool_0 = true;
                                                class4.class96_0.double_0 = 0.0;
                                                Class5 class5 = new Class5();
                                                class5.method_0(class4.method_1);
                                                class5.method_1(text4, text4 + "_new", text7, Enum1.const_1);
                                                File.Delete(text7);
                                                class2.string_0 = smethod_1(text4 + "_new");
                                                if (!text6.Contains(class2.string_0))
                                                {
                                                    lock (list_0)
                                                    {
                                                        list_0.Remove(class4.class96_0);
                                                    }
                                                    int_1++;
                                                    break;
                                                }
                                                File.Delete(text4 + "_diff");
                                                method_2(text4 + "_new", text4);
                                                lock (list_0)
                                                {
                                                    list_0.Remove(class4.class96_0);
                                                }
                                                int_1++;
                                            }
                                            if (class2.string_0 == text5)
                                            {
                                                Class6.dictionary_0["h_" + text4] = class2.string_0;
                                                continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex2)
                                    {
                                        MessageBox.Show("error occured: " + ex2);
                                    }
                                }
                                if (File.Exists(text3.Replace('/', '\\') + "_new"))
                                {
                                    File.Delete(text3.Replace('/', '\\') + "_new");
                                }
                                Class23 class6 = new Class23(text3.Replace('/', '\\') + "_new", string_1 + text3 + "?v=" + text5);
                                lock (list_0)
                                {
                                    list_0.Add(new Class96(class6, text3.Replace('/', '\\'), string_, text4));
                                }
                                int_2++;
                                class6.method_1(method_4);
                                class6.method_0(method_3);
                                string empty = string.Empty;
                                do
                                {
                                    empty = class6.string_0;
                                    class6.vmethod_0();
                                }
                                while (class6.string_0 != empty);
                                if (text3 == "_osume.exe")
                                {
                                    break;
                                }
                            }
                        }
                    }
                    bool_0 = true;
                    return;
                }
                catch (Exception ex3)
                {
                    MessageBox.Show("Error\n" + ex3);
                    return;
                }
            }
            MessageBox.Show("An error occurred retrieving the latest update information");
        }
        else
        {
            if (val2 == null)
            {
                val2 = new MethodInvoker(method_10);
            }
            Invoke(val2);
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

        private string GetMd5(string filepath)
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