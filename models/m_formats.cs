using SimpleFileSortingWPF.models;
using SimpleFileSortingWPF.settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SimpleFileSortingWPF.models
{

    public class _INotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PrimaryViewModel : _INotifyPropertyChanged
    {
        public m_formats Formats { get; } = new m_formats();
        public w_appsetting AppSetting { get; } = new w_appsetting();
    }

    public class m_formats : _INotifyPropertyChanged
    {
        private ObservableCollection<DirPath> _DirPaths = new ObservableCollection<DirPath>();
        public ObservableCollection<DirPath> DirPaths
        {
            get { return _DirPaths; }
            set { _DirPaths = value; OnPropertyChanged(nameof(mFormats)); }
        }

        private ObservableCollection<c_format> _mFormats = new ObservableCollection<c_format>();
        public ObservableCollection<c_format> mFormats
        {
            get { return _mFormats; }
            set { _mFormats = value; OnPropertyChanged(nameof(mFormats)); }
        }

        public void SaveDirPathsToJson()
        {
            string json = JsonSerializer.Serialize(DirPaths, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appdata.json", json);
        }

        public void LoadDirPathsFromJson()
        {
            if (File.Exists("appdata.json"))
            {
                string json = File.ReadAllText("appdata.json");
                DirPaths = JsonSerializer.Deserialize<ObservableCollection<DirPath>>(json);
            }
        }

        public void SaveFormatsToJson()
        {
            string json = JsonSerializer.Serialize(mFormats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("userdata.json", json);
        }

        public void LoadFormatsFromJson()
        {
            if (File.Exists("userdata.json"))
            {
                string json = File.ReadAllText("userdata.json");
                mFormats = JsonSerializer.Deserialize<ObservableCollection<c_format>>(json);
            }
        }
    }
    public class c_format : _INotifyPropertyChanged
    {
        private string _mName;
        public string mName 
        {
            get { return _mName; }
            set { _mName = value; OnPropertyChanged(nameof(mName)); }
        }

        private ObservableCollection<string> _mTypes = new ObservableCollection<string>();
        public ObservableCollection<string> mTypes
        {
            get { return _mTypes; }
            set { _mTypes = value; OnPropertyChanged(nameof(mTypes)); }
        }
    }
    public class DirPath : _INotifyPropertyChanged
    {
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        private string _mName;
        public string mName
        {
            get { return _mName; }
            set { _mName = value; OnPropertyChanged(nameof(mName)); }
        }

        private ObservableCollection<string> _mSortFilter = new ObservableCollection<string>
        {
            "byType", "byDate"
        };

        public ObservableCollection<string> mSortFilter
        {
            get { return _mSortFilter; }
            set { _mSortFilter = value; OnPropertyChanged(nameof(mSortFilter)); }
        }

        private string _SortDefaultSelection;
        public string SortDefaultSelection
        {
            get { return _SortDefaultSelection; }
            set { _SortDefaultSelection = value; OnPropertyChanged(nameof(SortDefaultSelection)); }
        }

        public DirPath()
        {
            SortDefaultSelection = _mSortFilter[0];
            willSort = mainWindow.FormatData.AppSetting.IncludePathOnAdd;
        }

        private bool _willSort;
        public bool willSort
        {
            get { return _willSort; }
            set { _willSort = value; OnPropertyChanged(nameof(willSort));}
        }
    }
}
