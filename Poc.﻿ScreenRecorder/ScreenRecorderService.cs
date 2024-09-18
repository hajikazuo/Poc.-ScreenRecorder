using ScreenRecorderLib;
using System.Diagnostics;

namespace Poc._﻿ScreenRecorder
{
    public class ScreenRecorderService
    {
        private bool _isRecording;
        private Stopwatch _stopWatch;
        private Recorder _recorder;
        private CancellationTokenSource _cts;

        public ScreenRecorderService()
        {
            InitializeRecorder();
        }

        private void InitializeRecorder()
        {
            var audioInputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
            var audioOutputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
            string selectedAudioInputDevice = audioInputDevices.Count > 0 ? audioInputDevices.First().DeviceName : null;
            string selectedAudioOutputDevice = audioOutputDevices.Count > 0 ? audioOutputDevices.First().DeviceName : null;

            var opts = new RecorderOptions
            {
                AudioOptions = new AudioOptions
                {
                    AudioInputDevice = selectedAudioInputDevice,
                    AudioOutputDevice = selectedAudioOutputDevice,
                    IsAudioEnabled = true,
                    IsInputDeviceEnabled = true,
                    IsOutputDeviceEnabled = true,
                }
            };

            _recorder = Recorder.CreateRecorder(opts);
            _recorder.OnRecordingFailed += Rec_OnRecordingFailed;
            _recorder.OnRecordingComplete += Rec_OnRecordingComplete;
            _recorder.OnStatusChanged += Rec_OnStatusChanged;
        }

        public void StartRecording()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            string filePath = Path.Combine(Path.GetTempPath(), "ScreenRecorder", $"{timestamp} {Guid.NewGuid()}.mp4");
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
