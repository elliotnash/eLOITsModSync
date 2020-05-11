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
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;

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
        public static String workingDirectory;
        public static bool useOptifine;
        

        public MainWindow()
        {
            InitializeComponent();

            //Initalizes all variables from config. if any of them fail to initalize, it will reset the config
            minecraftDirectory = getValue("minecraftDirectory");
            downloadAddress = getValue("downloadAddress");
            defaultAddress = getValue("defaultAddress");
            workingDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //initalizes optifine checkbox.
            if (getValue("useOptifine") == "true")
            {
                optifineBox.IsChecked = true;
            } else
            {
                optifineBox.IsChecked = false;
            }


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
            sb.AppendLine("    <add key=\"useOptifine\" value=\"false\" />");
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
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you would like to reset the minecraft directory to %AppData%\\.minecraft?", "Reset Minecraft Directory", System.Windows.MessageBoxButton.YesNo);
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
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you would like to reset the download address to https://github.com/elliotnash/fabricMods/archive/master.zip", "Reset download address", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                downloadAddress = defaultAddress;
                storeValue("downloadAddress", downloadAddress);
            }
        }

        //runs when install mods clicked
        private WebClient webClient = null;
        private void installMods_Click(object sender, RoutedEventArgs e)
        {
            useOptifine = optifineBox.IsChecked == true;
            //if file is downloading already, clicking again will do nothing. make sure to set webClient back to
            //null once done so that button will work later.
            try
            {
                if (webClient != null)
                    return;

                installMods.IsEnabled = false;
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadFileAsync(new Uri(downloadAddress), workingDirectory + "tempDownload.zip");
            } catch (Exception ee)
            {
                MessageBoxResult messageBox = MessageBox.Show("Mod installation failed, error:"+ee, "Installation fail", MessageBoxButton.OK);
            }
        }

        private async void Completed(object sender, AsyncCompletedEventArgs e)
        {
            void unZip()
            {
                try
                {

                    //checks if tempFolder exists and deletes it to update with zip
                    if (Directory.Exists(workingDirectory + "tempFolder"))
                    {
                        DeleteDirectory(workingDirectory + "tempFolder");
                    }
                    ZipFile.ExtractToDirectory(workingDirectory + "tempDownload.zip", workingDirectory + "tempFolder");
                    string[] subdirectoryEntries = Directory.GetDirectories(workingDirectory + "tempFolder");
                    String subFolder = subdirectoryEntries[0];
                    Debug.WriteLine(subFolder);

                    /*
                    checks if it should use optifine and if so merges optifine contents with mod folder. if not deletes optifine folder
                    firsts checks to see if there even is an optifine folder
                    lotsa try catches here lol
                    */
                    if (Directory.Exists(subFolder + @"\optifine") && Directory.Exists(subFolder + @"\mods"))
                    {
                        Debug.WriteLine("Optifine folder exists");

                        //runs if useOptifine checked
                        if(useOptifine)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(subFolder+@"\optifine", subFolder+@"\mods", true /* Overwrite */);
                        } else
                        {
                            DeleteDirectory(subFolder + @"\optifine");
                        }

                    }


                    //checks if mamiyaotaru folder exists and if so moves it to temp and then back after the deletion operation
                    bool mamiyaotaru = Directory.Exists(minecraftDirectory + @"\mods\mamiyaotaru");
                    if (mamiyaotaru)
                    {

                        Directory.Move(minecraftDirectory + @"\mods\mamiyaotaru", minecraftDirectory + @"\mamiyaotaru");
                    }
                    //deletes cachedimages and mods folder
                    if (Directory.Exists(minecraftDirectory + @"\mods"))
                    {
                        DeleteDirectory(minecraftDirectory + @"\mods");
                    }
                    if (Directory.Exists(minecraftDirectory + @"\cachedImages"))
                    {
                        DeleteDirectory(minecraftDirectory + @"\cachedImages");
                    }

                    //copies new mods and chached images folder
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(subFolder, minecraftDirectory, true /* Overwrite */);

                    //copies mamiyaotaru folder back
                    if (mamiyaotaru)
                    {
                        Directory.Move(minecraftDirectory + @"\mamiyaotaru", minecraftDirectory + @"\mods\mamiyaotaru");
                    }

                    //Deletes temp folder and zip.
                    DeleteDirectory(workingDirectory + "tempFolder");
                    File.Delete(workingDirectory + "tempDownload.zip");

                }catch (Exception ee)
                {
                    MessageBoxResult messageBox = MessageBox.Show("Mod installation failed, error:" + ee, "Installation fail", MessageBoxButton.OK);
                }

            }


            var task = new Task(() => unZip());
            task.Start();
            await task;
            Debug.WriteLine("Done unzipping");
            installMods.IsEnabled = true;
            webClient = null;
        }
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private void optifineBox_Clicked(object sender, RoutedEventArgs e)
        {
            if (optifineBox.IsChecked == true)
            {
                storeValue("useOptifine", "true");
            } else
            {
                storeValue("useOptifine", "false");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (webClient != null)
            {
                MessageBoxResult messageBox = MessageBox.Show("Installation in progress, please click cancle to not fuck stuff up. if you really need to close click ok, but don't say I didn't warn you...", "Really close?", MessageBoxButton.OKCancel);
                if (messageBox == MessageBoxResult.OK)
                {
                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
            }
            else
                e.Cancel = false;
        }
    }

}





