using MATSys;
using MATSys.Commands;
using JYUSB101;
using System;
using System.Threading;

namespace TestLib
{
    public class USB101Test : ModuleBase
    {
        public USB101Test(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {
        }
        [MATSysCommand]
        public double AI_Float()
        {
            double data = 0;
            var ai = new JYUSB101AITask(0);
            ai.AddChannel(1);
            ai.Mode = AIMode.Single;
            ai.Start();
            ai.ReadSinglePoint(ref data);
            ai.Stop();
            return data;
        }
        [MATSysCommand]
        public double AO_DCOutput(double value)
        {
            var ai = new JYUSB101AITask(0);
            ai.AddChannel(0);
            ai.Mode = AIMode.Single;
            ai.Start();
            var ao = new JYUSB101AOTask(0);
            ao.AddChannel(0);
            ao.Mode = AOMode.Single;
            ao.Start();
            ao.WriteSinglePoint(value);

            double data = 0;
            ai.ReadSinglePoint(ref data);
            ai.Stop();
            ao.WriteSinglePoint(0);
            ao.Stop();
            return data;
        }

        [MATSysCommand]
        public double AI_IdleNoise()
        {
            try
            {
                var ai = new JYUSB101AITask(0);
                ai.AddChannel(0);
                ai.Mode = AIMode.Finite;
                ai.SampleRate = 10000;
                ai.SamplesToAcquire = 1000;
                ai.Start();
                ai.Stop();

                double[] data = new double[1000];
                ai.Start();
                ai.ReadData(ref data,1000,-1);
                ai.Stop();
                double square = 0;
                double mean = 0;
                double root = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    square += Math.Pow(data[i], 2);
                }

                // Calculate Mean
                mean = (square / (double)(data.Length));

                // Calculate Root
                root = Math.Sqrt(mean);
                Thread.Sleep(1000);
                return root;

            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        [MATSysCommand]
        public bool DIO_Check(bool ch0, bool ch2)
        {
            var di = new JYUSB101DITask(0);
            di.AddChannel(new int[]{ 0,2});
            
            var dotask = new JYUSB101DOTask(0);
            dotask.AddChannel(new int[] { 1, 3 });            
            dotask.WriteSinglePoint(new bool[] { ch0, ch2 });
            bool[] data = new bool[2];
            di.ReadSinglePoint(ref data);

            return data[0]==ch0&&data[1]==ch2;
        }

    }
}