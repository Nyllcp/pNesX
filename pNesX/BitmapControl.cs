using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace pNesX
{
    public class NesBitmapControl : Control
    {
        private WriteableBitmap? _bitmap;
        private uint[]? _frame; // NES framebuffer 256x240

        private const int Width = 256;
        private const int Height = 240;

        public int redrawFrames;

        public NesBitmapControl()
        {
            // Skapa bitmap när kontrollen laddas
            this.AttachedToVisualTree += (s, e) =>
            {
                _bitmap = new WriteableBitmap(
                    new PixelSize(Width, Height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Premul
                );
            };
        }

        // Uppdatera framebuffer från NES-core
        public void UpdateFrame(uint[] frame)
        {
            if (frame.Length != Width * Height)
                throw new ArgumentException("Frame size mismatch");

            _frame = frame;
            InvalidateVisual(); // trigga repaint
            redrawFrames++;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (_bitmap == null || _frame == null)
                return;

            byte[] result = new byte[_frame.Length * sizeof(int)];
            Buffer.BlockCopy(_frame, 0, result, 0, result.Length);
            using (var fb = _bitmap.Lock())
            {
                // Kopiera _frame till bitmap (BGRA)
                Marshal.Copy(result, 0, fb.Address, result.Length);
            }

            // Rita bitmap i kontrollen
            var widthRatio = Bounds.Width / Width;
            var heightRatio = Bounds.Height / Height;
            var rat1 = 16f;
            var rat2 = 15f;
            double calcwidth = 0;
            double calcheight = 0;
            if(widthRatio > heightRatio)
            {
                calcheight = Bounds.Height;
                var ratio = calcheight / rat2;
                calcwidth = rat1 * ratio;
            }
            else
            {
                calcwidth = Bounds.Width;
                var ratio = calcwidth / rat1;
                calcheight = ratio * rat2;
            }
            var xoffset = (Bounds.Width - calcwidth) / 2;
            var yoffset = (Bounds.Height - calcheight) / 2;
            Rect rect = new Rect(xoffset, yoffset, calcwidth, calcheight);
            context.DrawImage(
                _bitmap,
                rect
            );
        }
    }
}