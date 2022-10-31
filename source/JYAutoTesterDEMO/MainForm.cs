using JYAutoTesterDEMO.modules;
using MATSys;
using MATSys.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Linq;

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
            handle.OnReadyToExecute += (item) =>
            {
                Invoke(new Action(() =>
                {
                    var row = dgv.Rows.Cast<DataGridViewRow>().First(x => (int)x.Cells[0].Value == item.ID);
                    row.Cells["Status"].Value = "Testing";
                    row.Selected = true;
                }));
            };
            handle.OnExecuteComplete += (item, result) =>
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
                            var res = JsonConvert.DeserializeObject<TestItemResult>(result);
                            cell.Value = res.Result;

                            switch (res.Result)
                            {
                                case Classification.Pass:
                                    cell.Style.BackColor = Color.Green;
                                    break;
                                case Classification.Fail:
                                    cell.Style.BackColor = Color.Red;
                                    break;
                                case Classification.Skip:
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
                    var param = JsonConvert.DeserializeObject<uint[]>(e);
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
                dgv.Rows.Add(
                    item.ID,
                    "Setup",
                    item.ModuleName,
                    item.Command.Split("=")[0],
                    item.Command.Split('=')[1],
                    "Idle",
                    ""
                    );
            }
            foreach (var item in handle.TestItems)
            {
                dgv.Rows.Add(
                    item.ID,
                    "TestItem",
                    item.ModuleName,
                    item.Command.Split("=")[0],
                    item.Command.Split('=')[1],
                    "Idle",
                    ""
                    );
            }
            foreach (var item in handle.TeardownItems)
            {
                dgv.Rows.Add(
                    item.ID,
                    "Teardown",
                    item.ModuleName,
                    item.Command.Split("=")[0],
                    item.Command.Split('=')[1],
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