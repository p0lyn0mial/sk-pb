using log4net;
using SK.EDSDKLibWrapper;
using SK.Utils;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.CameraAccessLayer.Tasks
{
    class TakePhotoTask : BaseTask<object>
    {
         private static readonly ILog log = LogManager.GetLogger(typeof(TakePhotoTask));
         private string photoDownloadPath;
         public TakePhotoTask(ICamera camera, Action<object, ITaskResult> callback, string photoDownloadPath)
             : base(camera, callback)
         {
             this.photoDownloadPath = photoDownloadPath;
         }

         private object RunInternal()
         {
             return null;
         }

         public override void Run()
         {
             try
             {
                 /* this call blocks */
                 ICameraReturnCode status = camera.TakePhoto(this, photoDownloadPath);

                 if (status != ICameraReturnCode.SUCCESS)
                 {
                     log.ErrorFormat("Error while taking photo, camera error = {0}", status);
                     EndWithError(MapCameraResultToTaskResult(status));
                     return;
                 }
                
                 log.Debug("Downloading photo from camera");
                 var data = RunInternal();
                 Notify(data, ITaskResult.OK);

                 log.Info("Take Photo task ended successfully.");
             }
             catch (Exception ex)
             {
                 log.Debug("Error while stopping live view. Stack = {0", ex);
                 log.ErrorFormat("Error while stoping live view. Reson = {0}", ex.Message);
                 EndWithError(ITaskResult.ERROR_WHILE_TAKING_PHOTO);
             }
             finally
             {
                 CleanUp();
             }
         }
    }
}
