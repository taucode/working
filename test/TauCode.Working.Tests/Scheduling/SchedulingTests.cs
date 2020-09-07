using NUnit.Framework;
using System;
using TauCode.Working.Scheduling;
using TauCode.Working.Scheduling.Schedules;

namespace TauCode.Working.Tests.Scheduling
{
    [TestFixture]
    public class SchedulingTests
    {
        [Test]
        public void Todo_Test()
        {
            // Arrange
            IScheduleManager scheduleManager = new ScheduleManager();
            scheduleManager.Start();

            var schedule = new SimpleSchedule();
            var worker = new MyScheduleWorker(schedule);

            // Act
            scheduleManager.RegisterWorker(worker);

            // Assert
            throw new NotImplementedException();
        }
    }
}
