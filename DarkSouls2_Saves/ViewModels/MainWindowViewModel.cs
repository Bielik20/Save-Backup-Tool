using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GlobalHotkeys;
using System.Windows;
using System.IO;
using DarkSouls2_Saves.Models;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using SimpleSerialization;
using System.Windows.Media;
using SimpleNotifications;

namespace DarkSouls2_Saves.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Commands
        public ICommand AddCommand { get; private set; }
        public ICommand HelpCommand { get; private set; }
        #endregion

        #region Const
        private const string serializationFile = "archive.xml";

        private const string message = "Application was created by Damian Bielecki in 2016.\n" +
            "Remember that while it's freeware and I do not allow commercial use!" + "\n" +
            "\n-------------------\n\n" +
            "Application is capable of backing up files using hotkeys. For this to work it DOES NOT need to be active window." + "\n" +
            "\n" +
            "Backing up follows this patters:" + "\n" +
            "- lets say file is in\t\t ...\\File_Folder" + "\n" +
            "- then Quick Save is in\t ...\\File_Folder\\_QuickSave" + "\n" +
            "- and Slots are in\t\t ...\\File_Folder\\_SlotSaves\\NUM" + "\n" +
            "\n" +
            "Additional informations:" + "\n" +
            "- try double click on file path to open containing folder." + "\n" +
            "- if loading or saving fails you will get notification and paths will be colored to indicate error.";

        private SolidColorBrush correctColor = new SolidColorBrush(Color.FromRgb(176, 196, 222));
        private SolidColorBrush errorColor = new SolidColorBrush(Color.FromRgb(199, 83, 83));

        private Key[] keyArray = { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9 };
        #endregion

        #region Properties and Fields
        public string InfoMessage
        {
            get { return _infoMessage; }
            set { _infoMessage = value; OnPropertyChanged("InfoMessage"); }
        }
        private string _infoMessage = "Here last action is displayed.";

        public ObservableCollection<FileTracker> FileList { get; set; } = new ObservableCollection<FileTracker>();

        private NotificationManager fadingNotification;
        #endregion

        #region Init
        public MainWindowViewModel()
        {
            var SaveLocation = @"C:\Users\Bielik\AppData\Roaming\DarkSoulsII\01100001057ac699";

            fadingNotification = new NotificationManager();
            fadingNotification.TextBlock.FontSize = 15;
            fadingNotification.WindowBorder.Cursor = Cursors.None;

            Deserialize();

            AddCommand = new RelayCommand(_ => AddFile());
            HelpCommand = new RelayCommand(_ => MessageBox.Show(message, "Help!"));

            _hotKey = new Hotkey(Key.F1, KeyModifier.None, OnHotKeyHandler); //Legacy thing.
            quickSaveHotkey = new Hotkey(Key.F5, KeyModifier.None, QuickSaveHotkeyHandler);
            quickLoadHotkey = new Hotkey(Key.F9, KeyModifier.None, QuickLoadHotkeyHandler);
            for (int i = 0; i < keyArray.Count(); i++)
            {
                slotsSaveHotkey[i] = new Hotkey(keyArray[i], KeyModifier.Ctrl | KeyModifier.Shift, SlotSaveHotkeyHandler);
                slotsLoadHotkey[i] = new Hotkey(keyArray[i], KeyModifier.Alt | KeyModifier.Shift, SlotLoadHotkeyHandler);
            }
        }
        #endregion

        #region Methods
        private void SetFileDelegates(FileTracker file)
        {
            file.FileDeleted += (sender, e) => DeleteFile(sender, e);
            file.FileBrowsed += (sender, e) => EditFile(sender, e);
        }

        private void DeleteFile(object sender, EventArgs e)
        {
            FileList.Remove(sender as FileTracker);
            Serialize();
        }

        private void AddFile()
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.*)|*.*";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = false;

            if (choofdlog.ShowDialog() == true)
            {
                var filePath = choofdlog.FileName;
                if (!FileList.Any(x => x.FilePath == filePath))
                {
                    var file = new FileTracker(choofdlog.FileName);
                    FileList.Add(file);
                    SetFileDelegates(file);

                    Serialize();
                }
            }
        }

        private void EditFile(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.*)|*.*";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = false;

            if (choofdlog.ShowDialog() == true)
            {
                var filePath = choofdlog.FileName;
                if (!FileList.Any(x => x.FilePath == filePath))
                {
                    (sender as FileTracker).UpdateFileTracker(filePath);
                    Serialize();
                }
            }
        }

        private void Serialize()
        {
            var temp = new List<string>();
            foreach (var item in FileList)
            {
                temp.Add(item.FilePath);
            }
            MySerialization.Serialize(serializationFile, temp);
        }

        private void Deserialize()
        {
            var temp = new List<string>();
            MySerialization.Deserialize(serializationFile, ref temp);
            foreach (var item in temp)
            {
                var file = new FileTracker(item);
                FileList.Add(file);
                SetFileDelegates(file);
            }
        }
        #endregion

        #region Hotkeys
        private void OnHotKeyHandler(Hotkey hotkey)
        {
            var a = new SimpleNotifications.NotificationManager();
            a.Show("Dark Souls!!!");
            InfoMessage = "Dark Souls!!!";
        }
        Hotkey _hotKey;

        private void QuickSaveHotkeyHandler(Hotkey hotkey)
        {
            //File.Copy(newPath, newPath.Replace(@SaveLocation, @SaveLocation + @"\_arch"), true);  //Nice way of navigating
            var flag = true;

            foreach (var file in FileList)
            {
                Directory.CreateDirectory(file.FolderLocation + "\\_QuickSave");
                try
                {
                    File.Copy(file.FilePath, file.FolderLocation + "\\_QuickSave\\" + file.FileName, true);
                    file.BackgroundColor = correctColor;
                }
                catch
                {
                    flag = false;
                    file.BackgroundColor = errorColor;
                }
            }

            if (flag)
            {
                InfoMessage = String.Format("Quick Save at - {0}", DateTime.Now.ToLongTimeString());
            }
            else
            {
                InfoMessage = String.Format("Failed to Quick Save at - {0}", DateTime.Now.ToLongTimeString());
            }
            fadingNotification.Show(InfoMessage);
        }
        Hotkey quickSaveHotkey;

        private void QuickLoadHotkeyHandler(Hotkey hotkey)
        {
            var flag = true;

            foreach (var file in FileList)
            {
                try
                {
                    File.Copy(file.FolderLocation + "\\_QuickSave\\" + file.FileName, file.FilePath , true);
                    file.BackgroundColor = correctColor;
                }
                catch
                {
                    flag = false;
                    file.BackgroundColor = errorColor;
                }
            }
            
            if (flag)
            {
                InfoMessage = String.Format("Quick Load at - {0}", DateTime.Now.ToLongTimeString());
            }
            else
            {
                InfoMessage = String.Format("Failed to Quick Load at - {0}", DateTime.Now.ToLongTimeString());
            }
            fadingNotification.Show(InfoMessage);
        }
        Hotkey quickLoadHotkey;

        private void SlotSaveHotkeyHandler(Hotkey hotkey)
        {
            var flag = true;

            foreach (var file in FileList)
            {
                Directory.CreateDirectory(file.FolderLocation + "\\_SlotSaves\\" + hotkey.Key.ToString());
                try
                {
                    File.Copy(file.FilePath, file.FolderLocation + "\\_SlotSaves\\" + hotkey.Key.ToString() + "\\" + file.FileName, true);
                    file.BackgroundColor = correctColor;
                }
                catch
                {
                    flag = false;
                    file.BackgroundColor = errorColor;
                }
            }

            if (flag)
            {
                InfoMessage = String.Format("Saved {0} at - {1}", hotkey.Key.ToString(), DateTime.Now.ToLongTimeString());
            }
            else
            {
                InfoMessage = String.Format("Failed to save {0} at - {1}", hotkey.Key.ToString(), DateTime.Now.ToLongTimeString());
            }
            fadingNotification.Show(InfoMessage);
        }
        Hotkey[] slotsSaveHotkey = new Hotkey[10];

        private void SlotLoadHotkeyHandler(Hotkey hotkey)
        {
            var flag = true;

            foreach (var file in FileList)
            {
                try
                {
                    File.Copy(file.FolderLocation + "\\_SlotSaves\\" + hotkey.Key.ToString() + "\\" + file.FileName, file.FilePath, true);
                    file.BackgroundColor = correctColor;
                }
                catch
                {
                    flag = false;
                    file.BackgroundColor = errorColor;
                }
            }

            if (flag)
            {
                InfoMessage = String.Format("Loaded {0} at - {1}", hotkey.Key.ToString(), DateTime.Now.ToLongTimeString());
            }
            else
            {
                InfoMessage = String.Format("Failed to save {0} at - {1}", hotkey.Key.ToString(), DateTime.Now.ToLongTimeString());
            }
            fadingNotification.Show(InfoMessage);
        }
        Hotkey[] slotsLoadHotkey = new Hotkey[10];
        #endregion
    }
}
