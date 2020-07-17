using Microsoft.Win32;
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
        private bool skipFirstOnEvent = true;
        private AudioManager audio = null;
        private bool muted = false;


        public Form1()
        {
            InitializeComponent();

            audio = new AudioManager();

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey.SetValue("Muter", Application.ExecutablePath);
        }


        private void MonitorOff()
        {
            if (!audio.GetMute())
            {
                audio.SetMute(true);
                muted = true;
            }
        }

        private void MonitorOn()
        {
            if (muted)
            {
                audio.SetMute(false);
                muted = false;
            }
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
                        var settings = (NativeMethods.POWERBROADCAST_SETTING)m.GetLParam(typeof(NativeMethods.POWERBROADCAST_SETTING));

                        switch (settings.Data)
                        {
                            case 0:
                                Console.WriteLine("Monitor Power Off");
                                this.MonitorOff();

                                break;
                            case 1:
                                //SKIP FIRST EVENT
                                Console.WriteLine("Monitor Power On");

                                if (this.skipFirstOnEvent)
                                {
                                    this.skipFirstOnEvent = false;
                                }
                                else
                                {
                                    this.MonitorOn();
                                }

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
            // NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
            base.OnFormClosing(e);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }


}
