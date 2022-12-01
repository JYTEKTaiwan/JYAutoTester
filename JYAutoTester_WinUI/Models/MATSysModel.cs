using MATSys.Hosting;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYAutoTester.Models
{
    internal class MATSysModel
    {
        private readonly IHost _host;

        public MATSysModel() 
        {
            _host = Host.CreateDefaultBuilder().UseMATSys().Build();
            _host.RunAsync();
        }
        public IRunner GetRunner()
        {
            return _host.Services.GetRunner();
        }
        ~MATSysModel() 
        {
            _host.StopAsync();
        }

    }
}
