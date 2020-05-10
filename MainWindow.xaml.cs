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


namespace eLOITsModSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Initializes variables from config so they're public
        public static String minecraftDirectory;
        public static String downloadAddressString;

        public MainWindow()
        {
            InitializeComponent();



            //Gets the default minecraft directory if first run, else gets the value from config.
            if (getValue("firstRun").Equals("true"))
            {
                minecraftDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                Debug.WriteLine(minecraftDirectory);
                storeValue("minecraftDirectory", minecraftDirectory);
                storeValue("firstRun", "false");
            }
            else
            {
                minecraftDirectory = getValue("minecraftDirectory");
            }
            
            //Don't pull values from config once running, only save them
            //Sets current dirctory text block to current minecraft directory
            currentDirectoryTextField.Text = minecraftDirectory;

            downloadAddressString = getValue("downloadAddress");


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
            try
            {
                return ConfigurationManager.AppSettings.Get(key);
            }catch(Exception e) { 
            }
            return null;
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
            inputDialog inputDialog = new inputDialog("Please enter a link to a direct download of a .zip file containing only folder with a minecraft directory. all files inside that folder will be copied to inside the .minecraft folder", downloadAddressString);
            if (inputDialog.ShowDialog() == true)
                downloadAddressString = inputDialog.Answer;

        }
    }
}





