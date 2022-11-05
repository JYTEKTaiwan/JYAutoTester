using MATSys;
using MATSys.Commands;
using Microsoft.Extensions.Configuration;


namespace JYAutoTesterDEMO.modules
{
    internal class Handler : ModuleBase
    {
        public Handler(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {
        }


        [MATSysCommand]
        public void Moving()
        {
            Thread.Sleep(200);
        }
    }
}
