using System;
using Avalonia.Input;

namespace pNesX
{
    
    public class IO
    {
   

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
        }

        public long ElapsedTimeMS()
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return now;
        }

        public long ElapsedTimeMicro()
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMicrosecond;
            return now;
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
      
    }
}

    

