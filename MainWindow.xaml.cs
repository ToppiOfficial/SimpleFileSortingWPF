using SimpleFileSortingWPF.models;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows;
using System;
using System.Linq;
using System.IO;
using SimpleFileSortingWPF.interfaces;
using SimpleFileSortingWPF.settings;
using System.Collections.Generic;

namespace SimpleFileSortingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string appbuild = "1.0.1";

        public PrimaryViewModel FormatData = new PrimaryViewModel();
        private bool isSettingWindowOpen = false;
        private UserSetting settingWindowInstance = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = $"Simple File Sorting by ToppiOfficial [Version {appbuild}]";

            FormatData.Formats.LoadDirPathsFromJson();
            DataContext = FormatData;

            UserSetting checkinstsance = new UserSetting();
            checkinstsance.Close();
        }

        public w_appsetting GetSetting()
        {
            return FormatData.AppSetting;
        }

        public void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        public void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            switch(menuItem.Header)
            {
                case "_Sorting":
                    if (!isSettingWindowOpen)
                    {
                        if (settingWindowInstance == null)
                        {
                            settingWindowInstance = new UserSetting();
                            settingWindowInstance.Closed += (sender, e) =>
                            {
                                isSettingWindowOpen = false;
                                settingWindowInstance = null;
                            };
                        }

                        isSettingWindowOpen = true;
                        settingWindowInstance.Show();
                        settingWindowInstance.Activate(); // Bring the window to the front
                    }
                    else
                    {
                        settingWindowInstance.Activate(); // If window is already open, bring it to the front
                    }
                    break;
            }
        }

        public void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "btnRemovePath":
                    DirPath dirPath = (DirPath)button.DataContext;
                    FormatData.Formats.DirPaths.Remove(dirPath);
                    FormatData.Formats.SaveDirPathsToJson();
                    break;

                case "BtnAddDir":
                    var folderDialog = new OpenFileDialog
                    {
                        FileName = "Select a folder",
                        CheckFileExists = false,
                        CheckPathExists = true
                    };

                    if (folderDialog.ShowDialog() == true)
                    {
                        try
                        {
                            string selectedFolder = System.IO.Path.GetDirectoryName(folderDialog.FileName);
                            bool directoryExists = FormatData.Formats.DirPaths.Any(dirPath => dirPath.mName == selectedFolder);
                            if (IsSystemOrCoreLocation(selectedFolder))
                            {
                                MessageBox.Show("Selected directory is a system or core location. Please choose a different directory.");
                                return;
                            }

                            if (directoryExists)
                            {
                                MessageBox.Show("Directory already listed");
                                break;
                            }

                            DirPath NewPath = new DirPath
                            {
                                mName = selectedFolder
                            };
                            FormatData.Formats.DirPaths.Add(NewPath);
                            FormatData.Formats.SaveDirPathsToJson();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;

                case "BtnRemoveAllDir":
                    if(FormatData.Formats.DirPaths.Count == 0)
                    {
                        break;
                    }
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all directories?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        FormatData.Formats.DirPaths.Clear();
                        FormatData.Formats.SaveDirPathsToJson();
                    }
                    break;

                case "BtnSort":
                    if(FormatData.Formats.DirPaths.Count == 0)
                    {
                        MessageBox.Show("Add a directory first");
                        break;
                    }
                    if(FormatData.AppSetting.SelectedDateFormat == string.Empty || FormatData.Formats.mFormats.Count == 0)
                    {
                        MessageBox.Show("Please configure through the user settings first");
                        break;
                    }
                    MessageBoxResult resultsort = MessageBox.Show("Are you sure you want to sort all checked directories?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (resultsort == MessageBoxResult.Yes)
                    {
                        try
                        {
                            int datacount = FormatData.Formats.DirPaths.Count;
                            int currentdatacount = 0;
                            FormatData.Formats.LoadFormatsFromJson();
                            foreach (DirPath dir in FormatData.Formats.DirPaths)
                            {
                                if(dir.willSort == true)
                                {
                                    if(dir.SortDefaultSelection.Contains("byType"))
                                    {
                                        string[] folderpath = Directory.GetFiles(dir.mName);
                                        foreach (string file in folderpath)
                                        {
                                            string fileType = Path.GetExtension(file);
                                            foreach (c_format formats in FormatData.Formats.mFormats)
                                            {
                                                foreach (string types in formats.mTypes)
                                                {
                                                    string checktypes = $".{types}";
                                                    if (checktypes == fileType)
                                                    {
                                                        string NewFolder = Path.Combine(dir.mName, formats.mName);
                                                        Directory.CreateDirectory(NewFolder);
                                                        string NewDest = Path.Combine(NewFolder, Path.GetFileName(file));
                                                        File.Move(file, NewDest);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (dir.SortDefaultSelection.Contains("byDate"))
                                    {
                                        string[] folderpath = Directory.GetFiles(dir.mName);
                                        foreach(string file in folderpath)
                                        {
                                            DateTime creationTime = File.GetCreationTime(file);
                                            string formattedDate = creationTime.ToString(FormatData.AppSetting.SelectedDateFormat);
                                            string NewFolder = Path.Combine(dir.mName, formattedDate);
                                            string NewDest = Path.Combine(NewFolder, Path.GetFileName(file));
                                            Directory.CreateDirectory(NewFolder);
                                            File.Move(file, NewDest);
                                        }
                                    }
                                }

                                currentdatacount++;
                                ProgressBarTab.Value = (currentdatacount * 100) / datacount;
                            }

                            if (FormatData.AppSetting.DelPathOnSort == true)
                            {
                                List<DirPath> pathsToRemove = new List<DirPath>();

                                foreach (DirPath dir in FormatData.Formats.DirPaths)
                                {
                                    if (dir.willSort == true)
                                    {
                                        pathsToRemove.Add(dir);
                                    }
                                }

                                foreach (DirPath dirToRemove in pathsToRemove)
                                {
                                    FormatData.Formats.DirPaths.Remove(dirToRemove);
                                }

                                FormatData.Formats.SaveDirPathsToJson();
                            }

                            MessageBox.Show("Sort Complete");
                            ProgressBarTab.Value = 0;
                            currentdatacount = 0;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;
            }
        }

        public void Contacts_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = (MenuItem)sender;
            switch (menuitem.Header.ToString())
            {
                case "_Discord":
                    MessageBox.Show("Discord: toppiofficial, Old: Toppi#3212");
                    break;
                case "_Github":
                    System.Diagnostics.Process.Start("https://github.com/ToppiOfficial");
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FormatData.Formats.SaveDirPathsToJson();
        }

        public bool IsSystemOrCoreLocation(string path)
        {
            string[] systemPaths = new string[]
            {
                @"C:\Program Files",
                @"C:\Program Files (x86)",
                @"C:\Windows"
            };

            return systemPaths.Any(systemPath => path.StartsWith(systemPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
