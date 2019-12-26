using EasyNetQ;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Working.Lab.Tests.All;

namespace TauCode.Working.Lab.Tests.Server
{
    internal class Program
    {
        private IQueueWorker<string> _worker;
        private readonly IBus _bus;
        private readonly AutoResetEvent _shutdownSignal;
        private int _assignmentNumber;

        #region Static Main

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            var program = new Program();
            program.Run();
        }

        #endregion

        public Program()
        {
            _bus = RabbitHutch.CreateBus("host=localhost");
            _bus.Respond<Command, CommandResult>(CommandResponder);
            _bus.Respond<StateRequest, StateResponse>(StateRequestResponder);
            _bus.Respond<Assignments, AssignmentsResult>(AssignmentsResponder);

            _shutdownSignal = new AutoResetEvent(false);
        }

        public void Run()
        {
            Console.WriteLine("Server is running. Waiting for shutdown signal.");
            _worker = new FooWorker()
            {
                Name = "FooWorker",
            };

            _shutdownSignal.WaitOne();

            if (_worker.State != WorkerState.Disposed)
            {
                try
                {
                    _worker.Dispose();
                }
                catch
                {
                    // dismiss
                }
            }

            Task.Delay(100).Wait(); // let the last message be delivered.

            _bus.Dispose();
        }

        private StateResponse StateRequestResponder(StateRequest stateRequest)
        {
            var stateResponse = new StateResponse
            {
                State = _worker.State,
                Backlog = _worker.Backlog,
            };

            return stateResponse;
        }

        private CommandResult CommandResponder(Command command)
        {
            try
            {
                switch (command.Verb)
                {
                    case "start":
                        _worker.Start();
                        break;

                    case "stop":
                        _worker.Stop();
                        break;

                    case "pause":
                        _worker.Pause();
                        break;

                    case "resume":
                        _worker.Resume();
                        break;

                    case "dispose":
                        _worker.Dispose();
                        break;

                    case "shutdown":
                        _shutdownSignal.Set();
                        break;

                    default:
                        throw new NotSupportedException($"Unknown command: {command.Verb}");
                }

                return new CommandResult
                {
                    IsSuccessful = true,
                };
            }
            catch (Exception ex)
            {
                return new CommandResult
                {
                    IsSuccessful = false,
                    ExceptionType = ex.GetType().FullName,
                    ExceptionMessage = ex.Message,
                };
            }
        }

        private AssignmentsResult AssignmentsResponder(Assignments assignments)
        {
            try
            {
                for (int i = 0; i < assignments.Count; i++)
                {
                    var number = Interlocked.Increment(ref _assignmentNumber);
                    _worker.Enqueue($"Assignment # {number}");
                }

                return new AssignmentsResult
                {
                    IsSuccessful = true,
                };

            }
            catch (Exception ex)
            {
                return new AssignmentsResult
                {
                    IsSuccessful = false,
                    ExceptionType = ex.GetType().FullName,
                    ExceptionMessage = ex.Message,
                };
            }
        }
    }
}
