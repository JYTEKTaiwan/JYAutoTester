using MATSys;
using MATSys.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace JYAutoTesterDEMO.modules
{
    internal class TestReporter : ModuleBase
    {
        private uint passedCnt, skippedCnt, bin1Cnt, bin2Cnt, bin3Cnt, bin4Cnt = 0;
        private StreamWriter sw;
        private string testLogPath;
        private string summaryLogPath;
        private TestReporterConfiguration _config;
        public TestReporter(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {

        }
        public override void Load(IConfigurationSection section)
        {
            if (section.Exists())
            {
                _config = section.Get<TestReporterConfiguration>();
            }            
        }

        [MATSysCommand]
        public string AddNewTestResult(string name, Bin bin = Bin.Skip, object? value = null, string msg = "")
        {
            try
            {
                UpdateCount(bin);
                sw.WriteLine($"{name},{bin},{System.Text.Json.JsonSerializer.Serialize(value)},{msg}");
                Base.Notifier.Publish(new uint[] { passedCnt,skippedCnt,bin1Cnt + bin2Cnt + bin3Cnt + bin4Cnt});
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [MATSysCommand]
        public void StartLog()
        {
            passedCnt= skippedCnt= bin1Cnt= bin2Cnt= bin3Cnt= bin4Cnt = 0;

            var logFolder = string.IsNullOrEmpty(_config.Logfolder)?@".\log": _config.Logfolder;
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var filename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            testLogPath = Path.Combine(logFolder, $"{filename}.testlog");
            summaryLogPath = Path.Combine(logFolder, $"{filename}.summary");
            sw = new StreamWriter(testLogPath);
        }
        [MATSysCommand]
        public void StopLog()
        {
            sw.Flush();
            sw.Close();
        }


        private void UpdateCount(Bin bin)
        {
            switch (bin)
            {
                case Bin.Pass:
                    Interlocked.Increment(ref passedCnt);
                    break;
                case Bin.Skip:
                    Interlocked.Increment(ref skippedCnt);
                    break;
                case Bin.Bin1:
                    Interlocked.Increment(ref bin1Cnt);
                    break;
                case Bin.Bin2:
                    Interlocked.Increment(ref bin2Cnt);
                    break;
                case Bin.Bin3:
                    Interlocked.Increment(ref bin3Cnt);
                    break;
                case Bin.Bin4:
                    Interlocked.Increment(ref bin4Cnt);
                    break;
                default:
                    Interlocked.Increment(ref skippedCnt);
                    break;
            }

            File.WriteAllText(summaryLogPath, $"Total:{passedCnt + bin1Cnt + bin2Cnt + bin3Cnt + bin4Cnt}, Passed: {passedCnt}, Failed: {bin1Cnt + bin2Cnt + bin3Cnt + bin4Cnt}, Skipped: {skippedCnt}, Bin1: {bin1Cnt}, Bin2: {bin1Cnt}, Bin3: {bin3Cnt}, Bin4: {bin4Cnt}");
        }


        public enum Bin
        {
            Pass=0,
            Skip=-1,
            Bin1=1,
            Bin2=2,
            Bin3=3,
            Bin4=4,
        }

    }

    internal class TestReporterConfiguration
    {
        public string Logfolder { get; set; } = @".\log";
    }
    public struct ResultData
    {
        public DateTime TimeStamp { get; }
        public string Name { get; set; }
        public sbyte Bin { get; }
        public object? Value { get; }
        public string Description { get; }

        public ResultData(string name, sbyte bin = -1, object? value = null, string des = "")
        {
            Name = name;
            TimeStamp = DateTime.Now;
            Bin = bin;
            Value = value;
            Description = des;
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

}
