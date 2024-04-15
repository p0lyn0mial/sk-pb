using log4net;
using SK.EDSDKLibWrapper;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.CameraAccessLayer.Tasks
{
    class ResumeLiveViewTask : BaseTask<object>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ResumeLiveViewTask));
        public ResumeLiveViewTask(ICamera camera, Action<object, ITaskResult> callback)
            : base(camera, callback)
        {
    
        }

        public override void Run()
        {
            try
            {
                log.Info("Resume Live View");

                /* this call blocks */
                bool ret = camera.ResumeLiveView(this);

                log.InfoFormat("Resume Livew View task ended with status = {0}.", ret);

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
                log.Debug("Error while resuming live view. Stack = {0", ex);
                log.ErrorFormat("Error while resuming live view. Reson = {0}", ex.Message);
                EndWithError();
            }
            finally
            {
                CleanUp();
            }
        }
    }
}
