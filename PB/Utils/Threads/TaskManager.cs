using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace SK.Utils.Threads
{
    /// <summary>
    /// THE Task manager - gives you ability to add and process tasks.
    /// </summary>
    public class TaskManager : ThreadQueue<ITask>, ITaskManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TaskManager));

        /// <summary>
        /// Initializes a new instance of task manager with specified number of threads.
        /// </summary>
        /// <param name="numThreads">number of threads</param>
        public TaskManager(int numThreads)
            : base(numThreads)
        {
            /* an empty ctor*/
        }

        /// <summary>
        /// Process a task taken from the queue by a thread
        /// </summary>
        /// <param name="task">task to process</param>
        protected override void Worker(ITask task)
        {
            try
            {
                task.Run();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception when running task. Reason = {0}", ex.Message);
                log.Debug(ex);

                task.Terminate();
            }
        }

        /// <summary>
        /// Add a task to the queue.
        /// </summary>
        /// <param name="task">task to add</param>
        /// <returns>ture if no error, an exception otherwise</returns>
        /// <exception cref="System.Exception">queue was not initialized</exception>
        public bool Process(ITask task)
        {
            Add(task);
            return true;
        }
    }
}
