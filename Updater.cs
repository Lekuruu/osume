using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
            
            // TODO: ...
        }
    }
}