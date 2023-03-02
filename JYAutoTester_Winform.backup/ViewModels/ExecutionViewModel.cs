using JYAutoTester_Winform.Models;
using MATSys.Hosting.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JYAutoTester_Winform.ViewModels
{
    internal class ExecutionViewModel
    {
        private JsonArray result;
        private readonly AutoTestModel model;
        private List<TestItem> testItems = new List<TestItem>();
        public List<TestItemToDisplay> TestSequence { get; set; } = new List<TestItemToDisplay>();
        public event EventHandler<CurrentItemChangedEventArg> CurrentItemChanged;

        public event EventHandler ListContentChanged;

        public event EventHandler ScriptExecutionDone;
        public ExecutionViewModel() 
        {
            model = new AutoTestModel();
            model.GetRunner().BeforeTestItemStarts += ExecutionViewModel_BeforeTestItemStarts;
            model.GetRunner().AfterSubTestItemComplete += ExecutionViewModel_AfterSubTestItemComplete;
            model.GetRunner().AfterScriptStops += ExecutionViewModel_AfterScriptStops;
            testItems.AddRange(model.TestItems);
            foreach (var item in testItems)
            {
                var seq = new TestItemToDisplay();

                seq.Name = item.Executer.Value.CommandString.AsObject().First().Key;
                var param=item.Executer.Value.CommandString.AsObject().First().Value as JsonArray;
                for (int i = 0; i < param.Count; i++)
                {
                    seq.Parameter += param[i].ToString();
                    if (i!=param.Count-1)
                    {
                        seq.Parameter += ";";
                    }
                }
                if (item.Analyzer!=null)
                {
                    seq.Logic = item.Analyzer.Name;
                    var str = "";
                    for (int i = 0; i < item.AnalyzerParameter.Length; i++)
                    {
                        if (i>0)
                        {
                            str += item.AnalyzerParameter[i].ToString();
                            if (i!=item.AnalyzerParameter.Length-1)
                            {
                                str += ";";
                            }
                        }
                    }
                    seq.Condition = str;
                }

                TestSequence.Add(seq);;;
            }
        }

        private void ExecutionViewModel_AfterScriptStops(JsonArray item)
        {
            result = item;
            ScriptExecutionDone.Invoke(null, null);
        }

        private void ExecutionViewModel_AfterSubTestItemComplete(MATSys.Hosting.Scripting.TestItem item, System.Text.Json.Nodes.JsonNode result)
        {
            var idx = testItems.FindIndex(x => x.UID == item.UID);
            TestSequence[idx].Result = result["Result"].ToString();
            TestSequence[idx].Value = result["Value"];
            TestSequence[idx].Attributes = result["Attributes"];
            ListContentChanged.Invoke(idx, null);
        }

        private void ExecutionViewModel_BeforeTestItemStarts(MATSys.Hosting.Scripting.TestItem item)
        {
            var idx = testItems.FindIndex(x => x.UID == item.UID);

            CurrentItemChanged.Invoke(null,new CurrentItemChangedEventArg(idx));
        }

        public void StartTest()
        {
            model.Start();
        }

        public void StopTest()
        {
            model.Stop();
        }
        public void Export()
        {
            File.WriteAllText(@".\report.txt", result.ToJsonString(new JsonSerializerOptions() { WriteIndented=true}));
        }

    }

    internal class CurrentItemChangedEventArg:EventArgs
    {
        public int Index { get; }
        public CurrentItemChangedEventArg(int idx)
        {
            Index = idx;
        }
    }


    public class TestItemToDisplay
    {
        private string _name = "";
        private string _result = "";
        private object _value = "";
        private object _attributes = "";
        private string _execCmd = "";
        private string _anaCmd = "";
        //private Brush _resultBrush = new SolidColorBrush(Colors.Transparent);

        
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public string Parameter { get; set; }

        public string Logic { get; set; } = "";
        public string Condition { get; set; } = "";

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
                //UpdateBrush(_result);
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



        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        //private void UpdateBrush(string result)
        //{
        //    switch (result)
        //    {
        //        case "Pass":
        //            ResultBrush = new SolidColorBrush(Colors.Green);
        //            break;
        //        case "Fail":
        //            ResultBrush = new SolidColorBrush(Colors.Red);
        //            break;
        //        default:
        //            ResultBrush = new SolidColorBrush(Colors.Transparent);
        //            break;
        //    }
        //}
    }

}
