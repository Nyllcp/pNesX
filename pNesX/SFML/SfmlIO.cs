using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pNesX;
using SFML.System;
using SFML.Window;

namespace pNesX
{
    
    public class IO
    {
        private Clock _clock;

        private int keyData;
        private int keyDataLast;

        private bool saveStateToggle = false;
        public bool FrameLimit = true;
        private bool frameLimitToggle = false;

        public IO()
        {
            _clock = new Clock();
        }

        public int ElapsedTimeMS()
        {
            return _clock.ElapsedTime.AsMilliseconds();
        }

        public long ElapsedTimeMicro()
        {
            return _clock.ElapsedTime.AsMicroseconds();
        }

        public void ClockRestart()
        {
            _clock.Restart();
        }
        public void Input(ref Core _nes)
        {
            keyDataLast = keyData;
            keyData = 0;
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                keyData |= 1 << 7;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                keyData |= 1 << 6;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                keyData |= 1 << 5;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                keyData |= 1 << 4;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                keyData |= 1 << 3;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                keyData |= 1 << 2;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Z))
            {
                keyData |= 1 << 1;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.X))
            {
                keyData |= 1 << 0;
            }
            if (keyData != keyDataLast)
            {
                _nes.Pad1 = (byte)keyData;
            }

            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.C) && !frameLimitToggle)
            {
                FrameLimit = !FrameLimit;
            }
            frameLimitToggle = SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.C);

            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.E) && !saveStateToggle)
            {
                _nes.SaveState = true;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.R) && !saveStateToggle)
            {
                _nes.LoadState = true;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Q) && !saveStateToggle)
            {
                _nes.SelectedState--;
            }
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.W) && !saveStateToggle)
            {
                _nes.SelectedState++;
   
            }
            saveStateToggle = SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.R) | SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.E) |
                              SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.Q) | SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.W);

        }
    }
}

    

