using SK.App.Fsm;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Config;

namespace App.Main
{
    public class AppContext : ApplicationContext
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppContext));
        private static FsmImpl fsm;
        private const string APP_VERSION = "1.0.1";
        public bool Initialize()
        {
            try
            {
                log.InfoFormat("Starting photo booth application. Version = {0}. Made by Stara Kaszarnia", APP_VERSION);
                fsm = new FsmImpl();
                if (fsm.Initialize())
                {
                    fsm.Start();
                }
                else
                {
                    log.Error("Error while initializing FSM");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Debug("Exception occured while starting the application. Full stack = {0}", ex);
                log.ErrorFormat("Exception occured while starting the application. Details = {0}", ex.Message);

                fsm.Stop();
                ExitThread();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Event handler for form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnFormClosed(object sender, EventArgs e)
        {
            log.Debug("OnFromColosed called. Stopping fsm");
            fsm.Stop();

            log.Debug("FSM stopped successfully, exiting application thread.");
            Application.ExitThread();
        }
    }
}
