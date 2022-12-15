using MATSys.Hosting;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.Hosting;

namespace JYAutoTester.Models
{
    internal class MATSysModel
    {
        private readonly IHost _host;

        public MATSysModel()
        {
            _host = Host.CreateDefaultBuilder().UseMATSys().Build();
        }
        public IRunner GetRunner()
        {
            return _host.Services.GetRunner();
        }

        public void Start()
        {
            _host.RunAsync();

        }
        public void Stop()
        {
            _host.StopAsync();

        }
        ~MATSysModel()
        {
        }

    }
}
