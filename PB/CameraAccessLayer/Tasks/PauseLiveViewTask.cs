using log4net;
using SK.CameraAccessLayer.Tasks;
using SK.EDSDKLibWrapper;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.CameraAccessLayer.Tasks
{
    class PauseLiveViewTask : BaseTask<object>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PauseLiveViewTask));

        public PauseLiveViewTask(ICamera camera, Action<object, ITaskResult> callback)
            : base(camera, callback)
        {
    
        }

        public override void Run()
        {
            try
            {
                log.Info("Pausing Live View");

                /* this call blocks */
                bool ret = camera.PauseLiveView(this);

                log.InfoFormat("Pause Livew View task ended with status = {0}.", ret);

                if (ret)
                {
                    Notify(null, ITaskResult.OK);
                }
                else
                {
                    EndWithError();
                }
            }
            catch (Exception ex)
            {
                log.Debug("Error while pausing live view. Stack = {0", ex);
                log.ErrorFormat("Error while pausing live view. Reson = {0}", ex.Message);
                EndWithError();
            }
            finally
            {
                CleanUp();
            }
        }
    }
}
