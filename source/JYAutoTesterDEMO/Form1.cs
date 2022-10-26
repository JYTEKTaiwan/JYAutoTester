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
            handle.RunTest((int)numericUpDown1.Value);
            button1.Enabled = false;
            button2.Enabled = true;
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
            (handle.Modules["TestReporter"] as TestReporter).OnSummaryUpdated += (pass, fail) => 
            {
                Invoke(new Action(() => 
                {
                    numericUpDown_pass.Value = pass;
                    numericUpDown_fail.Value = fail;
                    numericUpDown_total.Value = pass+fail;

                }));
            };
            host.RunAsync();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            host?.StopAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            handle.StopTest();
            button1.Enabled = true;
            button2.Enabled = false;
        }

    }
}