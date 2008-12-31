﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;
using System.Threading;

namespace OpenVPNManager
{
    /// <summary>
    /// Main program
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Holds a reference to the ResourceManager which contains
        /// localized strings.
        /// </summary>
        public static ResourceManager res = new ResourceManager(
            "OpenVPNManager.lang.strings",
            System.Reflection.Assembly.GetExecutingAssembly());

        private static frmGlobalStatus m_mainform;

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Parameters passed to the binary</param>
        [STAThread]
        static void Main(string[] args)
        {
            List<string> arguments = new List<string>(args);

            Microsoft.Win32.SystemEvents.PowerModeChanged += 
                new Microsoft.Win32.PowerModeChangedEventHandler(
                    SystemEvents_PowerModeChanged);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int i = 0;
            while(i < arguments.Count)
            {
                // Install autostart, quit (for setup, e.g.)
                if (arguments[i].ToLower() == "-install-autostart")
                {
                    helper.installAutostart();
                    return;
                }

                // Remove autostart, quit (for setup, e.g.)
                else if (arguments[i].ToLower() == "-remove-autostart")
                {
                    helper.removeAutostart();
                    return;
                }

                // Show help
                else if (arguments[i].ToLower() == "-h" || arguments[i].ToLower() == "-help")
                {
                    MessageBox.Show(res.GetString("ARGS_Help"));
                    return;
                }
                else
                    //arguments.Remove(arguments[i]);
                    ++i;
            }

            Mutex appSingleton = new Mutex(false, Application.ProductName +
                ".SingleInstance");

            if (appSingleton.WaitOne(0, false))
            {
                m_mainform = new frmGlobalStatus(arguments.ToArray());
                Application.Run(m_mainform);
            }
            else
            {
                if (arguments.Count > 0)
                {
                    SimpleComm sc = new SimpleComm(4911);
                    if (!sc.client(arguments.ToArray()))
                        MessageBox.Show(res.GetString("ARGS_Error"));
                }
            }

            appSingleton.Close();
        }

        /// <summary>
        /// Called when the PC is hibernated or woke up.
        /// Used to save and restore all vpn networks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
                m_mainform.closeAll();

            else if (e.Mode == Microsoft.Win32.PowerModes.Resume)
                m_mainform.resumeAll();
        }
    }
}