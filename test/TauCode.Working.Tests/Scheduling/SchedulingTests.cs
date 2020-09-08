using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Scheduling;
using TauCode.Working.Scheduling.Schedules;


namespace TauCode.Working.Tests.Scheduling
{
    [TestFixture]
    public class SchedulingTests
    {
        [Test]
        public async Task Todo_Test()
        {
            // Arrange
            IScheduleManager scheduleManager = new ScheduleManager();
            scheduleManager.Start();

            var now = TimeProvider.GetCurrent().TruncateMilliseconds();

            var schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);
            var worker = new MyWorker(schedule);

            // Act
            scheduleManager.Register(worker, schedule);

            await Task.Delay(2 * 1000); // todo

            // Assert


            throw new NotImplementedException();
        }

        [Test]
        public void Todo_Foo()
        {
            var baseDate = DateTime.UtcNow;

            var dataCount = 1 * 100;
            var list = new List<RegistrationMock>();
            for (int i = 0; i < dataCount; i++)
            {
                var date = baseDate.Add(TimeSpan.FromHours(i + 1));
                list.Add(new RegistrationMock
                {
                    DueTime = date,
                    SomeData = i + 1,
                });
            }

            var before = Environment.TickCount64;

            var cnt = 1000 * 1000;
            for (int i = 0; i < cnt; i++)
            {
                var min = list.Select(x => x.DueTime).Min();
            }

            var after = Environment.TickCount64;
            var ms = after - before;
            var msPerCall = (double)ms / (double)cnt;

            var k = 33;

        }
    }

    public class RegistrationMock
    {
        public DateTime DueTime { get; set; }
        public int SomeData { get; set; }
    }
}
