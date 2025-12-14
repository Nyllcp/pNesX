using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace pNesX
{   

    public class Video
    {
        private RenderWindow _window;
        private Texture _texture;
        private Sprite _sprite;

        private byte[] _frame;

        private uint _width;
        private uint _height;

        public Video(int nesWidth, int nesHeight, nint windowHandle)
        {
            _width = (uint)nesWidth;
            _height = (uint)nesHeight;
            _texture = new Texture(_width, _height);
            _texture.Smooth = false;
            _sprite = new Sprite(_texture);
            _sprite.Scale = new Vector2f(4f, 4f);
            _window = new RenderWindow(windowHandle);

            _frame = new byte[_width * _height * 4]; //4 Bytes per pixel

            _window.SetFramerateLimit(0);
            _window.SetVerticalSyncEnabled(false);
            _texture.Update(_frame);
            _window.Clear();
            _window.Draw(_sprite);
            _window.Display();
        }

        public void Resize(int width, int height)
        {
            float widthScale = width / (float)_width;
            float heightScale = height / (float)_height;
            _sprite.Scale = new Vector2f(widthScale, heightScale);
        }

        public void RenderFrame(uint[] frame)
        {
            UpdateFrameRGB(frame);
            _texture.Update(_frame);
            _window.Clear();
            _window.Draw(_sprite);
            _window.DispatchEvents(); // handle SFML events - NOTE this is still required when SFML is hosted in another wi
            _window.Display();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateFrameRGB(uint[] gbFrame)
        {
            for (int i = 0; i < gbFrame.Length; i++)
            {
                _frame[i * 4 + 2] = (byte)(gbFrame[i] & 0xFF);
                _frame[i * 4 + 1] = (byte)(gbFrame[i] >> 8 & 0xFF);
                _frame[i * 4 + 0] = (byte)(gbFrame[i] >> 16 & 0xFF);
                //_frame[i * 4 + 3] = (byte)((gbFrame[i] >> 24) & 0xFF);
                _frame[i * 4 + 3] = 0xFF;
            }
        }
    }

    
}
