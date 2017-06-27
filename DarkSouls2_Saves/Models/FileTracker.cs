using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Runtime.Serialization;
using System.Windows.Media;


namespace DarkSouls2_Saves.Models
{
    class FileTracker : ViewModels.ViewModelBase
    {
        #region Commands
        public ICommand BrowseCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand OpenDirectoryCommand { get; set; }
        #endregion

        #region Properties
        public string FilePath
        {
            get { return _filePath; }
            private set { _filePath = value; OnPropertyChanged("FilePath"); }
        }
        private string _filePath = "";

        public string FolderLocation { get { return Path.GetDirectoryName(FilePath); } }
        public string FileName { get { return Path.GetFileName(FilePath); } }

        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; OnPropertyChanged("BackgroundColor"); }
        }
        private SolidColorBrush _backgroundColor;
        #endregion

        #region Delegates
        public event EventHandler FileBrowsed;
        public event EventHandler FileDeleted;
        #endregion

        #region Init
        public FileTracker()
        {
            BackgroundColor = new SolidColorBrush(Color.FromRgb(176, 196, 222));

            BrowseCommand = new RelayCommand(_ => BrowseFile());
            DeleteCommand = new RelayCommand(_ => DeleteFile());
            OpenDirectoryCommand = new RelayCommand(_ => Process.Start(FolderLocation));
        }

        public FileTracker(string filePath) : this()
        {
            UpdateFileTracker(filePath);
        }
        #endregion

        #region Methods
        public void UpdateFileTracker(string filePath)
        {
            FilePath = filePath;
        }

        private void BrowseFile()
        {
            if(FileBrowsed != null)
                FileBrowsed(this, null);
        }

        private void DeleteFile()
        {
            if (FileDeleted != null)
                FileDeleted(this, EventArgs.Empty);
        }
        #endregion
    }
}
