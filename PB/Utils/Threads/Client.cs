using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SK.Utils.Threads
{
    class HavyTask : ITask
    {
        public void Run()
        {
            Console.WriteLine("Working hard . . .");
        }

        public void Terminate()
        {
            Console.WriteLine("Terminating the task.");
        }
    }

    class Client
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Client));

        public void TaskManagerDemo()
        {
            const int THREAD_NUMBER = 1;
            TaskManager tm = null;
            try
            {
                tm = new TaskManager(THREAD_NUMBER);
                if (!tm.initialize())
                {
                    log.Error("Error while initializing the task manager");
                    return;
                }

                tm.Process(new HavyTask());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing the task, reason = {0}", ex.Message);
                log.Debug(ex);
            }
            finally
            {
                if (tm != null)
                {
                    tm.shutdown();
                }
            }
        }
    }
}
