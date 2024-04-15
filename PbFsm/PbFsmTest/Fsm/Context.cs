using Solid.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbFsmTest.Fsm
{
    public class Context
    {
        /// <summary>
        /// A reference to the state machine so the states have access to it.
        /// </summary>
        public SolidMachine<PhotoBoothStateEvents> Fsm { get; set; }
    }
}
