using JYAutoTesterDEMO.modules;
using MATSys;
using MATSys.Hosting;
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
        ModuleHubBackgroundService handle;
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            handle.RunTest((int)numericUpDown1.Value);
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            host = Host.CreateDefaultBuilder().UseMATSys().
            Build();

            handle = host.Services.GetMATSysHandle();
            handle.BeforeTestItem += (item) =>
            {
                Invoke(new Action(() =>
                {
                    var row = dgv.Rows.Cast<DataGridViewRow>().First(x => (int)x.Cells[0].Value == item.ID);
                    row.Cells["Status"].Value = "Testing";
                    row.Selected = true;
                }));
            };
            handle.AfterTestItem += (item, result) =>
                {
                    Invoke(new Action(() =>
                    {
                        var row = dgv.Rows.Cast<DataGridViewRow>().First(x => (int)x.Cells[0].Value == item.ID);
                        var cell = (DataGridViewButtonCell)row.Cells["Status"];

                        if (string.IsNullOrEmpty(result))
                        {
                            cell.Value = "";
                            cell.Style.BackColor = Color.White;
                            row.Cells["Result"].Value = "";
                        }
                        else
                        {
                            var res = JsonSerializer.Deserialize<TestItemResult>(result);
                            cell.Value = res.Result;

                            switch (res.Result)
                            {
                                case TestResultType.Pass:
                                    cell.Style.BackColor = Color.Green;
                                    break;
                                case TestResultType.Fail:
                                    cell.Style.BackColor = Color.Red;
                                    break;
                                case TestResultType.Skip:
                                    cell.Style.BackColor = Color.White;
                                    break;
                                default:
                                    break;
                            }

                            row.Cells["Result"].Value = res.Value;

                        }
                    }));
                };
            (handle.Modules["TestReporter"]).OnDataReady += (e) =>
                {
                    var param = JsonSerializer.Deserialize<uint[]>(e);
                    Invoke(new Action(() =>
                        {
                            numericUpDown_pass.Value = param[0];
                            numericUpDown_fail.Value = param[2];
                            numericUpDown_total.Value = param[0] + param[2];

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
            foreach (var item in handle.SetupItems)
            {
                var cmd=JsonDocument.Parse(item.Command).RootElement.EnumerateObject().First();
                dgv.Rows.Add(
                    item.ID,
                    "Setup",
                    item.ModuleName,
                    cmd.Name,
                    cmd.Value,
                    "Idle",
                    ""
                    );
            }
            foreach (var item in handle.TestItems)
            {
                var cmd = JsonDocument.Parse(item.Command).RootElement.EnumerateObject().First();

                dgv.Rows.Add(
                    item.ID,
                    "TestItem",
                    item.ModuleName,
                    cmd.Name,
                    cmd.Value,
                    "Idle",
                    ""
                    );
            }
            foreach (var item in handle.TeardownItems)
            {
                var cmd = JsonDocument.Parse(item.Command).RootElement.EnumerateObject().First();

                dgv.Rows.Add(
                    item.ID,
                    "Teardown",
                    item.ModuleName,
                    cmd.Name,
                    cmd.Value,
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
            handle.StopTest();
            button1.Enabled = true;
            button2.Enabled = false;
        }

    }
}