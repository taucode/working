using System;
using System.Windows.Forms;

namespace TauCode.Working.TestDemo.Gui.Common
{
    public static class GuiExtensions
    {
        public static void ToMessageBox(this Exception exception)
        {
            MessageBox.Show(exception.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
