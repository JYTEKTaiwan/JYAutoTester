using CsvHelper.TypeConversion;
using MATSys;
using MATSys.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JYAutoTesterDEMO.modules
{
    internal class Tester : ModuleBase
    {
        public Tester(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {
        }

        public override void Load(IConfigurationSection section)
        {
        }

        public override void Load(object configuration)
        {
        }

        [MATSysCommand]
        public double MeasureVoltage()
        {
            var num = Random.Shared.NextDouble();
            var bin = num > 0.5?TestReporter.Bin.Bin1: TestReporter.Bin.Pass;

            (LocalPeers["TestReporter"] as TestReporter).AddNewTestResult("MeasureVoltage",bin,num,"");
            return num;
        }

        [MATSysCommand]
        public double MeasureCurrent()
        {
            var num = Random.Shared.NextDouble();
            var bin = num > 0.5?TestReporter.Bin.Bin2: TestReporter.Bin.Pass;

            (LocalPeers["TestReporter"] as TestReporter).AddNewTestResult("MeasureCurrent",bin,num,"");
            return num;;
        }


    }
}
