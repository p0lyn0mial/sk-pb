using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solid.State;
using PbFsmTest.Fsm;

namespace PbFsmTest.States
{
    class StateTwo : BaseState
    {
        protected override void DoEntering(Context context)
        {
            Console.WriteLine("State Two DoEntering called.");
            context.Fsm.Trigger(PhotoBoothStateEvents.Next);
        }

        protected override void DoExiting(Context context)
        {
            Console.WriteLine("State Two DoExiting called.");
        }
    }
}
