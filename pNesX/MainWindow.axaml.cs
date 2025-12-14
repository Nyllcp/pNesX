using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using pNesX;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace pNesX
{
    public partial class MainWindow : Window
    {

        private Audio _audio;
        private IO _io;
        private Core _nes;
        private Rom _rom;


        private const int NesWidth = 256;
        private const int NesHeight = 240;

        private Thread _emulationThread;
        private Thread _renderThread;
        private volatile bool _run;

        private readonly object _frameLock = new();
        private uint[] _frameBuffer = new uint[NesWidth * NesHeight];



        public MainWindow()
        {
            InitializeComponent();

            Opened += OnOpened;
            Closing += OnClosing;


        }

        private void OnOpened(object? sender, EventArgs e)
        {


            _audio = new Audio();
            _io = new IO();



            RomNameText.Text = "No Rom Loaded";
            StateText.Text = "Selected State : 0";
            FpsText.Text = "Emulator FPS:";

            uint[] _frame = new uint[NesWidth * NesHeight];
            for (int i = 0; i < _frame.Length; i++)
            {
              
                _frame[i] = 0xFFFF0000;
            }
            NesView.UpdateFrame(_frame);


        }

        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            StopEmulation();

            if (_rom != null)
                _rom.SavePRGRam();
        }


        private async void Open_Click(object? sender, RoutedEventArgs e)
        {
            StopEmulation();

            //var dialog = new OpenFileDialog
            //{
            //    Filters =
            //    {
            //        new FileDialogFilter
            //        {
            //            Name = "NES ROMs",
            //            Extensions = { "nes", "zip" }
            //        }
            //    }
            //};

            //var result = await dialog.ShowAsync(this);
            //if (result == null || result.Length == 0)
            //    return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open NES ROM",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                new FilePickerFileType("NES ROMs")
                {
                    Patterns = new[] { "*.nes", "*.zip" }
                }
                    }
                });

            if (files.Count == 0)
                return;

            var file = files[0];

            // Full sökväg till filen
            var path = file.Path.LocalPath;

            _rom = new Rom();
            _nes = new Core();

            if (!_rom.Load(path))
            {
                RomNameText.Text = "Invalid file";
                return;
            }

            if (_nes.LoadRom(_rom))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                RomNameText.Text = $"{name} Mapper {_rom.mapperNumber}";
                RunEmulation();
            }
            else
            {
                RomNameText.Text = $"Mapper {_rom.mapperNumber} not implemented";
            }
        }

        private void Reset_Click(object? sender, RoutedEventArgs e)
        {
            if (_rom == null)
                return;

            StopEmulation();

            _rom.SavePRGRam();
            _nes = new Core();

            if (_nes.LoadRom(_rom))
                RunEmulation();
        }

        private void Exit_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }



        private void RunEmulation()
        {
            _run = true;
            StartEmulation();
            FileMenu.Focusable = !_run;
            ResetMenu.Focusable = !_run;
        }

        private void StartEmulation()
        {
            if (_emulationThread != null && _emulationThread.IsAlive)
                return;
            if (_renderThread != null && _renderThread.IsAlive)
                return;

            _emulationThread = new Thread(EmulationThread)
            {
                IsBackground = true,
                Name = "NES Emulation Thread"
            };

            _emulationThread.Start();

            _renderThread = new Thread(RenderThread)
            {
                IsBackground = true,
                Name = "NES Render Thread"
            };

            _renderThread.Start();
           

        }


        private void StopEmulation()
        {
            _run = false;

            if (_emulationThread != null &&
                _emulationThread.IsAlive)
            {
                _emulationThread.Join();
            }
            if (_renderThread != null &&
                _renderThread.IsAlive)
            {
                _renderThread.Join();
            }
        }


        private void EmulationThread()
        {
            int frames = 0;
            int lastTime = _io.ElapsedTimeMS();
            while (_run)
            {
                if (_io.FrameLimit)
                {
                    while (_audio._audioStream.Count > (735 * 2))
                    {
                        // spin
                    }
                }

                _io.Input(ref _nes);
                _nes.RunOneFrame();

                _audio._audioStream.AddSample(
                    _nes.Samples,
                    _nes.NoOfSamples
                );

                lock (_frameLock)
                {
                    _frameBuffer = _nes.Frame;
                    
                }
                frames++;
                var now = _io.ElapsedTimeMS();
                if (now - lastTime > 1000)
                {
                    lastTime = now;
                    var fps = frames;
                    frames = 0;

                    Dispatcher.UIThread.Post(() =>
                    {
                        FpsText.Text = $"Emulated FPS: {fps}";

                    });
                }
            }
        }

        private void RenderThread()
        {
            uint[] frame;
            long renderLimiter = _io.ElapsedTimeMicro();
            int lastTime = _io.ElapsedTimeMS();
            while (_run)
            {

            

                var milisNow = _io.ElapsedTimeMicro();
                if(milisNow - renderLimiter > 16667)
                {
                    //60 fps
                    renderLimiter = milisNow;
                    lock (_frameLock)
                    {
                        frame = new uint[_frameBuffer.Length];
                        _frameBuffer.CopyTo(frame, 0);

                    }
                    Dispatcher.UIThread.Post(() =>
                    {
                        NesView.UpdateFrame(frame);
                        StateText.Text = $"Selected State : {_nes.SelectedState}";

                    });
                }


                var now = _io.ElapsedTimeMS();
                if (now - lastTime > 1000)
                {
                    lastTime = now;
                    var redrawFps = NesView.redrawFrames;
                    NesView.redrawFrames = 0;

                    Dispatcher.UIThread.Post(() =>
                    {
                        FpsRedrawText.Text = $"Redraw FPS: {redrawFps}";

                    });
                }
               
            }
        }

    }

}