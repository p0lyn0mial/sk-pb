using Microsoft.VisualStudio.TestTools.UnitTesting;
using SK.CameraAccessLayer;
using SK.EDSDKLibWrapper;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CameraAccessLayerUT.Tasks
{
    [TestClass]
    public class CalTasksNegativeUT
    {
        private CameraStub camera;
        private ICal target;

        [TestInitialize]
        public void TestInitialize()
        {
            var tm = new TaskManager(2);
            tm.initialize();

            camera = new CameraStub();
            target = new CalImpl(tm, camera);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            target.DeInitialize();
        }

        public void ShutDownWhileLV()
        {

        }

        /// <summary>
        /// Make sure that Live View will stop when some error will occur.
        /// </summary>
        [TestMethod, Timeout(15 * 1000) /*15 s*/]
        public void ErrorWhileLV()
        {
            var liveViewResetEvent = new AutoResetEvent(false);
            var errorOccuredEvent = new AutoResetEvent(false);
            var stopLiveViewResetEvent = new AutoResetEvent(false);

            //
            // START LIVE VIEW
            //
            camera.StartLiveViewSleepTime = 300;
            target.StartLiveView((object data, ITaskResult result) =>
            {
                //callback
                if (result == ITaskResult.OK)
                {
                    Assert.AreEqual(ITaskResult.OK, result, "Live View callback returned an error!");
                    liveViewResetEvent.Set();
                }
                Assert.AreEqual(ITaskResult.ERROR, result, "Live View callback should return an error !");
                errorOccuredEvent.Set();
            });

            Assert.IsTrue(liveViewResetEvent.WaitOne());
            camera.StartError();
            Assert.IsTrue(errorOccuredEvent.WaitOne());

            for (int i = 0; i <= 10; i++)
            {
                /*Make sure that LV is dead*/
                Assert.IsFalse(liveViewResetEvent.WaitOne(1000));
            }

            Assert.IsTrue(true);
        }
    }
}
