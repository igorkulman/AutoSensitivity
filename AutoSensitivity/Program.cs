using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AutoSensitivity.Properties;


namespace AutoSensitivity
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Settings.Default.StartMinimized)
            {
                Form1 form = new Form1();
                Application.Run();
            } else
            {
                Application.Run(new Form1());
            }
        }
    }
}
