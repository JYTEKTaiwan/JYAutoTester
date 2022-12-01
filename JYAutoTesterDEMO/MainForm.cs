using JYAutoTesterDEMO.modules;
using MATSys;
using MATSys.Hosting;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JYAutoTesterDEMO
{
    public partial class MainForm : Form
    {
        IHost host;
        IRunner runner;
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            ResetCount();
            runner.RunTestAsync((int)numericUpDown1.Value);
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            host = Host.CreateDefaultBuilder().UseMATSys().
            Build();
            runner = host.Services.GetRunner();
            var handle = host.Services.GetServices<IHostedService>().OfType<ModuleHubBackgroundService>().FirstOrDefault();
            runner.BeforeTestItemStarts += (item) =>
            {
                Invoke(new Action(() =>
                {
                    var row = dgv.Rows.Cast<DataGridViewRow>().First(x => (int)x.Cells[0].Value == item.UID);
                    row.Cells["Status"].Value = "Testing";
                    row.Selected = true;
                }));
            };
            runner.AfterTestItemStops += (item, result) =>
                {
                    Invoke(new Action(() =>
                    {
                        var row = dgv.Rows.Cast<DataGridViewRow>().First(x => (int)x.Cells[0].Value == item.UID);
                        var cell = (DataGridViewButtonCell)row.Cells["Status"];

                        var res = JsonSerializer.Deserialize<TestItemResult>(result);
                        cell.Value = res.Result;
                        textBox1.Text += result+"\r\n";
                        switch (res.Result)
                        {
                            case TestResultType.Pass:
                                cell.Style.BackColor = Color.Green;
                                numericUpDown_total.Value++;
                                numericUpDown_pass.Value++;

                                break;
                            case TestResultType.Fail:
                                cell.Style.BackColor = Color.Red;
                                numericUpDown_total.Value++;
                                numericUpDown_fail.Value++;

                                break;
                            case TestResultType.Skip:
                                cell.Style.BackColor = Color.White;
                                numericUpDown_total.Value++;                                
                                break;
                            default:
                                break;
                        }
                        row.Cells["Result"].Value = res.Value;

                    }));
                };
            host.RunAsync();


            button1.Enabled = true;
            button2.Enabled = false;
            HideArrows(numericUpDown_fail);
            HideArrows(numericUpDown_pass);
            HideArrows(numericUpDown_total);

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (var item in runner.TestScript.Setup)
            {
                var cmd = item.Executer.Value.CommandString;

                dgv.Rows.Add(
                    item.UID,
                    "Setup",
                    item.Executer.Value.ModuleName,
                    cmd.AsObject().First().Key,
                    cmd.AsObject().First().Value.ToJsonString(),
                    "Idle",
                    ""
                    );
            }
            foreach (var item in runner.TestScript.Test)
            {
                var cmd = item.Executer.Value.CommandString;

                dgv.Rows.Add(
                    item.UID,
                    "TestItem",
                    item.Executer.Value.ModuleName,
                    cmd.AsObject().First().Key,
                    cmd.AsObject().First().Value.ToJsonString(),
                    "Idle",
                    ""
                    );
            }
            foreach (var item in runner.TestScript.Teardown)
            {
                var cmd = item.Executer.Value.CommandString;

                dgv.Rows.Add(
                    item.UID,
                    "Teardown",
                    item.Executer.Value.ModuleName,
                    cmd.AsObject().First().Key,
                    cmd.AsObject().First().Value.ToJsonString(),
                    "Idle",
                    ""
                    );
            }

        }
        private void HideArrows(NumericUpDown ctrl)
        {
            ctrl.Controls[0].Visible = false;
            ctrl.BorderStyle = BorderStyle.None;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            host?.StopAsync();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            runner.StopTest();

            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void ResetCount()
        {
            numericUpDown_total.Value = 0;
            numericUpDown_pass.Value = 0;
            numericUpDown_fail.Value = 0;
        }

    }
}