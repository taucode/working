using System.Windows.Forms;

namespace TauCode.Working.TestDemo.Gui.Server
{
    public partial class RegisterJobDialog : Form
    {
        public RegisterJobDialog()
        {
            InitializeComponent();
        }

        public string JobName { get; private set; }

        private void buttonOk_Click(object sender, System.EventArgs e)
        {
            var jobName = textBoxJobName.Text;
            if (string.IsNullOrWhiteSpace(jobName))
            {
                MessageBox.Show("Invalid job name", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            this.JobName = jobName;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
