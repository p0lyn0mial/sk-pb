using App.Configuration;
using App.Screens;
using SK.CameraAccessLayer;
using SK.ImageProcessing;
using SK.ImageProcessing.Providers;
using SK.ImageProcessingAccessLayer;
using Solid.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.App.Fsm
{

    public class ImageEfectContext
    {
        public int SelectedImageId { get; set; }
        public bool ApplyEfect { get; set; }
        public ImgEfectType EffectType { get; set; }
    }

    /// <summary>
    /// Data buffer - utilized by the screens
    /// </summary>
    public class Context
    {
        public Context()
        {
            ImgEfectCtx = new ImageEfectContext();
        }

        public ICal Cal { get; set; }
        public IIal Ial { get; set; }
        public IImageProvider ImgProvider { get; set; }
        public AppConfiguration AppCfg; 
        public string ErrorMessage { get; set; }
        public uint SessionId { get; set; }
        
        /* path to the directory from which taken photo can be downloaded - currently eligible for print screen*/
        public string DownloadPhotoRootDir { get; set; }

        /* Image Efect Screen Ctx specific only */
        public ImageEfectContext ImgEfectCtx { get; set; }
    }

    public class FullContext
    {
        public FullContext()
        {
            ScreensContext = new Context();
        }

        /// <summary>
        /// A reference to the state machine so the Base Screen has access to it.
        /// </summary>
        public SolidMachine<FsmTranstionEvents> Fsm { get; set; }

        public Context ScreensContext { get; set; }
    }
}
