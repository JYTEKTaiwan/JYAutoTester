using MATSys.Hosting;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JYAutoTester_Winform.Models
{
    internal class AutoTestModel
    {
        private readonly IHost _host;
        public List<TestItem> TestItems { get; set; }=new List<TestItem>();
        public AutoTestModel()
        {
            _host = Host.CreateDefaultBuilder().UseMATSys().Build();
            var runner = _host.Services.GetRunner();
            TestItems.AddRange(runner.TestScript.Setup);
            TestItems.AddRange(runner.TestScript.Test);
            TestItems.AddRange(runner.TestScript.Teardown);
            _host.RunAsync().Wait(500);
        }


        public IRunner GetRunner()
        {
            return _host.Services.GetRunner();
        }

        public void Start()
        {
            GetRunner().RunTestAsync();
        }
        public void Stop()
        {
            GetRunner().StopTest();

        }

        ~AutoTestModel()
        {
            _host.StopAsync();
        }


    }

}
