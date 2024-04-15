using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Task;

namespace TaskManager.Mediator
{
    class MediatorImpl : IMediator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MediatorImpl));

        public MediatorImpl()
        {
            clients = new List<ITask>();
        }

        public bool Register(ITask client)
        {
            if (!clients.Contains(client))
            {
                clients.Add(client);
                return true;
            }
            return false;
        }

        public bool DeRegister(ITask client)
        {
            if (clients.Contains(client))
            {
                return clients.Remove(client);
            }
            return false;
        }

        public void Notify(ITask sender, NotificationType message)
        {
            foreach (ITask client in clients)
            {
                if (client != sender)
                {
                    client.Recieve(message);
                }
            }
        }

        private List<ITask> clients;
    }
}
