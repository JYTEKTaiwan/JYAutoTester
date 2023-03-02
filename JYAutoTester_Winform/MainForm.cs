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

namespace JYAutoTester_Winform
{
    public partial class MainForm : Form
    {
        DockContent form;
        public MainForm()
        {
            InitializeComponent();
            dockPanel1.Theme = new VS2015LightTheme();
        }

        private void dockPanel1_ActiveContentChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {            
            form.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.IsMdiContainer= true;
        }

        private void executeToolStripButton_Click(object sender, EventArgs e)
        {
            
            form = new Views.ExecutionForm();
            form.Show(dockPanel1, DockState.Document);

        }
    }
}
