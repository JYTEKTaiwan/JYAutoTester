using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace JYAutoTester.ViewModels
{
    public class EditorViewModel : ObservableObject
    {
        private string _filePath = "";
        private string _fileName = "";
        private string _readContent = "";
        private string _content = "";
        private bool _isFileOpened = false;
        private StorageFile _file;
        public ObservableCollection<string> ActiveModules { get; set; }
        public bool IsFileOpened
        {
            get => _isFileOpened;
            set => SetProperty(ref _isFileOpened, value);
        }
        public EditorViewModel()
        {
            var jObj = JsonObject.Parse(File.ReadAllText("appsettings.json"));
            var arr = jObj["MATSys"]["Modules"].AsArray();
            ActiveModules = new ObservableCollection<string>(arr.Select(x => x["Alias"].ToString()));

        }
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string OpenFile()
        {
            // Open a text file.
            FileOpenPicker open = new FileOpenPicker()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            open.FileTypeFilter.Add(".ats");


            var window = (Application.Current as App)?.Window as MainWindow;

            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(open, hwnd);

            var t = open.PickSingleFileAsync().AsTask();
            t.Wait();
            _file = t.Result;


            if (_file != null)
            {
                FileName = _file.Name;
                FilePath = _file.Path;
                _readContent = File.ReadAllText(_file.Path);
                IsFileOpened = true;
                return _readContent;
            }
            else
            {
                return null;
            }


        }

        public string NewFile()
        {
            FileName = "NewDocument";
            FilePath = "";
            _readContent = "[\r\n]";
            IsFileOpened = true;
            return _readContent;
        }

        public void SaveFile(string content)
        {
            if (_file == null)
            {
                // Open a text file.
                FileSavePicker save = new FileSavePicker()
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = "NewDocument",

                };
                save.FileTypeChoices.Add("ats file", new string[] { ".ats" });


                var window = (Application.Current as App)?.Window as MainWindow;

                // Get the current window's HWND by passing in the Window object
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                // Associate the HWND with the file picker
                WinRT.Interop.InitializeWithWindow.Initialize(save, hwnd);

                var t = save.PickSaveFileAsync().AsTask();
                t.Wait();
                _file = t.Result;
                if (_file != null)
                {
                    File.WriteAllText(_file.Path, content);
                    _file = null;
                }

            }
            else
            {
                File.WriteAllText(_file.Path, content);
                _file = null;
            }
            IsFileOpened = false;

        }
    }
}
