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

        public override void Load(IConfigurationSection section)
        {
            
        }

        public override void Load(object configuration)
        {
            
        }

        [MATSysCommand]
        public void Moving()
        {
            Thread.Sleep(100);
        }
    }
}
