using System;
using System.Runtime.InteropServices;

namespace AutoSensitivity
{
    class Win32
    {
        public const int
        WM_DEVICECHANGE = 0x0219;
        public const int
        DBT_DEVICEARRIVAL = 0x8000,
        DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int
        DEVICE_NOTIFY_WINDOW_HANDLE = 0,
        DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        public static Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");

        public const UInt32 SPI_SETMOUSESPEED = 0x0071;
        public const UInt32 SPI_GETMOUSESPEED = 0x0070;

        public const int DIGCF_PRESENT = 2;

        public static Guid GUID_DEVCLASS_MOUSE = new Guid("4D36E96F-E325-11CE-BFC1-08002BE10318");
        public static Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        public static int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004;

        [DllImport("setupapi.dll")]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid,IntPtr Enumerator, IntPtr hWndParent, int Flags);

        [DllImport("setupapi.dll")]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int Supplies, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoGet(uint action, uint param, ref uint vparam, uint init);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoSet(uint action, uint param, uint vparam, uint init);

        /*[StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public int Reserved;
        }*/
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public IntPtr Reserved;
        }

        /*[StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public short dbcc_name;
        }*/
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)] // Tried with or without Pack
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)] // Tried different values.
            public string dbcc_name; // tchar[1]
        };

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(
        IntPtr hRecipient,
        IntPtr NotificationFilter,
        Int32 Flags);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        static int GetOsArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }

        public static string GetOsInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        operatingSystem = vs.Minor == 0 ? "Vista" : "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                operatingSystem += " " + GetOsArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }
    }
}
