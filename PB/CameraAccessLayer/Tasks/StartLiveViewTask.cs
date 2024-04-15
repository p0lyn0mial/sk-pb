using log4net;
using SK.EDSDKLibWrapper;
using SK.Utils;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.CameraAccessLayer.Tasks
{
    class StartLiveViewTask : BaseTask<byte[]>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StartLiveViewTask));
        private bool pause = false;
        private bool abortFlag = false;

        private bool Pause
        {
            get
            {
                lock (synchronizer)
                {
                    return pause;
                }
            }
            set
            {
                lock (synchronizer)
                {
                    pause = value;
                }
            }
        }
        private bool AbortFlag
        {
            get
            {
                lock (synchronizer)
                {
                    return abortFlag;
                }
            }
            set
            {
                lock (synchronizer)
                {
                    abortFlag = value;
                }
            }
        }

        private void StopWork()
        {
            AbortFlag = true;
            Pause = true;
        }

        private void RunInternal()
        {
            while (!Pause)
            {
                byte[] frame = camera.GetLiveViewFrame();
                if (frame == null)
                {
                    log.Debug("Frame is null - stop live view requested ?");
                }
                else if (frame.Length > 0)
                {
                    callback(frame, ITaskResult.OK);
                }
                else
                {
                    EndWithError();
                    break;
                }
            }

            if (!AbortFlag)
            {
                log.Info("Pausing live view");
                Wait();
                log.Info("Resuming live view");
            }
        }

        protected override void EndWithError()
        {
            StopWork();
            base.EndWithError();
        }

        protected override void ShutDown()
        {
            log.Info("Shutting down work.");
            StopWork();
            base.ShutDown();
        }

        public StartLiveViewTask(ICamera camera, Action<byte[], ITaskResult> callback)
            : base(camera, callback)
        {
        }

        public override void Run()
        {
            try
            {
                /* this call blocks */
                camera.StartLiveView(this); 
                while (AbortFlag == false)
                {
                    RunInternal();
                }
                log.Info("Live View task ended successfully.");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception occured reason while retrieving live view = {0}", ex.Message);
                EndWithError();
            }
            finally
            {
                CleanUp();
            }
        }

        public override void StateChanged(CameraEvent message)
        {
            log.DebugFormat("Update called, message = ", message);
            switch (message)
            {
                case CameraEvent.LIVE_VIEW_STOP:
                    {
                        log.Debug("Stop Live View requested");
                        StopWork();
                        break;
                    }
                case CameraEvent.LIVE_VIEW_PAUSE:
                case CameraEvent.TAKE_PHOTO_START:
                    {
                        log.Debug("Pausing live view");
                        Pause = true;
                        break;
                    }
                case CameraEvent.LIVE_VIEW_RESUME:
                case CameraEvent.TAKE_PHOTO_END:
                    {
                        log.Debug("Resuming livew view");
                        Pause = false;
                        Pulse();
                        break;
                    }
                default:
                    {
                        base.StateChanged(message);
                        break;
                    }
            }
        }
    }
}
