using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Task;

namespace TaskManager.Mediator
{
    public interface IMediator
    {
        bool Register(ITask client);
        bool DeRegister(ITask client);
        void Notify(ITask sender, NotificationType message);
    }
}
