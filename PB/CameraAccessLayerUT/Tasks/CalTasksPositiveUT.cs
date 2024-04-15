using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SK.Utils.Threads;
using SK.EDSDKLibWrapper;
using SK.CameraAccessLayer;
using System.Threading;

namespace SK.CameraAccessLayerUT
{
    [TestClass]
    public class CalTasksPositiveUT
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

        /// <summary>
        /// While Live View take exacly three photoes
        /// </summary>
        [TestMethod, Timeout(45 * 1000) /*45 s*/]
        public void TakeThreePhotoWhileLV()
        {
            var liveViewResetEvent = new AutoResetEvent(false);
            var takePhotoResetEvent = new AutoResetEvent(false);
            var stopLiveViewResetEvent = new AutoResetEvent(false);

            //
            // START LIVE VIEW
            //
            camera.StartLiveViewSleepTime = 300;
            target.StartLiveView((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Live View callback returned an error!");
                liveViewResetEvent.Set();

            });

            //
            // FIRST PHOTO
            //
            camera.StartTakePhotoBeforeSleepTime = 2000;
            camera.StartTakePhotoAfterSleepTime = 0;
            camera.StopTakePhotoBeforeSleepTime = 0; /* During this time LV should be paused */
            camera.StopTakePhotoAfterSleepTime = 0;
            target.TakePhoto((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Take Photo callback returned an error!");
                takePhotoResetEvent.Set();
            });
            
            Assert.IsTrue(liveViewResetEvent.WaitOne());
            Assert.IsTrue(takePhotoResetEvent.WaitOne());
            for (int i = 0; i <= 10; i++ )
            {
                Thread.Sleep(1000);
                /*Make sure that LV is Resumed*/
                Assert.IsTrue(liveViewResetEvent.WaitOne());
            }

            //
            // SECOND PHOTO
            //
            target.TakePhoto((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Take Photo callback returned an error!");
                takePhotoResetEvent.Set();
            });
            Assert.IsTrue(takePhotoResetEvent.WaitOne());
            for (int i = 0; i <= 10; i++)
            {
                Thread.Sleep(1000);
                /*Make sure that LV is Resumed*/
                Assert.IsTrue(liveViewResetEvent.WaitOne());
            }

            //
            // THIRD PHOTO
            //
            target.TakePhoto((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Take Photo callback returned an error!");
                takePhotoResetEvent.Set();
            });
            Assert.IsTrue(takePhotoResetEvent.WaitOne());
            for (int i = 0; i <= 10; i++)
            {
                Thread.Sleep(1000);
                /*Make sure that LV is Resumed*/
                Assert.IsTrue(liveViewResetEvent.WaitOne());
            }

            //
            // STOP LIVE VIEW
            //
            camera.StopLiveViewSleepTime = 10;
            target.StopLiveView((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Stop Live View callback returned an error!");
                stopLiveViewResetEvent.Set();
            });

            Assert.IsTrue(stopLiveViewResetEvent.WaitOne());

            Assert.IsTrue(true);
        }

        /// <summary>
        /// This test makes sure that during Taking Photo
        /// Live View is Paused. It is rather time based check
        /// </summary>
        [TestMethod, Timeout(30 * 1000) /*30 s*/]
        public void MakeSureNoLVWhileTP()
        {
            var liveViewResetEvent = new AutoResetEvent(false);
            var takePhotoResetEvent = new AutoResetEvent(false);
            var stopLiveViewResetEvent = new AutoResetEvent(false);
            var synchronizer = new Object();
            var canStreamLivewView = true;

            //
            // START LIVE VIEW
            //
            camera.StartLiveViewSleepTime = 300;
            target.StartLiveView((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Live View callback returned an error!");
                lock (synchronizer)
                {
                    Assert.IsTrue(canStreamLivewView);
                }
                liveViewResetEvent.Set();

            });

            //
            // TAKE PHOTO
            //
            camera.StartTakePhotoBeforeSleepTime = 2000;
            camera.StartTakePhotoAfterSleepTime = 0;
            camera.StopTakePhotoBeforeSleepTime = 10000; /* During this time LV should be paused */
            camera.StopTakePhotoAfterSleepTime = 0; 
            target.TakePhoto((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Take Photo callback returned an error!");
                takePhotoResetEvent.Set();
            });

            Assert.IsTrue(liveViewResetEvent.WaitOne());
            Assert.IsFalse(takePhotoResetEvent.WaitOne(3000));
            lock (synchronizer)
            {
                canStreamLivewView = false;
            }
            Assert.IsFalse(takePhotoResetEvent.WaitOne(4000));
            lock (synchronizer)
            {
                canStreamLivewView = true;
            }
            Assert.IsTrue(takePhotoResetEvent.WaitOne());

            //
            // STOP LIVE VIEW
            //
            camera.StopLiveViewSleepTime = 10;
            target.StopLiveView((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Stop Live View callback returned an error!");
                stopLiveViewResetEvent.Set();
            });

            Assert.IsTrue(stopLiveViewResetEvent.WaitOne());

            /*Sanity check - LV Task should be dead*/
            lock (synchronizer)
            {
                canStreamLivewView = false;
            }
            /*Read about AutoResetEvent if you don't know why this will return true*/
            Assert.IsTrue(liveViewResetEvent.WaitOne(1000));
            Assert.IsFalse(liveViewResetEvent.WaitOne(1000));
            Assert.IsTrue(true);
        }

        /// <summary>
        /// This test basically makes sure that during Live View streaming
        /// we are able to take a photo in terms of task synchornization.
        /// </summary>
        [TestMethod, Timeout(20 * 1000) /*20 s*/]
        public void TakePhotoWhileLV()
        {
            var liveViewResetEvent = new AutoResetEvent(false);
            var takePhotoResetEvent = new AutoResetEvent(false);
            var stopLiveViewResetEvent = new AutoResetEvent(false);

            //
            // START LIVE VIEW
            //
            camera.StartLiveViewSleepTime = 500;
            target.StartLiveView((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Live View callback returned an error!");
                liveViewResetEvent.Set();
            });

            //
            // TAKE PHOTO
            //
            camera.StartTakePhotoBeforeSleepTime = 1100;
            camera.StopTakePhotoBeforeSleepTime = 700;
            target.TakePhoto((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Take Photo callback returned an error!");
                takePhotoResetEvent.Set();
            });

            Assert.IsTrue(liveViewResetEvent.WaitOne());
            Assert.IsTrue(takePhotoResetEvent.WaitOne());

            //
            // STOP LIVE VIEW
            //
            camera.StopLiveViewSleepTime = 10;
            target.StopLiveView((object data, ITaskResult result) =>
            {
                //callback
                Assert.AreEqual(ITaskResult.OK, result, "Stop Live View callback returned an error!");
                stopLiveViewResetEvent.Set();
            });

            Assert.IsTrue(stopLiveViewResetEvent.WaitOne());

            Assert.IsTrue(true);
        }
    }
}
