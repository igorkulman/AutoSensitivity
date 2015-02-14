using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutoSensitivity.Properties;
using Microsoft.Win32;

namespace AutoSensitivity
{
    public partial class Form1 : Form
    {
        private bool _isTouchpadOnly = true;
        private bool _canClose = false;

        private void InitSpeeds()
        {
            uint speed = GetMouseSpeed();
            touchpadBar.Value = Settings.Default.TrackpointSpeed > 0 ? Settings.Default.TrackpointSpeed : (int)speed;
            mouseBar.Value = Settings.Default.MouseSpeed > 0 ? Settings.Default.MouseSpeed : (int)speed;

            SetSpeed(_isTouchpadOnly ? touchpadBar.Value : mouseBar.Value);
        } 

        private static bool IsRunningAtStartup()
        {
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rkApp == null)
                {
                    return false;
                }     
                if (rkApp.GetValue("AutoSensitivity") != null)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /*private static bool SetRunAtStartup(bool setStarting)
        {           
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rkApp==null)
                {                    
                    return false;
                }
                if (setStarting)
                {                   
                    rkApp.SetValue("AutoSensitivity", Application.ExecutablePath.ToString());
                } else
                {
                    if (rkApp.GetValue("AutoSensitivity") != null)
                    {
                        rkApp.DeleteValue("AutoSensitivity", false);
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }*/

        private void InitSettings()
        {
           

            int count = 0;
            IntPtr devinfo = Win32.SetupDiGetClassDevs(
            ref Win32.GUID_DEVCLASS_MOUSE,
            IntPtr.Zero,
            IntPtr.Zero,
            Win32.DIGCF_PRESENT);
            Win32.SP_DEVINFO_DATA devInfoSet =
            new Win32.SP_DEVINFO_DATA
                {
                    cbSize = Marshal.SizeOf(typeof (Win32.SP_DEVINFO_DATA))
                };

            //List<string> devices = new List<string>();
            while (Win32.SetupDiEnumDeviceInfo(devinfo, count,
            ref devInfoSet))
            {
                count++;
                //devices.Add(devInfoSet.DevInst.ToString());
            }
            _isTouchpadOnly = count <= 1;
            label1.Text = count <= 1 ? Resources.TouchpadOnly: Resources.TouchpadAdnMouse;
            runAtStartupCB.Checked = IsRunningAtStartup();//Settings.Default.RunAtStartup;
            startMinimizedCB.Checked = Settings.Default.StartMinimized;
            if (Settings.Default.StartMinimized)
            {
                
                Hide();
            }        
            
        }

        public Form1()
        {
            InitializeComponent();
            InitSettings();
            InitSpeeds();
            RegisterHidNotification();
        }

        private static uint GetMouseSpeed()
        {
            uint mNMouse = 0;
            bool nResult = Win32.SystemParametersInfoGet(Win32.SPI_GETMOUSESPEED, 0, ref mNMouse, 0);

            return mNMouse;
        }      

       

      


        void CheckDevice(ref Message msg)
        {
            IntPtr devinfo = Win32.SetupDiGetClassDevs(ref
            Win32.GUID_DEVCLASS_MOUSE, IntPtr.Zero, IntPtr.Zero, Win32.DIGCF_PRESENT);
            Win32.SP_DEVINFO_DATA devInfoSet = new Win32.SP_DEVINFO_DATA
            {
                cbSize = Marshal.SizeOf(typeof(Win32.SP_DEVINFO_DATA))
            };
            if (Win32.SetupDiEnumDeviceInfo(devinfo, 0, ref devInfoSet))
            {
                int wParam = (int)msg.WParam;
                if (wParam == Win32.DBT_DEVICEARRIVAL)
                {
                    _isTouchpadOnly = false;
                    label1.Text = Resources.TouchpadAdnMouse;
                    SetSpeed(mouseBar.Value);
                    //notifyIcon1.ShowBalloonTip(1000, "AutoSensitivity", "Mouse connected", ToolTipIcon.Info);
                }
                else if (wParam == Win32.DBT_DEVICEREMOVECOMPLETE)
                {
                    _isTouchpadOnly = true;
                    label1.Text = Resources.TouchpadOnly;
                    SetSpeed(touchpadBar.Value);
                    //notifyIcon1.ShowBalloonTip(1000, "AutoSensitivity", "Mouse disconnected", ToolTipIcon.Info);
                } 
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_DEVICECHANGE: CheckDevice(ref m); break;
            }
            base.WndProc(ref m);
        }

       

        void RegisterHidNotification()
        {
            Win32.DEV_BROADCAST_DEVICEINTERFACE dbi = new
            Win32.DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(dbi);
            dbi.dbcc_size = size;
            dbi.dbcc_devicetype = Win32.DBT_DEVTYP_DEVICEINTERFACE;
            dbi.dbcc_reserved = 0;
            dbi.dbcc_classguid = Win32.GUID_DEVINTERFACE_HID;
            dbi.dbcc_name = string.Empty;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(dbi, buffer, true);
            IntPtr r = Win32.RegisterDeviceNotification(Handle, buffer, Win32.DEVICE_NOTIFY_WINDOW_HANDLE | Win32.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
            if (r == IntPtr.Zero)
            {
                label1.Text = Win32.GetLastError().ToString();
            }
        }

        private void Button1Click(object sender, EventArgs e)
        {
            uint speed = GetMouseSpeed();
            touchpadBar.Value = (int) speed;
        }

        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(
                "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WND2WGRGBLQVE&lc=SK&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void LinkLabel2LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://autosensitivity.codeplex.com");
        }

        private void Button3Click(object sender, EventArgs e)
        {
            Settings.Default.TrackpointSpeed = touchpadBar.Value;
            Settings.Default.Save();
            
            if (_isTouchpadOnly)
            {
                SetSpeed(touchpadBar.Value);
            }
        }

        private static bool SetSpeed(int speed)
        {
            bool res = Win32.SystemParametersInfoSet(
                Win32.SPI_SETMOUSESPEED,
                0,
                (uint)speed,
                0);

            return res;
        }

        private void Button4Click(object sender, EventArgs e)
        {          
            Settings.Default.MouseSpeed = mouseBar.Value;
            Settings.Default.Save();

            if (!_isTouchpadOnly)
            {
                SetSpeed(touchpadBar.Value);
            }
        }

      

        private void Button2Click(object sender, EventArgs e)
        {
            uint speed = GetMouseSpeed();
            mouseBar.Value = (int)speed;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void NotifyIcon1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Button5Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            _canClose = true;
            Application.Exit();
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_canClose) return;
            e.Cancel = true;
            Hide();
        }

       

        private void ToolStripMenuItem2Click(object sender, EventArgs e)
        {
            try
            {
                ApplicationDeployment updater = ApplicationDeployment.CurrentDeployment;
                bool verDepServer = updater.CheckForUpdate();
                if (verDepServer) // Update available
                {
                    DialogResult res = MessageBox.Show(
                        Resources.NewVersionYes,
                        Resources.Updater, MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        updater.Update();
                        MessageBox.Show(Resources.RestartMsg);
                    }
                }
                else
                {
                    MessageBox.Show(Resources.NewVersionNo, Resources.Updater);
                }
            } catch
            {
                MessageBox.Show(Resources.NewVersionNo, Resources.Updater);
            }
        }

        private void StartMinimizedCbCheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.StartMinimized = startMinimizedCB.Checked;
            Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Settings.Default.StartMinimized)
            {
                //this.WindowState = FormWindowState.Minimized;
                Hide();
            }
            /*string win = Win32.GetOsInfo();
            if (win.Contains("XP"))
            {
                //runAtStartupCB.Visible = false;
            }*/
        }

       

       
        private void ToolStripMenuItem3Click1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(
           "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WND2WGRGBLQVE&lc=SK&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
       
        }

        private void RunAtStartupCbClick(object sender, EventArgs e)
        {
            if (runAtStartupCB.Checked)
            {
                startMinimizedCB.Checked = true;
            }

            string win = Win32.GetOsInfo();
            if (win.Contains("XP"))
            {
                Process.Start(
               new ProcessStartInfo
               {                 
                   FileName = typeof(setStartup.Program).Assembly.Location,
                   UseShellExecute = true,
                   CreateNoWindow = true, // Optional....
                   Arguments = runAtStartupCB.Checked.ToString().ToLower()
               }).WaitForExit();

                return;
            }

            Process.Start(
               new ProcessStartInfo
               {
                   Verb = "runas",
                   FileName = typeof(setStartup.Program).Assembly.Location,
                   UseShellExecute = true,
                   CreateNoWindow = true, // Optional....
                   Arguments = runAtStartupCB.Checked.ToString().ToLower()
               }).WaitForExit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Refresh(true);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Refresh(true);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            Refresh(false);
        }

        private void Refresh(bool show)
        {
            InitSettings();
            if (show)
            {
                Show();
            } else
            {
                Hide();
            }
        }

       

       
    }
}
