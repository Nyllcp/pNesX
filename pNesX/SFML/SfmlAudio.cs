using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;
using SFML.System;

namespace pNesX
{
    public class Audio
    {
        private Sound _sound;
        public PnesAudioStream _audioStream;
        public Audio()
        {
            _sound = new Sound();
            _audioStream = new PnesAudioStream();
            _audioStream.Play();
        }

    }

    public class PnesAudioStream : SoundStream
    {

        private short[] _ringBuffer = new short[8192];
        private int _count = 0;
        private int _size = 8192;
        private static readonly object obj = new object();

        private static readonly int _samplesPerFrame = 735;


        private int tail = 0;
        private int head = 0;
        public PnesAudioStream()
        {
            this.Initialize(1, 44100);


        }

        public int Count { get => _count; }

        public int AvailableSpace()
        {
            return _size - _count;
        }

        protected override bool OnGetData(out short[] samples)
        {
            samples = new short[_samplesPerFrame / 2];
            GetSamples(_samplesPerFrame / 2, ref samples);
            return true;

        }

        protected override void OnSeek(Time timeOffset)
        {
            //throw new NotImplementedException();
        }

        private void IncrementPointer(ref int pointer)
        {
            pointer += 1;
            pointer %= _size;
        }

        private void GetSamples(int count, ref short[] samples)
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

        public void AddSample(short[] samples, int amount)
        {

            if (amount > AvailableSpace()) return;

            for (int i = 0; i < amount; i++)
            {

                _ringBuffer[head] = samples[i];
                _count++;
                IncrementPointer(ref head);
            }
        }



    }
}
