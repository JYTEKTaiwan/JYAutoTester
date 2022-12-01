using CommunityToolkit.WinUI.UI.Controls;
using JYAutoTester.Models;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.PointOfService.Provider;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using static NetMQ.NetMQSelector;

namespace JYAutoTester.ViewModels
{
    public class MATSysViewModel : INotifyPropertyChanged
    {

        private MATSysModel model;

        private ObservableCollection<TestItemData> _binding = new ObservableCollection<TestItemData>();
        private int _pass = 0;
        private int _fail = 0;
        private string _logText = "";
        private TestItemData _currentItem = null;

        public event PropertyChangedEventHandler PropertyChanged;
        public Microsoft.UI.Dispatching.DispatcherQueue TheDispatcher { get; set; } = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        public int PassCount
        {
            get { return _pass; }
            set
            {
                _pass = value;
                OnPropertyChanged();
            }
        }
        public int FailCount
        {
            get { return _fail; }
            set
            {
                _fail = value;
                OnPropertyChanged();
            }
        }
        public string LogText
        {
            get { return _logText; }
            set
            {
                _logText = value;
                OnPropertyChanged();
            }
        }
        public TestItemData CurrentItem
        {
            get { return _currentItem; }
            set 
            {
                _currentItem = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<TestItemData> TestItems
        {
            get { return _binding; }
            set
            {
                _binding = value;
                OnPropertyChanged();
            }
        }
        public MATSysViewModel()
        {
            model = new MATSysModel();
            _binding.Clear();

            var runner = model.GetRunner();

            TestItemParsing(runner.TestScript.Setup, "Setup", "st");
            TestItemParsing(runner.TestScript.Test, "Test", "te");
            TestItemParsing(runner.TestScript.Teardown, "Teardown", "td");

            OnPropertyChanged("TestItems");


            runner.AfterTestItemStops += (_, res) =>
            {
                var result = JsonSerializer.Deserialize<TestItemResult>(res);
                switch (result.Result)
                {
                    case TestResultType.Pass:
                        PassCount++;
                        break;
                    case TestResultType.Fail:
                        FailCount++;
                        break;
                    case TestResultType.Skip:
                        break;
                    default:
                        break;
                }
            };
            runner.AfterSubTestItemComplete += (item, result) =>
            {
                TheDispatcher.TryEnqueue(() =>
                {
                    var rowItem = _binding.FirstOrDefault(x => x.UID == item.UID);
                    var res = JsonSerializer.Deserialize<TestItemResult>(result);
                    rowItem.Value = res.Value;
                    rowItem.Result = res.Result.ToString();
                    rowItem.Attributes = res.Attributes;
                    CurrentItem = rowItem;
                    LogText += result.ToJsonString() + Environment.NewLine;
                });
            };

        }

        public void StartTest(int iteration = 1)
        {
            PassCount = 0;
            FailCount = 0;
            LogText = "";

            var runner = model.GetRunner();
            runner.RunTestAsync(iteration);

        }
        public void StopTest()
        {
            var runner = model.GetRunner();
            runner.StopTest();
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            TheDispatcher.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            });

        }
        private void TestItemParsing(IEnumerable<TestItem> tests, string category,string prefix)
        {
            int idx=1;
            foreach (var item in tests)
            {
                _binding.Add(new TestItemData()
                {
                    UID = item.UID,
                    TestItemID = $"{prefix}_{idx:D4}",
                    Name = item.Executer.Value.CommandString.AsObject().First().Key,
                    ExecuterCommand = item.GetExecuter().ToJsonString(),
                    AnalyzerCommand = item.GetAnalyzer().ToJsonString(),
                    Category = category,
                });
                idx++;
            }

        }
    }
    public class TestItemData : INotifyPropertyChanged
    {
        private int _uid = -1;
        private string _cat = "";
        private string _itemID = "";
        private string _name = "";
        private string _result = "";
        private object _value = null;
        private object _attributes = null;
        private string _desp = null;
        private string _execCmd = "";
        private string _anaCmd = "";
        private Brush _resultBrush = new SolidColorBrush(Colors.Transparent);
        public int UID
        {
            get { return _uid; }
            set
            {
                _uid = value;
                OnPropertyChanged();
            }
        }

        public string TestItemID
        {
            get { return _itemID; }
            set 
            {
                _itemID = value;
                OnPropertyChanged();
            }
        }
        public string Category
        {
            get { return _cat; }
            set
            {
                _cat = value;
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
                UpdateBrush(_result);
            }
        }
        public Brush ResultBrush
        {
            get
            {
                return _resultBrush;
            }
            set
            {
                _resultBrush = value;
                OnPropertyChanged();
            }
        }
        
        public string ExecuterCommand
        {
            get { return _execCmd; }
            set
            {
                _execCmd = value;
                OnPropertyChanged();
            }
        }
        public string AnalyzerCommand
        {
            get { return _anaCmd; }
            set
            {
                _anaCmd = value;
                OnPropertyChanged();
            }
        }
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public object Attributes
        {
            get { return _attributes; }
            set
            {
                _attributes = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _desp; }
            set
            {
                _desp = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private void UpdateBrush(string result)
        {
            switch (result)
            {
                case "Pass":
                    ResultBrush = new SolidColorBrush(Colors.Green);
                    break;
                case "Fail":
                    ResultBrush = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    ResultBrush = new SolidColorBrush(Colors.Black);
                    break;
            }            
        }
    }


}
