using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace setStartup
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool setStarting = true;
            if (args.Length==1 && args[0]=="false")
            {
                setStarting = false;
            }
            SetRunAtStartup(setStarting);
            //Console.Write(setStarting);
        }

        private static bool SetRunAtStartup(bool setStarting)
        {
            string exePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof (Program)).CodeBase).Replace("file:\\", "") + "\\autosensitivity.exe";
            
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rkApp == null)
                {
                    return false;
                }
                if (setStarting)
                {
                    rkApp.SetValue("AutoSensitivity", exePath);
                }
                else
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
        }
    }
}
