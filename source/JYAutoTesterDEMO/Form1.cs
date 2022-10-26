using JYAutoTesterDEMO.modules;
using MATSys.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JYAutoTesterDEMO
{
    public partial class Form1 : Form
    {
        IHost host;
        ModuleHubBackgroundService handle;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            handle.RunTest(5);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            host = Host.CreateDefaultBuilder().UseMATSys().
            Build();
            handle = host.Services.GetMATSysHandle();
            handle.OnReadyToExecute += (item) =>
            {
                Invoke(new Action(() => textBox1.Text += $"[{item.ModuleName}]{item.Command}...")); ;
            };
            handle.OnExecuteComplete += (item, result) =>
                {
                    Invoke(new Action(() => textBox1.Text += $"Done...{result}\r\n")); ;
                };
            host.RunAsync();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            host?.StopAsync();
        }
    }
}