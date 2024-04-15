using App.Screens;
using SK.EDSDKLibWrapper;
using SK.Utils.Threads;
using Solid.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SK.CameraAccessLayer;
using log4net;
using SK.ImageProcessing.Providers;
using SK.ImageProcessingAccessLayer;
using App.Configuration;
using log4net.Config;

namespace SK.App.Fsm
{
    public enum FsmTranstionEvents
    {
        Back,
        Next,
        Exit,
        Finish,
        Error,

        /* TakePhoto Screen Specific Events */
        ToSetBackground,
        ToSetForeground,
        ToColorEfects,

        /* Settings Screen Specific Events */
        ToSettings,
        ToLiveViewChromaKeySettings,
        ToPhotoChromaKeySettings,

        /* */
        ToPrinterInfoScreen
    }

    class FsmImpl
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FsmImpl));
        private SolidMachine<FsmTranstionEvents> fsm;
        private FullContext context;
        private AppConfiguration cfg;

        public FsmImpl()
        {
            context = new FullContext();
            cfg = new AppConfiguration();

            var tm = new TaskManager(2 /* Ideally read it from configuration file*/);
            var camera = new Camera();
            var cal = new CalImpl(tm, camera);

            var imgProvider = new FileImageProvider();
            var imgProcTm = new TaskManager(1 /* Ideally read it from configuration file*/);
            var imgProcLayer = new ImageProcessingLayerImpl(imgProcTm, imgProvider);

            context.ScreensContext.Cal = cal;
            context.ScreensContext.Ial = imgProcLayer;
            context.ScreensContext.ImgProvider = imgProvider;
            context.ScreensContext.AppCfg = cfg;
            fsm = new SolidMachine<FsmTranstionEvents>(context);

            context.Fsm = fsm;
            InitTransitionTable();
        }

        public bool Initialize()
        {
            bool ret = false;
            try
            {
                // Initialize log4net configuration
                XmlConfigurator.Configure();
               
                ret = true;
                return ret;
            }
            catch (Exception ex)
            {
                log.Debug("Exception occured while initializng image provider. Full stack = {0}", ex);
                log.ErrorFormat("Exception occured while initializng image provider. Details = {0}", ex.Message);
                return false;
            }
        }

        public void Start()
        {
            fsm.Start();
        }

        public void Stop()
        {
            fsm.Stop();
            context.ScreensContext.Cal.DeInitialize();
            context.ScreensContext.Ial.DeInitialize();
            context.ScreensContext.ImgProvider.DeInitialize();
        }

        private void InitTransitionTable()
        {
            fsm.State<WelcomeScreen>()
                .On(FsmTranstionEvents.Next).GoesTo<TakePhotoScreen>()
                //.On(FsmTranstionEvents.Next).GoesTo<PrintingScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>()
                .On(FsmTranstionEvents.ToSettings).GoesTo<SettingsScreen>();
            
            fsm.State<TakePhotoScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<WelcomeScreen>()
                .On(FsmTranstionEvents.Next).GoesTo<PrintingScreen>()
                .On(FsmTranstionEvents.ToSetBackground).GoesTo<SetBackgroundScreen>()
                .On(FsmTranstionEvents.ToSetForeground).GoesTo<SetForegroundScreen>()
                .On(FsmTranstionEvents.ToColorEfects).GoesTo<SetColorEfectScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>();

            fsm.State<ErrorScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<WelcomeScreen>();

            fsm.State<SetBackgroundScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<TakePhotoScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>();

            fsm.State<SetForegroundScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<TakePhotoScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>(); 

            fsm.State<SetColorEfectScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<TakePhotoScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>();

            fsm.State<SettingsScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<WelcomeScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>()
                .On(FsmTranstionEvents.ToLiveViewChromaKeySettings).GoesTo<LiveViewSettingsScreen>();

            fsm.State<LiveViewSettingsScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>()
                .On(FsmTranstionEvents.ToPhotoChromaKeySettings).GoesTo<PhotoSettingsScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<SettingsScreen>();

            fsm.State<PhotoSettingsScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>()
                .On(FsmTranstionEvents.Back).GoesTo<SettingsScreen>();

            fsm.State<PrintingScreen>()
                .On(FsmTranstionEvents.Next).GoesTo<WelcomeScreen>()
                .On(FsmTranstionEvents.Error).GoesTo<ErrorScreen>()
                .On(FsmTranstionEvents.ToPrinterInfoScreen).GoesTo<PrinterInfoScreen>();

            fsm.State<PrinterInfoScreen>()
                .On(FsmTranstionEvents.Next).GoesTo<WelcomeScreen>();
        }
    }
}
