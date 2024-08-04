using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    public class FormHelper : Form
    {
        public override Font Font
        {
            get
            {
                if (Environment.OSVersion.Version.Major < 6)
                    return SystemFonts.DialogFont;

                return SystemFonts.MessageBoxFont;
            }
            set
            {
                base.Font = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            ResetFonts();
            base.OnLoad(e);
        }

        private void ResetFonts()
        {
            AutoScaleMode = AutoScaleMode.None;

            foreach (Control control in Controls)
            {
                control.Font = Font;
            }
        }

        public void Invoke(MethodInvoker invoker)
        {
            Invoke(invoker, false);
        }

        public void Invoke(MethodInvoker invoker, bool force)
        {
            if (!InvokeRequired)
                invoker.Invoke();

            if (!base.IsHandleCreated)
            {
                Thread.Sleep(3000);
                if (!base.IsHandleCreated)
                    return;
            }

            if (Disposing || IsDisposed)
                return;

            int retryCount = 5;

            while (true)
            {
                retryCount--;

                if (retryCount < 0)
                    return;
                
                if (IsHandleCreated)
                    break;
                
                Thread.Sleep(600);
            }

            base.Invoke(invoker);
        }
    }
}