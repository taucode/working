using System;
using System.Windows.Forms;
using TauCode.Extensions.Lab;
using TauCode.Working.Jobs;
using TauCode.Working.Jobs.Schedules;
using TauCode.Working.TestDemo.Gui.Common;
using TauCode.Working.TestDemo.Gui.Server.Dialogs;

namespace TauCode.Working.TestDemo.Gui.Server.Forms
{
    public partial class MainForm : Form
    {
        private readonly IJobManager _jobManager;
        private readonly TextBoxWriter _textBoxWriter;

        /// <summary>
        /// Used only for UI design while development.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IJobManager jobManager, TextBoxWriter textBoxWriter)
            : this()
        {
            _jobManager = jobManager;
            _textBoxWriter = textBoxWriter;
        }

        private void toolStripMenuItemRegisterJob_Click(object sender, System.EventArgs e)
        {
            var dialog = new RegisterJobDialog();
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var schedule = new SimpleSchedule(SimpleScheduleKind.Minute, 1, "2020-01-01".ToExactUtcDate()); // todo temp
                var parameter = 1488; // todo temp
                this.DoRegisterJob(dialog.JobName, schedule, parameter);
            }
        }

        private void DoRegisterJob(string jobName, ISchedule schedule, object parameter)
        {
            throw new NotImplementedException();
            //try
            //{
            //    var demoRunner = new DemoRunner();
            //    _jobManager.Register(jobName, demoRunner.Run, schedule, parameter);
            //    var jobForm = new JobForm(_jobManager, jobName, demoRunner);
            //    jobForm.MdiParent = this;
            //    jobForm.Show();
            //}
            //catch (Exception ex)
            //{
            //    ex.ToMessageBox();
            //}
        }

        private void toolStripMenuItemLog_Click(object sender, EventArgs e)
        {
            foreach (var form in MdiChildren)
            {
                if (form is LogForm)
                {
                    form.Show();
                    return;
                }
            }

            var logForm = new LogForm(_textBoxWriter);
            logForm.MdiParent = this;
            logForm.Show();
        }
    }
}
