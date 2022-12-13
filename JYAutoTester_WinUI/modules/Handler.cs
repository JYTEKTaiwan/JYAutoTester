using MATSys;
using MATSys.Commands;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace JYAutoTesterDEMO.modules
{
    internal class Handler:ModuleBase
    {
        public Handler(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {
            
        }

        [MATSysCommand]
        public void MovingToLocation()
        {
            Thread.Sleep(50);
        }
        [MATSysCommand]
        public void MovingToHome()
        {
            Thread.Sleep(50);
        }
    }
}
