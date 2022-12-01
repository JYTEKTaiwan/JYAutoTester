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
        public double MeasureVoltage(double clamp)
        {
            Thread.Sleep(150);

            var num = Random.Shared.NextDouble()*clamp;

            Base.Recorder.Write(num);
            return num;

        }

        [MATSysCommand]
        public double MeasureCurrent(double clamp)
        {
            var num = Random.Shared.NextDouble()*clamp;

            Base.Recorder.Write(num);
            return num;
        }

    }
}
