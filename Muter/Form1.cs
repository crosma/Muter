using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void toolStripMenuIExit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
            Application.ExitThread();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                this.Hide();
                this.Show();
                this.BringToFront();
            }

        }


        private IntPtr unRegPowerNotify = IntPtr.Zero;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var settingGuid = new NativeMethods.PowerSettingGuid();
            Guid powerGuid = IsWindows8Plus() ? settingGuid.ConsoleDisplayState : settingGuid.MonitorPowerGuid;

            unRegPowerNotify = NativeMethods.RegisterPowerSettingNotification(this.Handle, ref powerGuid, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        private bool IsWindows8Plus()
        {
            var version = Environment.OSVersion.Version;
            if (version.Major > 6) return true; // Windows 10+
            if (version.Major == 6 && version.Minor > 1) return true; // Windows 8+
            return false;  // Windows 7 or less
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_POWERBROADCAST:
                    if (m.WParam == (IntPtr)NativeMethods.PBT_POWERSETTINGCHANGE)
                    {
                        var settings = (NativeMethods.POWERBROADCAST_SETTING)m.GetLParam(
                            typeof(NativeMethods.POWERBROADCAST_SETTING));
                        switch (settings.Data)
                        {
                            case 0:
                                Console.WriteLine("Monitor Power Off");
                                break;
                            case 1:
                                //SKIP FIRST EVENT
                                Console.WriteLine("Monitor Power On");
                                break;
                            case 2:
                                Console.WriteLine("Monitor Dimmed");
                                break;
                        }
                    }
                    m.Result = (IntPtr)1;
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
            base.OnFormClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AudioManager.SetMasterVolumeMute(!AudioManager.GetMasterVolumeMute());
            Console.WriteLine(AudioManager.GetMasterVolumeMute());
        }
    }


}
