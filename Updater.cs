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
using osu_common.Updater;
using osu_common.Helpers;
using osu_common.Libraries;
using osu_common.Libraries.NetLib;

namespace Updater
{
    public partial class Updater : Form
    {
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

        private void OnLoad(object sender, EventArgs e)
        {
            // TODO: ...
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            // TODO: ...
        }

        private void OnStatusUpdate(object sender, EventArgs e)
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
            // TODO: ...
        }

        private void OnAutoStartToggled(object sender, EventArgs e)
        {
            // TODO: ...
        }

        private void OnTestBuildToggled(object sender, EventArgs e)
        {
            // TODO: ...
        }
    }
}