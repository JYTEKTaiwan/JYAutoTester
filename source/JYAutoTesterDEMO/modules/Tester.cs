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
        public TestItemResult MeasureVoltage(double highLimit)
        {
            Thread.Sleep(200);

            var num = Random.Shared.NextDouble();
            int bin = -1;
            Classification cat;
            if (num > highLimit)
            {
                bin = 1;
                cat = Classification.Fail;
            }
            else
            {
                bin = 0;
                cat = Classification.Pass;
            }
            var cmd = CommandBase.Create("AddNewTestResult", "MeasureVoltage", bin, num, "");
            LocalPeers["TestReporter"].Execute(cmd);
            return new TestItemResult(cat, bin, num, null);

        }

        [MATSysCommand]
        public TestItemResult MeasureCurrent(double lowLimit)
        {
            Thread.Sleep(200);

            var num = Random.Shared.NextDouble();
            int bin = -1;
            Classification cat;
            if (num < lowLimit)
            {
                bin = 2;
                cat = Classification.Fail;
            }
            else
            {
                bin = 0;
                cat = Classification.Pass;
            }

            var cmd = CommandBase.Create("AddNewTestResult", "MeasureCurrent", bin, num, "");
            LocalPeers["TestReporter"].Execute(cmd);
            return new TestItemResult(cat, bin, num, null);
        }



    }
}
