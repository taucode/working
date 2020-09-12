using System;
using System.ComponentModel;
using System.Windows.Forms;
using TauCode.Working.Jobs;
using TauCode.Working.TestDemo.Gui.Common;

namespace TauCode.Working.TestDemo.Gui.Server.Forms
{
    public partial class JobForm : Form
    {
        private readonly IJobManager _jobManager;
        private readonly string _jobName;
        private readonly DemoRunner _demoRunner;

        public JobForm()
        {
            InitializeComponent();
        }

        public JobForm(IJobManager jobManager, string jobName, DemoRunner demoRunner)
            : this()
        {
            _jobManager = jobManager;
            _jobName = jobName;
            _demoRunner = demoRunner;
        }

        private void JobForm_Load(object sender, System.EventArgs e)
        {
            this.Text = _jobName;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.RemoveJob();
            base.OnClosing(e);
        }

        private void RemoveJob()
        {
            throw new NotImplementedException();
            //try
            //{
            //    _jobManager.Remove(_jobName);
            //}
            //catch (Exception ex)
            //{
            //    ex.ToMessageBox();
            //}
        }
    }
}
