using JYAutoTester_Winform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace JYAutoTester_Winform.ViewModels
{
    internal class ReportViewModel
    {
        private readonly Models.ReportModel model;
        private string currentPath;
        private IEnumerable<ReportData> data;
        public event EventHandler<DataLoadingEventArgs> DataLoading;
        public event EventHandler<IEnumerable<ReportData>> DataLoaded;
        public event EventHandler<SavingEventArgs> Saving;
        public event EventHandler SaveCompleted;
        public ReportViewModel()
        {
            model= new Models.ReportModel();
        }
        public void Load(string path)
        {
            DataLoading?.Invoke(this,new DataLoadingEventArgs(path));
            var nodes=model.Load(path);
            data = ConvertToEnumerable(nodes);
            DataLoaded?.Invoke(path, data);
            currentPath = path;


        }
        public void SaveAs(ReportType type)
        {
            var name = Path.GetFileNameWithoutExtension(currentPath) +".csv";
            switch (type)
            {
                case ReportType.CSV:
                    Saving?.Invoke(this, new SavingEventArgs(name, type));
                    model.SaveAsCSV(name,data);
                    SaveCompleted?.Invoke(this, new EventArgs());
                    break;
                default:
                    break;
            }
        }
        
        private IEnumerable<ReportData> ConvertToEnumerable (JsonNode nodes)
        {
            foreach (var node in nodes.AsArray())
            {
                var data=new ReportData();
                data.TimeStamp = node["Timestamp"].ToString();
                data.TestItem = node["ItemName"].ToString();
                data.Parameter = node["ItemParameter"].ToString();
                data.Condition = node["Condition"].ToString();
                data.ConditionParameter = node["ConditionParameter"].ToString();
                data.Result = node["Result"].ToString();
                data.Value = node["Value"].ToString();
                data.Attributes = node["Attributes"]?.ToString();
                yield return data;
            }
        }
        public enum ReportType
        {
            CSV
        }

        internal class ReportData
        {
            public string TestItem { get; set; }
            public string Parameter { get; set; }
            public string Condition { get; set; }
            public string ConditionParameter { get; set; }
            public string TimeStamp { get; set; }
            public string Result { get; set; }
            public string Value { get; set; }
            public string Attributes { get; set; }
        }
        internal class DataLoadingEventArgs:EventArgs
        {
            public string Path { get; }
            public DataLoadingEventArgs(string path)
            {
                Path= path;
            }
        }
        internal class SavingEventArgs : EventArgs
        {
            public string Path { get; }
            public ReportType Type { get; }
            public SavingEventArgs(string path, ReportType type)
            {
                Path = path;
                Type = type;
            }
        }

    }
}
