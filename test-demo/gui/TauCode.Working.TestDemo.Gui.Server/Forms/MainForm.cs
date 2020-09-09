using System;
using System.Windows.Forms;

namespace TauCode.Working.TestDemo.Gui.Server
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void toolStripMenuItemRegisterJob_Click(object sender, System.EventArgs e)
        {
            var dialog = new RegisterJobDialog();
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                throw new NotImplementedException();
            }
        }
    }
}
