﻿using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MView.Audio
{
    public class AudioPlayback : IDisposable
    {
        private WaveOut playbackDevice;
        private WaveStream fileStream;

        public event EventHandler<FftEventArgs> FftCalculated;

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        public void Load(string fileName, bool isVorbis = false)
        {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(fileName, isVorbis);
        }

        private void CloseFile()
        {
            fileStream?.Dispose();
            fileStream = null;
        }

        private void OpenFile(string fileName, bool isVorbis = false)
        {
            try
            {
                if (isVorbis)
                {
                    var inputStream = new VorbisWaveReader(fileName);
                    fileStream = inputStream;

                    var aggregator = new AudioSampleAggregator(inputStream);
                    aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
                    aggregator.PerformFFT = true;
                    aggregator.FftCalculated += (s, a) => FftCalculated?.Invoke(this, a);
                    aggregator.MaximumCalculated += (s, a) => MaximumCalculated?.Invoke(this, a);
                    playbackDevice.Init(aggregator);
                    playbackDevice.PlaybackStopped += OnPlaybackStopped;
                }
                else
                {
                    var inputStream = new AudioFileReader(fileName);
                    fileStream = inputStream;

                    var aggregator = new AudioSampleAggregator(inputStream);
                    aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
                    aggregator.PerformFFT = true;
                    aggregator.FftCalculated += (s, a) => FftCalculated?.Invoke(this, a);
                    aggregator.MaximumCalculated += (s, a) => MaximumCalculated?.Invoke(this, a);
                    playbackDevice.Init(aggregator);
                    playbackDevice.PlaybackStopped += OnPlaybackStopped;
                }
            }
            catch (Exception)
            {
                CloseFile();
                throw;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Stop();
        }

        private void EnsureDeviceCreated()
        {
            if (playbackDevice == null)
            {
                CreateDevice();
            }
        }

        private void CreateDevice()
        {
            playbackDevice = new WaveOut { DesiredLatency = 200 };
        }

        public void Play()
        {
            if (playbackDevice != null && fileStream != null && playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                playbackDevice.Play();
            }
        }

        public void Pause()
        {
            playbackDevice?.Pause();
        }

        public void Stop()
        {
            playbackDevice?.Stop();
            if (fileStream != null)
            {
                fileStream.Position = 0;
            }
        }

        public double GetCurrentPosition()
        {
            if (fileStream != null)
            {
                return fileStream.CurrentTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }

        public void SetPosition(double seconds)
        {
            if (playbackDevice != null)
            {
                fileStream.CurrentTime = TimeSpan.FromSeconds(seconds);
            }
        }

        public TimeSpan GetTotalTime()
        {
            if (playbackDevice != null)
            {
                return fileStream.TotalTime;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public void SetVolume(float volume)
        {
            if (playbackDevice != null)
            {
                playbackDevice.Volume = volume;
            }
        }

        public void Dispose()
        {
            Stop();
            CloseFile();
            playbackDevice?.Dispose();
            playbackDevice = null;
        }
    }
}
