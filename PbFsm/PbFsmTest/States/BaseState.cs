using PbFsmTest.Fsm;
using Solid.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbFsmTest.States
{
    /// <summary>
    /// Abstract base class for states that belong to the FSM. The context is cast
    /// to the correct type to simplify for inheriting states.
    /// </summary>
    public abstract class BaseState : ISolidState
    {
        protected virtual void DoEntering(Context context) { }

        protected virtual void DoExiting(Context context) { }

        public void Entering(object context)
        {
            DoEntering(context as Context);
        }

        public void Exiting(object context)
        {
            DoExiting(context as Context);
        }
    }
}
