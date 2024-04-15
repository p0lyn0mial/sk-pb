using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.Utils.Threads
{

    public enum ITaskResult
    {
        OK,
        ERROR,
        ERROR_WHILE_TAKING_PHOTO
    }

    public interface ITask
    {
        void Run();
        void Terminate();
    }
}
