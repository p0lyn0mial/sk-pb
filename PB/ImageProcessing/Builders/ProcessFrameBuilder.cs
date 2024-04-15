using SK.ImageProcessing;
using SK.ImageProcessing.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Builders
{
    class FrameBuilder<T> where T : BaseImageDecorator, new()
    {
        IImage bg;
        IImage fg;
        IImage frame;

        public FrameBuilder()
        {
            bg = null;
            fg = null;
        }

        public T BuildProcessFrameDecorator()
        {
            T item = new T();
           
            item.AddFrame(frame);
            item.AddBg(bg);
            item.AddFg(fg);

            return item;
        }

        public void AddBackgroundImg(IImage background)
        {
            bg = background;
        }

        public void AddForegroundImg(IImage foreground)
        {
            fg = foreground;
        }

        public void AddFrame(IImage frame)
        {
            this.frame = frame;
        }
    }
}
