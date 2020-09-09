using System;
using System.Windows.Forms;

namespace TauCode.Working.TestDemo.Gui.Server
{
    public class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());

            var program = new Program();
            program.Run();
        }

        public Program()
        {   
        }

        public MainForm MainForm { get; private set; }

        private void Run()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            this.MainForm = new MainForm();
            Application.Run(this.MainForm);
        }
    }
}
