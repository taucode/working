using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TauCode.Extensions;

namespace TauCode.Working.TestDemo.Gui.Common
{
    public class TextBoxWriter : TextWriter
    {
        private readonly StringWriterWithEncoding _writer;
        private readonly object _lock;
        private TextBox _textBox;

        public TextBoxWriter()
        {
            _writer = new StringWriterWithEncoding(Encoding.UTF8);
            _lock = new object();
        }

        public TextBox TextBox
        {
            get
            {
                lock (_lock)
                {
                    return _textBox;
                }
            }
            set
            {
                lock (_lock)
                {
                    _textBox = value ?? throw new ArgumentNullException();
                    _textBox.Text = _writer.ToString();
                }
            }
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            lock (_lock)
            {
                _writer.Write(value);
                _textBox?.InvokeIfRequired(() => _textBox?.AppendText(value.ToString()));
            }
        }

        public override void Write(string s)
        {
            lock (_lock)
            {
                _writer.Write(s);
                _textBox?.InvokeIfRequired(() => _textBox?.AppendText(s));
            }
        }
    }
}
