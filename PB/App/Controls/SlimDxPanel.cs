using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using SlimDX;
using SlimDX.Direct2D;
using System.Drawing.Imaging;
using SK.ImageProcessing;
using System.Runtime.InteropServices;

namespace App.Controls
{
    public class SlimDxPanel : IPanel
    {
        private RendererDirect2D renderer;
        System.Drawing.Bitmap frame;
        BitmapData frameData;

        public SlimDxPanel(Panel panel)
        {
            renderer = new RendererDirect2D();
            renderer.Initialize(panel);
        }

        //public void Add(byte[] frame)
        //{

        //    //renderer.StoreCurrentFrame(frame,1056, 704, 4242);
        //    //renderer.Render();

        //    //Make sure to clean up resources
        //    var bitmap = new System.Drawing.Bitmap(1056, 704, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    var data = bitmap.LockBits(new Rectangle(0, 0, 1056, 704), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    int len = 1056 * 704 * 3;
        //    Marshal.Copy(frame, 0, data.Scan0, frame.Length);
            

        //    renderer.StoreCurrentFrame(data.Width, data.Height, data.Stride, data.Scan0);
        //    renderer.Render();
        //    bitmap.UnlockBits(data);
        //}

        public void Add(System.Drawing.Bitmap frame)
        {
            this.frame = frame;
            
            //
                frameData = frame.LockBits(
                        new Rectangle(0, 0, frame.Width, frame.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb//bmp.PixelFormat
                        );
            
                renderer.StoreCurrentFrame(frameData.Width, frameData.Height, frameData.Stride, frameData.Scan0);
                renderer.Render();
                frame.UnlockBits(frameData);
        }

        public SlimDX.Direct2D.Bitmap CreateBitmap(System.Drawing.Bitmap drawingBitmap)
        {
            return renderer.LoadBitmap(drawingBitmap);
        }

        public void Add(SlimDX.Direct2D.Bitmap bmp)
        {
            renderer.StoreCurrentFrame(bmp);
            renderer.Render();
        }
       
    }

    internal class RendererDirect2D
    {
        #region Properties
        public Control Surface
        {
            get { return m_RenderingSurface; }
        }
        #endregion

        #region Members
        private Control m_RenderingSurface;
        private Direct2DContext m_Direct2DContext;
        private SlimDX.Direct2D.Bitmap m_VideoImageBitmap;	// Current frame to render.

        //private SolidColorBrush brush;
        //private Stopwatch m_Stopwatch = new Stopwatch();
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction/Destruction
        ~RendererDirect2D()
        {
            Dispose(false);
        }
        #endregion

        #region AbstractRenderer Implementation

        #region IFrameStore Implementation
        public void StoreCurrentFrame(SlimDX.Direct2D.Bitmap bmp)
        {

            if (m_VideoImageBitmap != null)
            {
                m_VideoImageBitmap.Dispose();
            }

            m_VideoImageBitmap = bmp;
        }

        public void StoreCurrentFrame(byte[] frame, int _width, int _height, int _stride)
        {
            SlimDX.DataStream dataStream = new SlimDX.DataStream(frame, true, false);
            SlimDX.Direct2D.BitmapProperties properties = new SlimDX.Direct2D.BitmapProperties();
            properties.PixelFormat = new SlimDX.Direct2D.PixelFormat(SlimDX.DXGI.Format.B8G8R8A8_UNorm, SlimDX.Direct2D.AlphaMode.Ignore);


            if (m_VideoImageBitmap != null)
            {
                m_VideoImageBitmap.Dispose();
            }

            m_VideoImageBitmap = new SlimDX.Direct2D.Bitmap(m_Direct2DContext.RenderTarget, new Size(_width, _height));

            m_VideoImageBitmap.FromMemory(frame, _stride);
        }

        public void StoreCurrentFrame(int _width, int _height, int _stride, IntPtr _scan0)
        {
            // Convert FFMpeg BGRA image to a Direct2D.Bitmap object.
            //try
            //{
                


                if (m_VideoImageBitmap == null)
                {
                    SlimDX.DataStream dataStream = new SlimDX.DataStream(_scan0, _stride * _height, true, false);
                    SlimDX.Direct2D.BitmapProperties properties = new SlimDX.Direct2D.BitmapProperties();
                    //properties.PixelFormat = new SlimDX.Direct2D.PixelFormat(SlimDX.DXGI.Format.B8G8R8A8_UNorm, SlimDX.Direct2D.AlphaMode.Ignore);
                    properties.PixelFormat = new SlimDX.Direct2D.PixelFormat(SlimDX.DXGI.Format.B8G8R8A8_UNorm, SlimDX.Direct2D.AlphaMode.Ignore);
                    m_VideoImageBitmap = new SlimDX.Direct2D.Bitmap(m_Direct2DContext.RenderTarget, new Size(_width, _height), dataStream, _stride, properties);

                    return;
                }

                m_VideoImageBitmap.FromMemory(_scan0, _stride);
                
            //}
            //catch (Exception e)
            //{
            //    //log.Error("Error while converting image to Direct2D image.");
            //    //log.Error(e.TargetSite);
            //    //log.Error(e.Message);
            //    //log.Error(e.InnerException);
            //    //log.Error(e.StackTrace);
            //}

            #region Snippet to convert GDI+ Bitmap to Direct2D bitmap (UNUSED).
            /*
			// Note: Code from Roland Koenig on submission to SlimDX (google group)
			//Lock the gdi resource
            BitmapData drawingBitmapData = drawingBitmap.LockBits(
                new Rectangle(0, 0, drawingBitmap.Width, drawingBitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            //Prepare loading the image from gdi resource
            DataStream dataStream = new DataStream(
                drawingBitmapData.Scan0,
                drawingBitmapData.Stride * drawingBitmapData.Height,
                true, false);
            SlimDX.Direct2D.BitmapProperties properties = new SlimDX.Direct2D.BitmapProperties();
            properties.PixelFormat = new SlimDX.Direct2D.PixelFormat(
                SlimDX.DXGI.Format.B8G8R8A8_UNorm,
                SlimDX.Direct2D.AlphaMode.Premultiplied);

            //Load the image from the gdi resource
            result = new SlimDX.Direct2D.Bitmap(
                m_Direct2DContext.RenderTarget,
                new Size(drawingBitmap.Width, drawingBitmap.Height),
                dataStream, drawingBitmapData.Stride,
                properties);

            //Unlock the gdi resource
            drawingBitmap.UnlockBits(drawingBitmapData);
            */
            #endregion
        }
        public System.Drawing.Bitmap GetCurrentFrameBitmap()
        {
            // Return the current image as .NET Bitmap.
            // This method should completely replace VideoFile.CurrentFrame
            System.Drawing.Bitmap bmp = null;
            if (m_VideoImageBitmap != null)
            {
                //m_VideoImageBitmap.ComPointer
                //m_VideoImageBitmap.InternalPointer
                //m_VideoImageBitmap.PixelFormat
                // m_VideoImageBitmap.PixelSize

            }

            return bmp;
        }
        #endregion

        public void Initialize(Control _surface)
        {
            //   log.Debug("Initializing Renderer");

            // Create the rendering surface from the control.
            m_RenderingSurface = _surface;

            // Initialize the Direct2D rendering context.
            DeviceSettings2D settings = new DeviceSettings2D()
            {
                Width = _surface.ClientSize.Width,
                Height = _surface.ClientSize.Height
            };
            m_Direct2DContext = new Direct2DContext(m_RenderingSurface.Handle, settings);

            LoadResources();
        }
        public void Render()
        {
            //log.Debug("Rendering image");
            BeforeRender();
            DoRender();
            AfterRender();
        }
        public void Resize()
        {
            //log.Debug("Resizing Renderer");
            UnloadResources();
            m_Direct2DContext.RenderTarget.Resize(m_RenderingSurface.ClientSize);
            LoadResources();
            //Render();
        }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        /// <param name="disposeManagedResources">If true, managed resources should be disposed of in addition to unmanaged resources.</param>
        public void Dispose(bool _bDisposeManagedResources)
        {
            UnloadResources();

            if (m_VideoImageBitmap != null)
            {
                m_VideoImageBitmap.Dispose();
            }

            if (_bDisposeManagedResources)
            {
                m_Direct2DContext.Dispose();
            }

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods

        #region Resources
        /// <summary>
        /// LoadResources - Called on device creation/changes.
        /// </summary>
        private void LoadResources()
        {
            //brush = new SolidColorBrush(m_Direct2DContext.RenderTarget, new Color4(0.3f, 0.3f, 0.3f));
        }

        /// <summary>
        /// UnloadResources - Called on device loss/changes.
        /// </summary>
        private void UnloadResources()
        {
            //brush.Dispose();
        }
        #endregion

        #region Rendering

        /// <summary>
        /// Logic that should occur before all other rendering.
        /// </summary>
        private void BeforeRender()
        {
            m_Direct2DContext.RenderTarget.BeginDraw();
            m_Direct2DContext.RenderTarget.Transform = SlimDX.Matrix3x2.Identity;
        }
        /// <summary>
        /// Logic to render the sample.
        /// </summary>
        private void DoRender()
        {
            //try
            //{
                if (m_VideoImageBitmap != null)
                {
                    // Draw the main screen bitmap
                    // TODO: get proper destination rectangle.
                    //m_Direct2DContext.RenderTarget.DrawBitmap(m_VideoImageBitmap, m_RenderingSurface.ClientRectangle);
                    m_Direct2DContext.RenderTarget.DrawBitmap(m_VideoImageBitmap);
                }
                else
                {
                    //log.Debug("m_VideoImageBitmap is null");
                }
            //}
            //catch (Exception exp)
            //{
            //    //log.Error("Error while rendering Direct2D bitmap.");
            //    //log.Error(exp.StackTrace);
            //}
        }
        /// <summary>
        /// Logic that should occur after all other rendering.
        /// </summary>
        private void AfterRender()
        {
            m_Direct2DContext.RenderTarget.EndDraw();
        }
        #endregion

        /// <summary>
        /// Load a Direct2D bitmap from the given GDI+ bitmap.
        /// </summary>
        /// <param name="drawingBitmap">The gdi resource.</param>
        public SlimDX.Direct2D.Bitmap LoadBitmap(System.Drawing.Bitmap drawingBitmap)
        {
            // TODO: this method should not be necessary when we store the image directly in Direct2D way.
            // Note: Code from Roland Koenig on submission to SlimDX (google group)
            // log.Debug("Converting GDI+ bitmap into Direct2D bitmap");
            SlimDX.Direct2D.Bitmap result = null;

            //Lock the gdi resource
            BitmapData drawingBitmapData = drawingBitmap.LockBits(
                new Rectangle(0, 0, drawingBitmap.Width, drawingBitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            //Prepare loading the image from gdi resource
            SlimDX.DataStream dataStream = new SlimDX.DataStream(
                drawingBitmapData.Scan0,
                drawingBitmapData.Stride * drawingBitmapData.Height,
                true, false);
            SlimDX.Direct2D.BitmapProperties properties = new SlimDX.Direct2D.BitmapProperties();
            properties.PixelFormat = new SlimDX.Direct2D.PixelFormat(
                SlimDX.DXGI.Format.B8G8R8A8_UNorm,
                SlimDX.Direct2D.AlphaMode.Ignore);

            //Load the image from the gdi resource
            result = new SlimDX.Direct2D.Bitmap(
                m_Direct2DContext.RenderTarget,
                new Size(drawingBitmap.Width, drawingBitmap.Height),
                dataStream, drawingBitmapData.Stride,
                properties);

            //Unlock the gdi resource
            drawingBitmap.UnlockBits(drawingBitmapData);

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Provides creation and management functionality for a Direct2D rendering context.
    /// </summary>
    internal class Direct2DContext : IDisposable
    {
        #region Public Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContext2D"/> class.
        /// </summary>
        /// <param name="handle">The window handle to associate with the device.</param>
        /// <param name="settings">The settings used to configure the device.</param>
        public Direct2DContext(IntPtr handle, DeviceSettings2D settings)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Value must be a valid window handle.", "handle");
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.settings = settings;

            factory = new Factory();

            WindowRenderTargetProperties wrtp = new WindowRenderTargetProperties()
            {
                Handle = handle,
                PixelSize = new Size(settings.Width, settings.Height)
            };

            RenderTargetProperties rtp = new RenderTargetProperties()
            {
                PixelFormat = new SlimDX.Direct2D.PixelFormat(SlimDX.DXGI.Format.B8G8R8A8_UNorm, SlimDX.Direct2D.AlphaMode.Ignore),
                Type = RenderTargetType.Hardware // Hardware, Software.
                
            };

            RenderTarget = new WindowRenderTarget(factory, rtp, wrtp);
        }

        /// <summary>
        /// Performs object finalization.
        /// </summary>
        ~Direct2DContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        /// <param name="disposeManagedResources">If true, managed resources should be
        /// disposed of in addition to unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                RenderTarget.Dispose();
                factory.Dispose();
            }
        }

        /// <summary>
        /// Gets the underlying Direct3D render target.
        /// </summary>
        public WindowRenderTarget RenderTarget
        {
            get;
            private set;
        }

        #endregion

        #region Implementation Detail

        DeviceSettings2D settings;

        Factory factory;

        #endregion
    }

    /// <summary>
    /// Settings used to initialize a Direct2D context.
    /// </summary>
    internal class DeviceSettings2D
    {
        /// <summary>
        /// Gets or sets the width of the renderable area.
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the height of the renderable area.
        /// </summary>
        public int Height
        {
            get;
            set;
        }
    }
}
