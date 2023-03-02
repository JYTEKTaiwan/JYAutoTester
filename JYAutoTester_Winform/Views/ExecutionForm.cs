using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
namespace JYAutoTester_Winform.Views
{
    public partial class ExecutionForm : DockContent
    {
        private readonly ViewModels.ExecutionViewModel viewModel;
        public ExecutionForm()
        {
            InitializeComponent();
            TopLevel = false;
            Dock = DockStyle.Fill;
            viewModel = new ViewModels.ExecutionViewModel();
            viewModel.ListContentChanged += ViewModel_ListContentChanged;
            viewModel.CurrentItemChanged += ViewModel_CurrentItemChanged;
            viewModel.ScriptExecutionDone += ViewModel_ScriptExecutionDone;
            bs = new BindingSource();
            bs.DataSource = viewModel.TestSequence;
            dataGridView1.DataSource = bs;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            bindingNavigator1.BindingSource = bs;
        }

        private void ViewModel_ScriptExecutionDone(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
            {
                toolStripButton_export.Enabled = true;

            }));
        }

        private void ViewModel_CurrentItemChanged(object sender, ViewModels.CurrentItemChangedEventArg e)
        {
            Invoke(new Action(() =>
            {
                bs.Position = e.Index + 1;
                dataGridView1.InvalidateRow(e.Index);
            }));
        }

        private void ViewModel_ListContentChanged(object sender, EventArgs e)
        {
            dataGridView1.InvalidateRow((int)sender);
        }

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {            
            toolStripButton_export.Enabled = false;
            viewModel.StartTest();
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            viewModel.StopTest();
        }

        private void ExecutionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                switch (e.Value.ToString())
                {
                    case "Pass":
                        e.CellStyle.BackColor = Color.Green;
                        break;
                    case "Fail":
                        e.CellStyle.BackColor = Color.Red;
                        break;
                    default:
                        break;
                }

            }
        }

        private void toolStripButton_export_Click(object sender, EventArgs e)
        {
            viewModel.Export();
        }
    }
}
