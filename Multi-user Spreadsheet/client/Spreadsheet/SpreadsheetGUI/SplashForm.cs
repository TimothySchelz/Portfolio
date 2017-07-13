using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public partial class SplashForm : Form
    {
        private string username, address;
        private SpreadSheetForm view;
        public SplashForm(SpreadSheetForm _view)
        {
            InitializeComponent();
            view = _view;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (usernameBox.TextLength == 0 || addressBox.TextLength == 0)
            {
                MessageBox.Show(this, "Please enter a username and server address.");
                return;
            }
            if (usernameBox.TextLength > 25)
            {
                MessageBox.Show(this, "Username must be under 25 characters long.");
                return;
            }
            username = usernameBox.Text;
            address = addressBox.Text;
            view.StartConnection(username, address);
            this.Close();
        }
    }
}
