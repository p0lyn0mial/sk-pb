using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solid.State;
using PbFsmTest.States;

namespace PbFsmTest.Fsm
{
    public enum PhotoBoothStateEvents
    {
        Back,
        Next,
        Cancel,
        Finish,
        Error
    }

    class PhotoBoothFsm
    {
        private SolidMachine<PhotoBoothStateEvents> fsm;
        private Context context;

        public PhotoBoothFsm()
        {
            context = new Context();
            fsm = new SolidMachine<PhotoBoothStateEvents>(context);

            context.Fsm = fsm;
            InitTransitionTable();
        }

        private void InitTransitionTable()
        {
            fsm.State<StateOne>()
                .On(PhotoBoothStateEvents.Next).GoesTo<StateTwo>();

            fsm.State<StateTwo>()
                .On(PhotoBoothStateEvents.Back).GoesTo<StateOne>()
                .On(PhotoBoothStateEvents.Next).GoesTo<StateThree>();

            fsm.State<StateThree>()
                .On(PhotoBoothStateEvents.Back).GoesTo<StateTwo>();
        }

        public void Start()
        {
            fsm.Start();
        }

        public void Stop()
        {
            fsm.Stop();
        }
    }
}
