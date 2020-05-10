using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.CodeDom;
using Microsoft.VisualBasic;
using System.Reflection;

namespace eLOITsModSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Initializes variables from config so they're public
        public static String minecraftDirectory;
        public static String downloadAddress;
        public static String defaultAddress;

        public MainWindow()
        {
            InitializeComponent();

            //Initalizes all variables from config. if any of them fail to initalize, it will reset the config
            minecraftDirectory = getValue("minecraftDirectory");
            downloadAddress = getValue("downloadAddress");
            defaultAddress = getValue("defaultAddress");
            
            //Don't pull values from config once running, only save them
            //Sets current dirctory text block to current minecraft directory
            currentDirectoryTextField.Text = minecraftDirectory;


        }
        //pushes changes to config file
        public static void storeValue(String key, String value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings.Remove(key);
                config.AppSettings.Settings.Add(key, value);
                config.Save(ConfigurationSaveMode.Minimal);
            }catch (Exception e)
            {

            }
        }
        //reads a value from config
        public static String getValue(String key)
        {
            String toReturn = ConfigurationManager.AppSettings.Get(key);
            if (toReturn == null)
            {
                resetConfig();
            }
            Debug.WriteLine("this is toReturn" + toReturn);
            return toReturn;
            
        }
        //Opens a folder with selected folder path
        private void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show(string.Format("{0} Directory does not exist!", folderPath));
            }
        }
        //Method that makes new config
        public static void resetConfig()
        {
            Debug.WriteLine("Resetting config");
            System.Text.StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<configuration>");
            sb.AppendLine("  <appSettings>");
            sb.AppendLine("    <add key=\"minecraftDirectory\" value=\""+ Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft\" />");
            sb.AppendLine("    <add key=\"downloadAddress\" value=\"https://github.com/elliotnash/fabricMods/archive/master.zip\" />");
            sb.AppendLine("    <add key=\"defaultAddress\" value=\"https://github.com/elliotnash/fabricMods/archive/master.zip\" />");
            sb.AppendLine("  </appSettings>");
            sb.AppendLine("</configuration>");


            string loc = Assembly.GetEntryAssembly().Location;
            System.IO.File.WriteAllText(String.Concat(loc, ".config"), sb.ToString());

            minecraftDirectory = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft");
            downloadAddress = "https://github.com/elliotnash/fabricMods/archive/master.zip";
            defaultAddress = "https://github.com/elliotnash/fabricMods/archive/master.zip";

        }


        private void changeDirectory_Click(object sender, RoutedEventArgs e)
        {
            //asks for .minecraft directory
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = minecraftDirectory;
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Minecraft directory";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                minecraftDirectory = dialog.FileName;
                storeValue("minecraftDirectory", minecraftDirectory);
                currentDirectoryTextField.Text = minecraftDirectory;
            }
        }

        private void resetDirectory_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you would like to reset the minecraft directory to %AppData%\\.minecraft?", "Reset Minecraft Directory", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                minecraftDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                storeValue("minecraftDirectory", minecraftDirectory);
                currentDirectoryTextField.Text = minecraftDirectory;

            }
        }

        private void changeAddress_Click(object sender, RoutedEventArgs e)
        {
            inputDialog inputDialog = new inputDialog("Please enter a link to a direct download of a .zip file containing only folder with a minecraft directory. all files inside that folder will be copied to inside the .minecraft folder", downloadAddress);
            if (inputDialog.ShowDialog() == true)
                downloadAddress = inputDialog.Answer;
                storeValue("downloadAddress",downloadAddress);
        }

        private void resetAddress_Click(object sender, RoutedEventArgs e)
        {
            storeValue("downloadAddress", downloadAddress);
            
        }
    }
}





