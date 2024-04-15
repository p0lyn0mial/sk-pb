using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solid.State;
using PbFsmTest.Fsm;

namespace PbFsmTest.States
{
    class StateThree : BaseState
    {
        protected override void DoEntering(Context context)
        {
            Console.WriteLine("State Three DoEntering called.");
            //context.Fsm.Trigger(PhotoBoothStateEvents.Back);
        }

        protected override void DoExiting(Context context)
        {
            Console.WriteLine("State Three DoExiting called.");
        }
    }
}
