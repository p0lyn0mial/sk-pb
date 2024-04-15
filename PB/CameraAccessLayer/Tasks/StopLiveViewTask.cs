using log4net;
using SK.EDSDKLibWrapper;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.CameraAccessLayer.Tasks
{
    class StopLiveViewTask : BaseTask<object>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StopLiveViewTask));

        public StopLiveViewTask(ICamera camera, Action<object, ITaskResult> callback)
            : base(camera, callback)
        {

        }

        public override void Run()
        {
            try
            {
                log.Info("Stopping Live View");
                /* this call blocks */
                camera.StopLiveView(this);

                log.Info("Stoping Livew View task ended successfully.");
                Notify(null, ITaskResult.OK);
            }
            catch (Exception ex)
            {
                log.Debug("Error while stopping live view. Stack = {0", ex);
                log.ErrorFormat("Error while stoping live view. Reson = {0}", ex.Message);
                EndWithError();
            }
            finally
            {
                CleanUp();
            }
        }
    }

}
