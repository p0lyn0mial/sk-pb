using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoBoothTestApp
{
    abstract class ShapesClass
    {
        abstract public int Area();
    }

    class Square : ShapesClass
    {
        int side = 0;

        public Square(int n)
        {
            side = n;
        }
        // Area method is required to avoid 
        // a compile-time error. 
        public override int Area()
        {
            return side * side;
        }

        
    }

    public partial class Form1 : Form
    {
        int liczba = 4;
        string h = "dupa";
        

        private PrintDocument printDoc;

        public Form1()
        {

                        InitializeComponent();


            ///
            //ShapesClass sq = new Square(12);
            //var ret = sq.Area();
            ///

            printDoc = new PrintDocument();
            
            printDoc.PrinterSettings.PrinterName = "Canon SELPHY CP910";
            //printDoc.PrinterSettings.PrinterName = "PDFCreator";
            PaperSize size = new PaperSize("JapanesePostcard", 4, 6);
            printDoc.DefaultPageSettings.Landscape = false;

            printDoc.PrintPage += printDoc_PrintPage;
            printDoc.BeginPrint += printDoc_BeginPrint;
            printDoc.EndPrint += printDoc_EndPrint;

            //printDoc.DefaultPageSettings.PaperSize = new PaperSize("Custom", someWidth, someHeight);

        }

        private void printCtr_EndPrint(PrintDocument doc, PrintEventArgs e)
        {
            Console.WriteLine("sdfdf");
        }

        private void printDoc_BeginPrint(object sender, PrintEventArgs e)
        {
            Console.WriteLine("Beging");
        }

        private void printDoc_EndPrint(object sender, PrintEventArgs e)
        {
            Console.WriteLine("End");
        }

        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var bmp = Bitmap.FromFile(@"c:\Users\Lukasz\Pictures\RemotePhoto\22_7_2014_18_08_37\0\strip.jpg");
            Point ulCorner = new Point(0, 0);
            //e.Graphics.DrawImage(bmp, 0, 0, 394, 583);
            //e.Graphics.DrawImage(bmp, e.MarginBounds);

            e.Graphics.DrawImage(bmp, e.PageBounds.X, e.PageBounds.Y, e.PageBounds.Width, e.PageBounds.Height);
            
            //InsertText(“OK”);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            dlg.Document = printDoc;
            dlg.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            printDoc.Print();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog dlg = new PrintPreviewDialog();
            dlg.Document = printDoc;
            dlg.ShowDialog();
        }
    }
}

