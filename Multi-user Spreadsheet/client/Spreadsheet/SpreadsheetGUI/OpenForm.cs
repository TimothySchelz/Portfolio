using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public partial class OpenForm : Form
    {
        private SSController controller;

        public OpenForm(SSController _c)
        {
            controller = _c;
            InitializeComponent();
            this.Visible = true;
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                controller.SetOpenFileSelection(listBox1.SelectedItems[0].ToString());
                this.Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void AddToFilesListBox(HashSet<string> files)
        {
            listBox1.BeginUpdate();
            foreach(String file in files)
            {
                listBox1.Items.Add(file);
            }
            listBox1.EndUpdate();
        }
    }
}
