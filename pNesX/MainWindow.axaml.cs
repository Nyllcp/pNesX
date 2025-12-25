using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading;
using Avalonia.Input;
using Avalonia.Media.Imaging;

namespace pNesX
{
    public partial class MainWindow : Window
    {

        private PortAudioX _audio;
        private IO _io;
        private Core? _nes;
        private Rom? _rom;


        private const int NesWidth = 256;
        private const int NesHeight = 240;

        private Thread? _emulationThread;
        private Thread? _renderThread;
        private volatile bool _run;

        private readonly object _frameLock = new();
        private uint[] _frameBuffer = new uint[NesWidth * NesHeight];
        
        private IStorageFolder? _lastFolder;



        public MainWindow()
        {
            InitializeComponent();
            _audio = new PortAudioX();
            _io = new IO();
            Opened += OnOpened;
            Closing += OnClosing;
            InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown);
            InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp);
            
            
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if(_nes != null) _io.AvaloniaKeyDown(ref _nes, e);
            e.Handled = true;
        }
        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if(_nes != null) _io.AvaloniaKeyUp(ref _nes, e);
            e.Handled = true;
        }
        private void OnOpened(object? sender, EventArgs e)
        {

            _audio.Initialize();
            RomNameText.Text = "No Rom Loaded";
            StateText.Text = "Selected State : null";
            FpsText.Text = "Emulator FPS:";
            FpsRedrawText.Text = "Blit FPS:";

            using var stream = File.OpenRead("pNesX_title.bmp"); 
            NesView.SetBitmapFromStream(stream);

        }

        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
           
            StopEmulation();
            _audio.TerminateStream();
            if (_rom != null)
                _rom.SavePRGRam();
        }


        private async void Open_Click(object? sender, RoutedEventArgs e)
        {
            
            
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return;

            _lastFolder = await topLevel.StorageProvider
                .TryGetFolderFromPathAsync(Environment.CurrentDirectory);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open NES ROM",
                    AllowMultiple = false,
                    SuggestedStartLocation = _lastFolder,
                    FileTypeFilter = [ new FilePickerFileType("NES ROMs")
                    {
                        Patterns = [ "*.nes", "*.zip" ]
                    }]
      
                });

            if (files.Count == 0)
                return;
            _lastFolder = await files[0].GetParentAsync();
            
            StopEmulation();
            
            var file = files[0];
            
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
                StartEmulation();
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
                StartEmulation();
        }


        private void Exit_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void StartEmulation()
        {
            _run = true;
            _audio.Start();
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
            _audio.Stop();
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
            long lastTime = _io.ElapsedTimeMS();

            while (_run)
            {
                if (_io.FrameLimit)
                {
                    while (_audio.Count > _audio.SamplesPerFrame * 2 && _run)
                    {
                        // spin

                    }
                }
                
                _nes.RunOneFrame();
                
                _audio.AddSample(
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
            long lastTime = _io.ElapsedTimeMS();
            while (_run)
            {

            

                var millisNow = _io.ElapsedTimeMicro();
                if(millisNow - renderLimiter > 16667)
                {
                    //60 fps
                    renderLimiter = millisNow;
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
                        FpsRedrawText.Text = $"Blit FPS: {redrawFps}";

                    });
                }
               
            }
        }

        private void Interpolation_OnClick(object? sender, RoutedEventArgs e)
        {
            if(sender == null) return;
            var tag = ((MenuItem)sender).Tag;

            switch (tag)
            {
                case "None": NesView.InterpolationMode(BitmapInterpolationMode.None); break;
                case "Low": NesView.InterpolationMode(BitmapInterpolationMode.LowQuality); break;
                case "High": NesView.InterpolationMode(BitmapInterpolationMode.HighQuality); break;
            }
  
        }

        private void HideOverscan_OnClick(object? sender, RoutedEventArgs e)
        {
            _nes.HideOverscan = !_nes.HideOverscan;
            HideOverscan.IsChecked = _nes.HideOverscan;
        }
        
        private void Flicker_Click(object? sender, RoutedEventArgs e)
        {
            if (_rom == null || _nes == null) 
                return;
            lock (_frameLock)
            {
                _nes.DisableSpriteFlicker = !_nes.DisableSpriteFlicker;
                FlickerMenu.IsChecked = _nes.DisableSpriteFlicker;
            }
        }
    }

}