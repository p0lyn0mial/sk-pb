using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace SK.Utils.Threads
{
    /// <summary>
    /// Generic implementation of consumer - producer queue - thread safe
    /// </summary>
    /// <typeparam name="T">type of tasks to process</typeparam>
    public abstract class ThreadQueue<T>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ThreadQueue<T>));
        private Thread[] threads;
        private readonly int _numThreads = -1;

        private class TaskInternal
        {
            public T task;

            private TaskInternal()
            { }

            public TaskInternal(T task)
            {
                this.task = task;
            }
        }

        private readonly Queue<TaskInternal> _tasks = new Queue<TaskInternal>();

        private readonly object _synchronizer = new object();
        private bool _abort_flag = false;
        private bool _initialized = false;

        /// <summary>
        /// Initializes a new instance of threaded queue with specified number of threads
        /// </summary>
        /// <param name="numThreads"></param>
        public ThreadQueue(int numThreads)
        {
            _numThreads = numThreads;
        }

        /// <summary>
        /// Intialize and start specified number of threads.
        /// </summary>
        /// <returns> true if no errors, otherwise false.</returns>
        public bool initialize()
        {
            log.Info("Initializing the queue");

            if (threads != null)
            {
                log.Error("Queue already initialized.");
                return false;
            }

            _abort_flag = false;
            threads = new Thread[_numThreads];

            for (int i = 0; i < _numThreads; i++)
            {
                log.InfoFormat("Starting Thread {0}", i);
                threads[i] = new Thread(WorkerInternal);
                threads[i].Start();
            }

            _initialized = true;
            return true;
        }

        /// <summary>
        /// Stop all threads, and deinitialize the queue.
        /// </summary>
        public void shutdown()
        {
            log.Info("Shutting down requested.");
            if (!_initialized)
            {
                return;
            }

            lock (_synchronizer)
            {
                _abort_flag = true;
                Monitor.PulseAll(_synchronizer);
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }

            threads = null;
            _initialized = false;
        }

        /// <summary>
        /// Add a task to the queue
        /// </summary>
        /// <param name="task">a task to add</param>
        /// <exception cref="System.Exception">the queue was not initialized</exception>
        protected void Add(T task)
        {
            if (!_initialized)
            {
                throw new Exception("Could not add a task to NOT initialized task queue");
            }
            lock (_synchronizer)
            {
                log.Debug("Adding a task to the queue");
                _tasks.Enqueue(new TaskInternal(task));
                Monitor.Pulse(_synchronizer);
            }
        }

        /// <summary>
        /// Process a task - an implementation should be provided by subclasses
        /// </summary>
        /// <param name="task">a task to process</param>
        protected abstract void Worker(T task);

        /// <summary>
        /// Wait for a task and process it - performed by all threads
        /// </summary>
        private void WorkerInternal()
        {
            TaskInternal taskInternal = null;
            while (_abort_flag == false)
            {
                try
                {
                    lock (_synchronizer)
                    {
                        while (_tasks.Count == 0)
                        {
                            Monitor.Wait(_synchronizer);
                            if (_abort_flag == true)
                            {
                                return;
                            }
                        }

                        taskInternal = _tasks.Dequeue();
                    }

                    if (taskInternal == null)
                    {
                        log.Error("The Task retrieved from the task queue is NULL.");
                    }
                    else
                    {
                        Worker(taskInternal.task);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception in workerInternal thread. Exception = {0}", ex.Message);
                    log.Debug(ex);
                }
            }
        }
    }
}
