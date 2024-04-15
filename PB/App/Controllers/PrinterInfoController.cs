using App.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Controllers
{
    class PrinterInfoController : BaseController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PrinterInfoController));
        private AppConfiguration cfg;

        public PrinterInfoController(AppConfiguration cfg, BaseScreen screen) : base(screen)
        {
            this.cfg = cfg;
        }

        public string GetInfoMsg()
        {
            if (InkEmpty())
            {
                log.InfoFormat("There is no ink in the printer");
                return Properties.Resources.PrinterInfoScreenNoInk;
            }
            else if (PaperEmpty())
            {
                log.InfoFormat("There is no paper in the printer");
                return Properties.Resources.PrinterInfoScreenNoPaper;
            }
            throw new Exception("Printer Info Screen should not be displaed if there is ink or paper.");
        }

        public void ResetPrinterUsage()
        {
            if (InkEmpty())
            {
                log.InfoFormat("Ink replaced in the printer, reseting counter");
                cfg.PhotoInkSetting = 0;
            }
            if (PaperEmpty())
            {
                log.InfoFormat("Paper replaced in the printer, reseting counter");
                cfg.PhotoPaperSetting = 0;
            }
        }

        private bool InkEmpty() 
        {
            return (cfg.PhotoInkSetting == AppConfiguration.PHOTO_INK_LIMIT);
        }
        private bool PaperEmpty()
        {
            return (cfg.PhotoPaperSetting == AppConfiguration.PHOTO_PAPER_LIMIT);
        }
    }
}
