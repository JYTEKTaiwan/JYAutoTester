using MATSys;
using MATSys.Commands;
using System;
using System.Threading;

namespace JYAutoTesterDEMO.modules
{
    internal class Tester : ModuleBase
    {
        int delay = 20;
        private int cnt = 0;
        public Tester(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {
        }

        [MATSysCommand]
        public int CounterValue()
        {
            Thread.Sleep(delay);

            return cnt++;
        }

        [MATSysCommand]
        public double MeasureVoltage(double clamp)
        {
            Thread.Sleep(delay);

            var num = Random.Shared.NextDouble() * clamp;

            Base.Recorder.Write(num);
            return num;

        }

        [MATSysCommand]
        public double MeasureCurrent(double clamp)
        {
            Thread.Sleep(delay);

            var num = Random.Shared.NextDouble() * clamp;

            Base.Recorder.Write(num);
            return num;
        }

        [MATSysCommand]
        public double PowerInBand()
        {
            Thread.Sleep(delay);

            var num = Random.Shared.NextDouble();

            Base.Recorder.Write(num);
            return num;
        }

        [MATSysCommand]
        public double SignalToNoiseRatio()
        {
            Thread.Sleep(50);

            var num = Random.Shared.NextDouble() * 10.0 - 100;

            Base.Recorder.Write(num);
            return num;
        }

        [MATSysCommand]
        public double Temperature()
        {
            Thread.Sleep(delay);

            var num = Random.Shared.NextDouble() * 20 + 10;

            Base.Recorder.Write(num);
            return num;
        }

        [MATSysCommand]
        public void SOT()
        {
            Thread.Sleep(delay);

        }

        [MATSysCommand]
        public void EOT()
        {
            Thread.Sleep(delay);
            cnt = 0;

        }
    }
}
