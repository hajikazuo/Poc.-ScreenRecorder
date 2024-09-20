using NAudio.CoreAudioApi;
using ScreenRecorderLib;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Poc._ScreenRecorder
{
    public class ScreenRecorderService
    {
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        public const int SM_CXSCREEN = 0; 
        public const int SM_CYSCREEN = 1; 

        private bool _isRecording;
        private Stopwatch _stopWatch;
        private Recorder _recorder;
        private CancellationTokenSource _cts;
        private readonly string _outputFolder;
        private readonly string _frameRate;

        public ScreenRecorderService(IConfiguration configuration)
        {
            _outputFolder = configuration.GetSection("FolderVideos").Value;
            _frameRate = configuration.GetSection("FrameRate").Value;
            InitializeRecorder();
        }

        private void InitializeRecorder()
        {
            var audioInputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
            var audioOutputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
            string selectedAudioInputDevice = GetDefaultAudioInputDevice();
            string selectedAudioOutputDevice = GetDefaultAudioOutputDevice();

            var displaySources = Recorder.GetDisplays();

            var screenSize = GetScreenSize();
            int width = screenSize.Width;
            int height = screenSize.Height;

            var opts = new RecorderOptions
            {
                AudioOptions = new AudioOptions
                {
                    AudioInputDevice = selectedAudioInputDevice,
                    AudioOutputDevice = selectedAudioOutputDevice,
                    IsAudioEnabled = true,
                    IsInputDeviceEnabled = true,
                    IsOutputDeviceEnabled = true,
                },
                OutputOptions = new OutputOptions
                {
                    OutputFrameSize = new ScreenSize(width, height)
                },
                SourceOptions = new SourceOptions
                {
                    RecordingSources = new List<RecordingSourceBase>(displaySources)
                },
            };

            opts.VideoEncoderOptions = new VideoEncoderOptions();
            opts.VideoEncoderOptions.Framerate = int.Parse(_frameRate);
            opts.VideoEncoderOptions.Encoder = new H264VideoEncoder { BitrateMode = H264BitrateControlMode.Quality, EncoderProfile = H264Profile.Baseline };
            opts.VideoEncoderOptions.Quality = 40;

            _recorder = Recorder.CreateRecorder(opts);
            _recorder.OnRecordingFailed += Rec_OnRecordingFailed;
            _recorder.OnRecordingComplete += Rec_OnRecordingComplete;
            _recorder.OnStatusChanged += Rec_OnStatusChanged;
        }

        private (int Width, int Height) GetScreenSize()
        {
            int width = GetSystemMetrics(SM_CXSCREEN); 
            int height = GetSystemMetrics(SM_CYSCREEN); 
            return (width, height);
        }

        private string GetDefaultAudioInputDevice()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                return defaultDevice.FriendlyName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter o dispositivo de entrada de áudio: {ex.Message}");
                return "Erro ao obter dispositivo de entrada de áudio";
            }
        }

        private string GetDefaultAudioOutputDevice()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                return defaultDevice.FriendlyName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter o dispositivo de saída de áudio: {ex.Message}");
                return "Erro ao obter dispositivo de saída de áudio";
            }
        }


        public void StartRecording()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            string fileName = $"{timestamp} {Guid.NewGuid()}.mp4";
            string filePath = Path.Combine(_outputFolder, fileName);
            _recorder.Record(filePath);
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        return;
                    if (_isRecording)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Elapsed: {_stopWatch.Elapsed:mm\\:ss\\:fff}");
                    }
                    await Task.Delay(10);
                }
            }, token);
        }

        public void StopRecording()
        {
            _cts?.Cancel();
            _recorder.Stop();
        }

        private void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            switch (e.Status)
            {
                case RecorderStatus.Idle:
                    break;
                case RecorderStatus.Recording:
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    _isRecording = true;
                    Console.WriteLine("Recording started");
                    Console.WriteLine("Press ESC to stop recording");
                    break;
                    //case RecorderStatus.Paused:
                //    Console.WriteLine("Recording paused");
                //    break;
                case RecorderStatus.Finishing:
                    Console.WriteLine("Finishing encoding");
                    break;
                default:
                    break;
            }
        }

        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            Console.WriteLine("Recording completed");
            _isRecording = false;
            _stopWatch?.Stop();
            Console.WriteLine($"File: {e.FilePath}");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
        }

        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            Console.WriteLine("Recording failed with: " + e.Error);
            _isRecording = false;
            _stopWatch?.Stop();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
        }
    }
}
