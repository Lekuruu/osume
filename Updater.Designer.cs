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

namespace Updater
{
    partial class Updater
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resourceManager = new System.ComponentModel.ComponentResourceManager(typeof(Updater));
            
            this.changelogBrowser = new WebBrowser();
			this.progressBar = new ProgressBar();
			this.buttonStart = new Button();
			this.statusTextWrapper = new Panel();
			this.bottomPanelWrapper = new Panel();
			this.bottomPanel = new Panel();
			this.statusText = new TextBox();
			this.checkBoxWrapper = new Panel();
			this.checkBoxPanel = new Panel();
			this.autoStart = new CheckBox();
			this.testBuild = new CheckBox();
			this.extrasTabWrapper = new TabControl();
			this.changelogTab = new TabPage();
			this.extrasTab = new TabPage();
			this.packagesTextBox = new TextBox();
			this.extrasCheckBoxList = new CheckedListBox();
			this.statusUpdater = new System.Windows.Forms.Timer(this.components);
			this.bgWorker = new BackgroundWorker();
			
			this.statusTextWrapper.SuspendLayout();
			this.bottomPanelWrapper.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.checkBoxWrapper.SuspendLayout();
			this.checkBoxPanel.SuspendLayout();
			this.extrasTabWrapper.SuspendLayout();
			this.changelogTab.SuspendLayout();
			this.extrasTab.SuspendLayout();
			this.SuspendLayout();
			
			this.changelogBrowser.AllowWebBrowserDrop = false;
			this.changelogBrowser.IsWebBrowserContextMenuEnabled = false;
			this.changelogBrowser.Location = new Point(0, 0);
			this.changelogBrowser.Margin = new Padding(0);
			this.changelogBrowser.MinimumSize = new Size(20, 20);
			this.changelogBrowser.Name = "changelogBrowser";
			this.changelogBrowser.ScriptErrorsSuppressed = true;
			this.changelogBrowser.Size = new Size(485, 284);
			this.changelogBrowser.TabIndex = 4;
			this.changelogBrowser.WebBrowserShortcutsEnabled = false;
			this.changelogBrowser.Navigating += this.OnBrowserNavigating;
			
			this.progressBar.Dock = DockStyle.Fill;
			this.progressBar.Location = new Point(0, 0);
			this.progressBar.MarqueeAnimationSpeed = 1;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new Size(494, 34);
			this.progressBar.Style = ProgressBarStyle.Continuous;
			this.progressBar.TabIndex = 1;
			
			this.buttonStart.Cursor = Cursors.Hand;
			this.buttonStart.DialogResult = DialogResult.Cancel;
			this.buttonStart.Dock = DockStyle.Fill;
			this.buttonStart.FlatStyle = FlatStyle.System;
			this.buttonStart.Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.buttonStart.Location = new Point(0, 0);
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.Size = new Size(494, 34);
			this.buttonStart.TabIndex = 2;
			this.buttonStart.Text = "start osu!";
			this.buttonStart.UseVisualStyleBackColor = false;
			this.buttonStart.Visible = false;
			this.buttonStart.Click += this.OnStartButtonClick;
			
			this.statusTextWrapper.Controls.Add(this.bottomPanelWrapper);
			this.statusTextWrapper.Controls.Add(this.statusText);
			this.statusTextWrapper.Dock = DockStyle.Bottom;
			this.statusTextWrapper.Location = new Point(0, 365);
			this.statusTextWrapper.Name = "statusTextWrapper";
			this.statusTextWrapper.Size = new Size(494, 47);
			this.statusTextWrapper.TabIndex = 4;
			
			this.bottomPanelWrapper.Controls.Add(this.bottomPanel);
			this.bottomPanelWrapper.Dock = DockStyle.Fill;
			this.bottomPanelWrapper.Location = new Point(0, 13);
			this.bottomPanelWrapper.Name = "bottomPanelWrapper";
			this.bottomPanelWrapper.Size = new Size(494, 34);
			this.bottomPanelWrapper.TabIndex = 4;
			
			this.bottomPanel.Controls.Add(this.buttonStart);
			this.bottomPanel.Controls.Add(this.progressBar);
			this.bottomPanel.Dock = DockStyle.Fill;
			this.bottomPanel.Location = new Point(0, 0);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new Size(494, 34);
			this.bottomPanel.TabIndex = 4;
			
			this.statusText.BackColor = SystemColors.Control;
			this.statusText.BorderStyle = BorderStyle.None;
			this.statusText.Cursor = Cursors.Arrow;
			this.statusText.Dock = DockStyle.Top;
			this.statusText.Enabled = false;
			this.statusText.Location = new Point(0, 0);
			this.statusText.Name = "statusText";
			this.statusText.ReadOnly = true;
			this.statusText.Size = new Size(494, 13);
			this.statusText.TabIndex = 0;
			this.statusText.TextAlign = HorizontalAlignment.Center;
			this.statusText.WordWrap = false;
			
			this.checkBoxWrapper.Controls.Add(this.checkBoxPanel);
			this.checkBoxWrapper.Controls.Add(this.extrasTabWrapper);
			this.checkBoxWrapper.Dock = DockStyle.Fill;
			this.checkBoxWrapper.Location = new Point(0, 0);
			this.checkBoxWrapper.Name = "checkBoxWrapper";
			this.checkBoxWrapper.Size = new Size(494, 365);
			this.checkBoxWrapper.TabIndex = 5;
			
			this.checkBoxPanel.Controls.Add(this.autoStart);
			this.checkBoxPanel.Controls.Add(this.testBuild);
			this.checkBoxPanel.Dock = DockStyle.Bottom;
			this.checkBoxPanel.Location = new Point(0, 316);
			this.checkBoxPanel.Name = "checkBoxPanel";
			this.checkBoxPanel.Size = new Size(494, 49);
			this.checkBoxPanel.TabIndex = 10;
			
			this.autoStart.Location = new Point(12, 14);
			this.autoStart.Name = "autoStart";
			this.autoStart.Size = new Size(243, 19);
			this.autoStart.TabIndex = 7;
			this.autoStart.Text = "Automatically start osu! after updating";
			this.autoStart.UseVisualStyleBackColor = true;
			this.autoStart.CheckedChanged += this.OnAutoStartToggled;
			this.testBuild.Location = new Point(328, 14);
			this.testBuild.Name = "testBuild";
			this.testBuild.Size = new Size(154, 19);
			this.testBuild.TabIndex = 9;
			this.testBuild.Text = "Use test build";
			this.testBuild.UseVisualStyleBackColor = true;
			this.testBuild.Visible = false;
			this.testBuild.CheckedChanged += this.OnTestBuildToggled;
			
			this.extrasTabWrapper.Controls.Add(this.changelogTab);
			this.extrasTabWrapper.Controls.Add(this.extrasTab);
			this.extrasTabWrapper.Dock = DockStyle.Top;
			this.extrasTabWrapper.Enabled = false;
			this.extrasTabWrapper.Location = new Point(0, 0);
			this.extrasTabWrapper.Name = "extrasTabWrapper";
			this.extrasTabWrapper.SelectedIndex = 0;
			this.extrasTabWrapper.Size = new Size(494, 310);
			this.extrasTabWrapper.TabIndex = 8;
			
			this.changelogTab.Controls.Add(this.changelogBrowser);
			this.changelogTab.Location = new Point(4, 22);
			this.changelogTab.Name = "changelogTab";
			this.changelogTab.Padding = new Padding(3);
			this.changelogTab.Size = new Size(486, 284);
			this.changelogTab.TabIndex = 0;
			this.changelogTab.Text = "Changelog";
			this.changelogTab.UseVisualStyleBackColor = true;
			
			this.extrasTab.BackColor = SystemColors.Control;
			this.extrasTab.Controls.Add(this.packagesTextBox);
			this.extrasTab.Controls.Add(this.extrasCheckBoxList);
			this.extrasTab.Location = new Point(4, 22);
			this.extrasTab.Name = "extrasTab";
			this.extrasTab.Padding = new Padding(3);
			this.extrasTab.Size = new Size(486, 284);
			this.extrasTab.TabIndex = 1;
			this.extrasTab.Text = "Extras";
			
			this.packagesTextBox.BackColor = SystemColors.Control;
			this.packagesTextBox.BorderStyle = BorderStyle.None;
			this.packagesTextBox.Dock = DockStyle.Top;
			this.packagesTextBox.Enabled = false;
			this.packagesTextBox.Location = new Point(3, 3);
			this.packagesTextBox.Multiline = true;
			this.packagesTextBox.Name = "packagesTextBox";
			this.packagesTextBox.Size = new Size(480, 21);
			this.packagesTextBox.TabIndex = 6;
			this.packagesTextBox.Text = "Please select any optional packages you would like to install.";
			
			this.extrasCheckBoxList.CheckOnClick = true;
			this.extrasCheckBoxList.Dock = DockStyle.Bottom;
			this.extrasCheckBoxList.FormattingEnabled = true;
			this.extrasCheckBoxList.IntegralHeight = false;
			this.extrasCheckBoxList.Location = new Point(3, 27);
			this.extrasCheckBoxList.Margin = new Padding(0);
			this.extrasCheckBoxList.Name = "extrasCheckBoxList";
			this.extrasCheckBoxList.Size = new Size(480, 254);
			this.extrasCheckBoxList.TabIndex = 5;
			
			this.statusUpdater.Tick += this.OnStatusUpdateTick;
			this.bgWorker.DoWork += (object sender, DoWorkEventArgs e) =>
			{
				this.CheckUpdates();
			};
			
			this.AutoScaleDimensions = new SizeF(6f, 13f);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.AutoValidate = AutoValidate.EnableAllowFocusChange;
			
			this.CancelButton = this.buttonStart;
			this.ClientSize = new Size(494, 412);
			this.Controls.Add(this.checkBoxWrapper);
			this.Controls.Add(this.statusTextWrapper);
			
			this.Enabled = false;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.Icon = (Icon)resourceManager.GetObject("$this.Icon");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			
			this.SizeGripStyle = SizeGripStyle.Hide;
			this.StartPosition = FormStartPosition.CenterScreen;
			
			this.Text = "osu! updater";
			this.Name = "Updater";
			
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClose);
			this.Load += new System.EventHandler(this.OnLoad);
			
			this.statusTextWrapper.ResumeLayout(false);
			this.statusTextWrapper.PerformLayout();
			this.bottomPanelWrapper.ResumeLayout(false);
			this.bottomPanel.ResumeLayout(false);
			this.checkBoxWrapper.ResumeLayout(false);
			this.checkBoxPanel.ResumeLayout(false);
			this.extrasTabWrapper.ResumeLayout(false);
			this.changelogTab.ResumeLayout(false);
			this.extrasTab.ResumeLayout(false);
			this.extrasTab.PerformLayout();
			this.ResumeLayout(false);
        }

        #endregion
        
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Panel statusTextWrapper;
        private System.Windows.Forms.Panel checkBoxWrapper;
        private System.Windows.Forms.Panel bottomPanelWrapper;
        private System.Windows.Forms.WebBrowser changelogBrowser;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.TextBox statusText;
        private System.Windows.Forms.CheckBox autoStart;
        private System.Windows.Forms.TabControl extrasTabWrapper;
        private System.Windows.Forms.TabPage changelogTab;
        private System.Windows.Forms.CheckedListBox extrasCheckBoxList;
        private System.Windows.Forms.TabPage extrasTab;
        private System.Windows.Forms.Timer statusUpdater;
        private System.Windows.Forms.TextBox packagesTextBox;
        private System.ComponentModel.BackgroundWorker bgWorker;
        private System.Windows.Forms.Panel checkBoxPanel;
        private System.Windows.Forms.CheckBox testBuild;
    }
}