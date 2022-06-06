using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PWARServerLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // add Processes to RAM
        public Process AccountCacher;

        public Process LauncherServer;
        public Process LobbyServer;
        public Process WorldServer;

        #region Start Button

        /// <summary>
        /// We start AccountCacher.exe,LauncherServer.exe,LobbyServer.exe,
        /// WorldServer.exe by pressing this button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_StartSelected_Click(object sender, RoutedEventArgs e)
        {
            // AccountCacher
            if (File.Exists("AccountCacher.exe"))
            {
                AccountCacher = new Process();
                AccountCacher.StartInfo.FileName = "AccountCacher.exe";
                AccountCacher.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                AccountCacher.Start();
            }
            else
            {
                MessageBox.Show("WARNING! " +
                    "\nCan't find AccountCacher.exe",
                    this.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
            // LauncherServer
            if (File.Exists("LauncherServer.exe"))
            {
                LauncherServer = new Process();
                LauncherServer.StartInfo.FileName = "LauncherServer.exe";
                LauncherServer.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                LauncherServer.Start();
            }
            else
            {
                MessageBox.Show("WARNING! " +
                    "\nCan't find LauncherServer.exe",
                    this.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
            // LobbyServer
            if (File.Exists("LobbyServer.exe"))
            {
                LobbyServer = new Process();
                LobbyServer.StartInfo.FileName = "LobbyServer.exe";
                LobbyServer.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                LobbyServer.Start();
            }
            else
            {
                MessageBox.Show("WARNING! " +
                    "\nCan't find LobbyServer.exe",
                    this.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
            // WorldServer
            if (File.Exists("WorldServer.exe"))
            {
                WorldServer = new Process();
                WorldServer.StartInfo.FileName = "WorldServer.exe";
                WorldServer.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                WorldServer.Start();
            }
            else
            {
                MessageBox.Show("WARNING! " +
                    "\nCan't find WorldServer.exe",
                    this.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
        }

        #endregion Start Button

        #region Stop Button

        /// <summary>
        /// We stop AccountCacher.exe,LauncherServer.exe,LobbyServer.exe,
        /// WorldServer.exe by pressing this button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_StopSelected_Click(object sender, RoutedEventArgs e)
        {
            // WorldServer
            try
            {
                if (WorldServer != null) WorldServer.Kill();
            }
            catch (Exception) { }
            // LobbyServer
            try
            {
                if (LobbyServer != null) LobbyServer.Kill();
            }
            catch (Exception) { }
            // LauncherServer
            try
            {
                if (LauncherServer != null) LauncherServer.Kill();
            }
            catch (Exception) { }
            // AccountCacher
            try
            {
                if (AccountCacher != null) AccountCacher.Kill();
            }
            catch (Exception) { }
        }

        #endregion Stop Button
    }
}