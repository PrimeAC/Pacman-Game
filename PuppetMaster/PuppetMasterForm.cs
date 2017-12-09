using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pacman
{
    public partial class PuppetMasterWindow : Form
    {
        public PuppetMasterWindow()
        {
            InitializeComponent();
            tbChat.Text = "";
            tbChat.ReadOnly = true;
        }


        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PuppetMaster.read(tbMsg.Text);
                tbMsg.Text = "";
            }
        }

        public void changeText(string input)
        {
            tbChat.Text = input;
        }

        private void PuppetMasterWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
