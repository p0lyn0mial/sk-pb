using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Controls
{
    public class DoubleBufferedPanel : Panel, IPanel
    {
        private object synchronizer = new object();
        private Bitmap frame;


        public DoubleBufferedPanel()
        {
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint | 
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | 
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, 
                true);

            this.SetStyle(ControlStyles.Opaque, true);

            this.Location = new System.Drawing.Point(50, 50);
            this.Name = "panel1";
            this.Size = new System.Drawing.Size(950, 668);
            this.BackColor = Color.Black;
            this.Paint += this.DrawFrame;
        }

        public void Add(Bitmap frame)
        {
            lock (synchronizer)
            {
                this.frame = frame;
            }
            this.Refresh();
        }

        public void DrawFrame(object sender, PaintEventArgs e)
        {
            lock (synchronizer)
            {
                if(frame != null)
                {
                    /* GDI+ - Fast rendering settings */
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

                    e.Graphics.DrawImageUnscaled(frame, 0, 0);
                    frame.Dispose();
                    frame = null;
                }
            }
        }
    }
}
