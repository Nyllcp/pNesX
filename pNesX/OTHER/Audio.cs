using System;
using System.Threading;
using PortAudioSharp;

namespace pNesX
{
    public class PortAudioX
    {
        private Stream _stream;
        private RingBuffer _ringBuffer;
        
        
        private static readonly int _samplesPerFrame = 735;
        private Stream.Callback _callbackDelegate;
        public PortAudioX()
        {
            
            _ringBuffer = new RingBuffer(4096);
            _callbackDelegate = new Stream.Callback(AudioCallback);

        }
        
        public void AddSample(short[] samples, int amount) { _ringBuffer.AddSample(samples, amount); }
        public int Count => _ringBuffer.Count;
        
        public void Initialize()
        {
            PortAudio.Initialize();
            var streamParameters = new StreamParameters();
            var device = PortAudio.DefaultOutputDevice;
            streamParameters.channelCount = 1;
            streamParameters.device = device;
            streamParameters.sampleFormat = SampleFormat.Int16;
            _stream = new Stream(null,streamParameters,44100,256,StreamFlags.NoFlag,_callbackDelegate,IntPtr.Zero);
            _stream.Start();
        }
        
        private StreamCallbackResult AudioCallback(
            IntPtr input,
            IntPtr output,
            uint frameCount,
            ref StreamCallbackTimeInfo timeInfo,
            PortAudioSharp.StreamCallbackFlags statusFlags,
            IntPtr userData)
        {
            _ringBuffer.GetSamplesPointer((int)frameCount, output);
            return StreamCallbackResult.Continue;
        }
        
       

    }
     public class RingBuffer
        {

            private short[] _ringBuffer;
            private int _count = 0;
            private int _size = 0;
            private static readonly object obj = new object();

            


            private int tail = 0;
            private int head = 0;
            public RingBuffer(int size)
            {
                _size = size;
                _ringBuffer = new short[_size];
            }

            public int Count { get => _count; }

            public int AvailableSpace()
            {
                return _size - _count;
            }
            

            private void IncrementPointer(ref int pointer)
            {
                pointer += 1;
                pointer %= _size;
            }
            
            public void GetSamples(int count, ref short[] samples)
            {
                if (count > _count)
                {
                    //Console.WriteLine("Ran ouf of sound, left in buffer {0}", _count);
                    return;
                }
                lock (obj)
                {
                    for (int i = 0; i < count; i++)
                    {
                        samples[i] = _ringBuffer[tail];
                        _count--;
                        IncrementPointer(ref tail);
                    }
                }
            }
            
            public void GetSamplesPointer(int count, IntPtr buffer)
            {
                if (count > _count)
                {
                    unsafe
                    {
                        short* ptr = (short*)buffer;
                        for (int i = 0; i < count; i++)
                        {
                            *ptr = 0;
                            ptr++;
                        }
                    }
                    return;
                }
                lock (obj)
                {
                    unsafe
                    {
                        short* ptr = (short*)buffer;
                        for (int i = 0; i < count; i++)
                        {
                            *ptr= _ringBuffer[tail];
                            ptr++;
                            _count--;
                            IncrementPointer(ref tail);
                        }
                    }
                    
                }
            }

            public void AddSample(short[] samples, int amount)
            {

                if (amount > AvailableSpace()) return;

                lock (obj)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        _ringBuffer[head] = samples[i];
                        _count++;
                        IncrementPointer(ref head);
                    }
                }
                
            }



        }
}
