using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SK.Utils.Threads
{
    public interface ITaskManager
    {
        bool Process(ITask task);
    }
}
