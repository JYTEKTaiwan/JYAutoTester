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


        [MATSysCommand]
        public TestItemResult MeasureVoltage(double highLimit)
        {
            Thread.Sleep(150);

            var num = Random.Shared.NextDouble();
            int bin = -1;
            TestResultType cat;
            if (num > highLimit)
            {
                bin = 1;
                cat = TestResultType.Fail;
            }
            else
            {
                bin = 0;
                cat = TestResultType.Pass;
            }

            var cmd = CommandBase.Create("AddNewTestResult", "MeasureVoltage", bin, num, "");
            LocalPeers["TestReporter"].Execute(cmd);
            Base.Recorder.Write(num);
            return TestItemResult.Create(cat, bin, num);

        }

        [MATSysCommand]
        public TestItemResult MeasureCurrent(double lowLimit)
        {
            Thread.Sleep(150);

            var num = Random.Shared.NextDouble();
            int bin = -1;
            TestResultType cat;
            if (num < lowLimit)
            {
                bin = 2;
                cat = TestResultType.Fail;
            }
            else
            {
                bin = 0;
                cat = TestResultType.Pass;
            }

            var cmd = CommandBase.Create("AddNewTestResult", "MeasureCurrent", bin, num, "");
            LocalPeers["TestReporter"].Execute(cmd);
            Base.Recorder.Write(num);
            return TestItemResult.Create(cat, bin, num);
        }

    }
}
