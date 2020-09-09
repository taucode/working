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

        /// <summary>
        /// Used only for UI design while development.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IJobManager jobManager)
            : this()
        {
            _jobManager = jobManager;
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
            try
            {
                _jobManager.Register(jobName, Program.CreateJobTask, schedule, parameter);
                var jobForm = new JobForm(_jobManager, jobName);
                jobForm.MdiParent = this;
                jobForm.Show();
            }
            catch (Exception ex)
            {
                ex.ToMessageBox();
            }
        }
    }
}
