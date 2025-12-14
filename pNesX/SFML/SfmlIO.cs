using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
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

        private const byte NES_UP = 0x10;
        private const byte NES_DOWN = 0x20;
        private const byte NES_LEFT = 0x40;
        private const byte NES_RIGHT = 0x80;
        private const byte NES_A = 0x1;
        private const byte NES_B = 0x2;
        private const byte NES_START = 0x8;
        private const byte NES_SELECT = 0x4;

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

        public void AvaloniaKeyDown(ref Core _nes, Avalonia.Input.KeyEventArgs e)
        {
            if(_nes == null) return;
            keyDataLast = keyData;
            //keyData = 0;
            if (e.Key == Key.Right)
            {
                keyData |= NES_RIGHT;
            }
            if (e.Key == Key.Left)
            {
                keyData |= NES_LEFT;
            }
            if (e.Key == Key.Up)
            {
                keyData |= NES_UP;
            }
            if (e.Key == Key.Down)
            {
                keyData |= NES_DOWN;
            }
            if (e.Key == Key.S)
            {
                keyData |= NES_START;
            }
            if (e.Key == Key.A)
            {
                keyData |= NES_SELECT;
            }
            if (e.Key == Key.X)
            {
                keyData |= NES_A;
            }
            if (e.Key == Key.Z)
            {
                keyData |= NES_B;
            }
            if (keyData != keyDataLast)
            {
                _nes.Pad1 = (byte)keyData;
            }

            if (e.Key == Key.C)
            {
                FrameLimit = !FrameLimit;
            }
            if (e.Key == Key.Q)
            {
                _nes.SelectedState--;
            } 
            if (e.Key == Key.W)
            {
                _nes.SelectedState++;
            }

            if (e.Key == Key.R)
            {
                _nes.LoadState = true;
            }

            if (e.Key == Key.E)
            {
                _nes.SaveState = true;
            }
            
        }
        public void AvaloniaKeyUp(ref Core _nes, Avalonia.Input.KeyEventArgs e)
        {
            if(_nes == null) return;
            keyDataLast = keyData;
            //keyData = 0;
            if (e.Key == Key.Right)
            {
                keyData &= ~NES_RIGHT;
            }
            if (e.Key == Key.Left)
            {
                keyData &= ~NES_LEFT;
            }
            if (e.Key == Key.Up)
            {
                keyData &= ~NES_UP;
            }
            if (e.Key == Key.Down)
            {
                keyData &= ~NES_DOWN;
            }
            if (e.Key == Key.S)
            {
                keyData &= ~NES_START;
            }
            if (e.Key == Key.A)
            {
                keyData &= ~NES_SELECT;
            }
            if (e.Key == Key.X)
            {
                keyData &= ~NES_A;
            }
            if (e.Key == Key.Z)
            {
                keyData &= ~NES_B;
            }
            if (keyData != keyDataLast)
            {
                _nes.Pad1 = (byte)keyData;
            }
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

    

