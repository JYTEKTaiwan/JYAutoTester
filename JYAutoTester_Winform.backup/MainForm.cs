using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JYAutoTester_Winform
{
    public partial class MainForm : Form
    {
        Form form;
        public MainForm()
        {
            InitializeComponent();
           
        }

        private void dockPanel1_ActiveContentChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            form = new Views.ExecutionForm();

            panel1.Controls.Add(form);
            form.Show();
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.Close();
        }
    }
}
