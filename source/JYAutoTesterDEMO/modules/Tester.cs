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
        public double MeasureVoltage(double highLimit)
        {
            var num = Random.Shared.NextDouble();
            var bin = num > highLimit ? TestReporter.Bin.Bin1: TestReporter.Bin.Pass;

            var cmd = CommandBase.Create("AddNewTestResult", "MeasureVoltage", bin, num, "");
            LocalPeers["TestReporter"].Execute(cmd);
            return num;
        }

        [MATSysCommand]
        public double MeasureCurrent(double lowLimit)
        {

            var num = Random.Shared.NextDouble();
            var bin = num < lowLimit ? TestReporter.Bin.Bin2: TestReporter.Bin.Pass;
            var cmd = CommandBase.Create("AddNewTestResult", "MeasureCurrent", bin, num, "");
            LocalPeers["TestReporter"].Execute(cmd);
            return num;;
        }



    }
}
