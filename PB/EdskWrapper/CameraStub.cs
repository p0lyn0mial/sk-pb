using SK.EDSDKLibWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//namespace SK.EDSDKLibWrapper
//{
//    public class CameraStub : ICamera
//    {
//        protected object synchronizer = new object();
//        private List<IObserver> observers;
//        public CameraStub()
//        {
//            observers = new List<IObserver>();

//            /* Those values are RANDOM */
//            StopTakePhotoBeforeSleepTime = 500;
//            StartTakePhotoBeforeSleepTime = 300;

//            StartLiveViewSleepTime = 200;
//            StopLiveViewSleepTime = 150;

//            StartTakePhotoAfterSleepTime = 0;
//            StopTakePhotoAfterSleepTime = 0;
//        }

//        public void Attach(IObserver observer)
//        {
//            lock (synchronizer)
//            {
//                observers.Add(observer);
//            }
//        }

//        public void Detach(IObserver observer)
//        {
//            lock (synchronizer)
//            {
//                observers.Remove(observer);
//            }
//        }

//        public void Notify(CameraEvent result, IObserver sender = null)
//        {
//            lock (synchronizer)
//            {
//                foreach (var item in observers)
//                {
//                    try
//                    {
//                        item.StateChanged(result);
//                    }
//                    catch (Exception ex)
//                    {
//                        //
//                    }
//                }
//            }
//        }

//        public int StopLiveViewSleepTime { get; set; }
//        public void StopLiveView()
//        {
//            new Thread(delegate()
//                {
//                    Thread.Sleep(StopLiveViewSleepTime);
//                    Notify(CameraEvent.LIVE_VIEW_STOP);
//                }).Start();
//        }

//        public int StartLiveViewSleepTime { get; set; }
//        public void StartLiveView()
//        {
//            new Thread(delegate()
//                {
//                    Thread.Sleep(StartLiveViewSleepTime);
//                    Notify(CameraEvent.LIVE_VIEW_START);
//                }).Start();
//        }

//        public int StartTakePhotoBeforeSleepTime { get; set; }
//        public int StartTakePhotoAfterSleepTime { get; set; }
//        public void StartTakePhoto()
//        {
//            new Thread(delegate()
//            {
//                Thread.Sleep(StartTakePhotoBeforeSleepTime);
//                Notify(CameraEvent.TAKE_PHOTO_START);
//                Thread.Sleep(StartTakePhotoAfterSleepTime);
//            }).Start();
//        }

//        public int StopTakePhotoBeforeSleepTime { get; set; }
//        public int StopTakePhotoAfterSleepTime { get; set; }
//        public void StopTakePhoto()
//        {
//            new Thread(delegate()
//            {
//                Thread.Sleep(StopTakePhotoBeforeSleepTime);
//                Notify(CameraEvent.TAKE_PHOTO_END);
//                Thread.Sleep(StopTakePhotoAfterSleepTime);
//            }).Start();
//        }


//        public void ShutDown()
//        {
//            new Thread(delegate()
//            {
//                Thread.Sleep(5000);
//                Notify(CameraEvent.TAKE_PHOTO_END);
//            }).Start();
//        }

//        //public void StartError()
//        //{
//        //    new Thread(delegate()
//        //    {
//        //        Thread.Sleep(500);
//        //        Notify(CameraEvent.ERROR);
//        //    }).Start();
//        //}

//        public bool Initialize()
//        {
//            return true;
//        }

//        public bool DeInitialize()
//        {
//            return false;
//        }

//        ICameraReturnCode ICamera.Initialize()
//        {
//            throw new NotImplementedException();
//        }


//        public string CameraErrorToHumanReadable(ICameraReturnCode errorCode)
//        {
//            throw new NotImplementedException();
//        }


//        public System.Drawing.Image GetLiveViewFrame()
//        {
//            throw new NotImplementedException();
//        }


//        public void StartLiveView(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }

//        public void StopLiveView(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }


//        public void StartTakePhoto(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }

//        public void StopTakePhoto(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }


//        ICameraReturnCode ICamera.StartLiveView(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }

//        ICameraReturnCode ICamera.StopLiveView(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }

//        ICameraReturnCode ICamera.StartTakePhoto(IObserver sender)
//        {
//            throw new NotImplementedException();
//        }

//        bool ICamera.DeInitialize()
//        {
//            throw new NotImplementedException();
//        }

//        void ICamera.Attach(IObserver observer)
//        {
//            throw new NotImplementedException();
//        }

//        void ICamera.Detach(IObserver observer)
//        {
//            throw new NotImplementedException();
//        }

        
//        string ICamera.CameraErrorToHumanReadable(ICameraReturnCode errorCode)
//        {
//            throw new NotImplementedException();
//        }


//        byte[] ICamera.GetLiveViewFrame()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
