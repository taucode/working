using System.Windows.Forms;
using TauCode.Working.TestDemo.Gui.Common;

namespace TauCode.Working.TestDemo.Gui.Server.Forms
{
    public partial class LogForm : Form
    {
        private readonly TextBoxWriter _textBoxWriter;

        public LogForm()
        {
            InitializeComponent();
        }

        public LogForm(TextBoxWriter textBoxWriter)
            : this()
        {
            _textBoxWriter = textBoxWriter;
        }

        private void LogForm_Load(object sender, System.EventArgs e)
        {
            _textBoxWriter.TextBox = textBoxLog;
        }
    }
}
