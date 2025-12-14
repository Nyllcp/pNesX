using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace pNesX
{
    public class SfmlVideoControl : NativeControlHost
    {
        private RenderWindow _window;
        private Texture _texture;
        private Sprite _sprite;
        

        private uint _width = 256;
        private uint _height = 240;
        private byte[] _frame; // NES framebuffer 256x240
        
        public IntPtr Handle { get; private set; }  
        public int redrawFrames;

        public SfmlVideoControl()
        {

        }
        
        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)  
        {  
            var handle = base.CreateNativeControlCore(parent);  
            Handle = handle.Handle;  
            Console.WriteLine($"Handle : {Handle}");  
            return handle;  
        }

        public void Init()
        {
            _window = new RenderWindow(this.Handle);
            _texture = new Texture(_width, _height);
            _sprite = new Sprite(_texture);
            _frame = new byte[_width * _height * 4];
            _window.SetFramerateLimit(0);
            _window.SetVerticalSyncEnabled(false);
            _texture.Update(_frame);
            _window.Clear();
            _window.Draw(_sprite);
            _window.Display();
        }

        // Uppdatera framebuffer från NES-core
        public void UpdateFrame(uint[] frame)
        {
            if (frame.Length != _width * _height)
                throw new ArgumentException("Frame size mismatch");
            
            for (int i = 0; i < frame.Length; i++)
            {
                _frame[i * 4 + 2] = (byte)(frame[i] & 0xFF);
                _frame[i * 4 + 1] = (byte)((frame[i] >> 8) & 0xFF);
                _frame[i * 4 + 0] = (byte)((frame[i] >> 16) & 0xFF);
                //_frame[i * 4 + 3] = (byte)((gbFrame[i] >> 24) & 0xFF);
                _frame[i * 4 + 3] = (byte)0xFF;
            }
            InvalidateVisual(); // trigga repaint
            redrawFrames++;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
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
            _sprite.Scale = new Vector2f((float)widthRatio, (float)heightRatio);
            _texture.Update(_frame);
            _window.Clear();
            _window.Draw(_sprite);
            _window.DispatchEvents(); // handle SFML events - NOTE this is still required when SFML is hosted in another wi
            _window.Display();
        }
    }
}