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

namespace eLOITsModSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            

            if (getValue("firstRun").Equals("true"))
            {
                String minecraftDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                Debug.WriteLine(minecraftDirectory);
                storeValue("minecraftDirectory", minecraftDirectory);
                storeValue("firstRun", "false");

                currentDirectoryTextField.Text = minecraftDirectory;
                OpenFolder(minecraftDirectory);
            }
            else
            {
                currentDirectoryTextField.Text = getValue("minecraftDirectory");
                OpenFolder(getValue("minecraftDirectory"));
            }

            
        }







        

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
        public static String getValue(String key)
        {
            try
            {
                return ConfigurationManager.AppSettings.Get(key);
            }catch(Exception e) { 
            }
            return null;
        }



















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
    }
}





