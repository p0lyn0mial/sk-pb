using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solid.State;
using PbFsmTest.Fsm;

namespace PbFsmTest.States
{
    class StateOne : BaseState
    {
        protected override void DoEntering(Context context)
        {
            Console.WriteLine("State One DoEntering called.");
            context.Fsm.Trigger(PhotoBoothStateEvents.Next);
        }

        protected override void DoExiting(Context context)
        {
            Console.WriteLine("State One DoExiting called.");
        }
    }
}
