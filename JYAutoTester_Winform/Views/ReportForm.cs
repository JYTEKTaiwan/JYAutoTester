using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
namespace JYAutoTester_Winform.Views
{
    public partial class ReportForm : DockContent
    {
        private readonly ViewModels.ReportViewModel viewModel;

        public ReportForm()
        {
            InitializeComponent();
            TopLevel = false;
            Dock = DockStyle.Fill;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows= false;
            viewModel = new ViewModels.ReportViewModel();
            viewModel.DataLoaded += ViewModel_DataLoaded;
        }

        private void ViewModel_DataLoaded(object sender, IEnumerable<ViewModels.ReportViewModel.ReportData> e)
        {
            dataGridView1.DataSource = (e.ToList());
            filaName_StripStatusLabel.Text = Path.GetFullPath((string)sender);
        }

        private void ReportViewer_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

        }

        private void localPath_toolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewModel.Load(@".\report.txt");
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewModel.SaveAs(ViewModels.ReportViewModel.ReportType.CSV);
        }

        private void ReportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void ReportForm_Shown(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
