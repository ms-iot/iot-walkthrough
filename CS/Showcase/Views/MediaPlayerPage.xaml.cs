using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Showcase
{
    public sealed partial class MediaPlayerPage : Page
    {
        private CoreDispatcher uiThreadDispatcher = null;
        private IReadOnlyList<StorageFile> _fileList;
        private int _currentFile;
        private VoiceCommand _voiceCommand = new VoiceCommand();
        private Dictionary<string, RoutedEventHandler> _voiceCallbacks;

        public MediaPlayerPage()
        {
            this.InitializeComponent();
            uiThreadDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            Player.AutoPlay = true;

            _voiceCallbacks = new Dictionary<string, RoutedEventHandler>()
            {
                { "Play audio", PlayAudio_Click },
                { "Play music", PlayAudio_Click },
                { "Play video", PlayVideo_Click },
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _voiceCommand.AddCommands(_voiceCallbacks);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _voiceCommand.RemoveCommands(_voiceCallbacks);
            Player.Source = null;
        }

        private async void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            _fileList = await KnownFolders.MusicLibrary.GetFilesAsync();
            if (_fileList.Count != 0)
            {
                StartPlayback();
            }
            else
            {
                PlayAudioButton.IsEnabled = false;
                PlayAudioButton.Content = "No files in Music library";
            }
        }

        private async void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            _fileList = await KnownFolders.VideosLibrary.GetFilesAsync();
            if (_fileList.Count != 0)
            {
                StartPlayback();
            }
            else
            {
                PlayVideoButton.IsEnabled = false;
                PlayVideoButton.Content = "No files in Video library";
            }
        }

        private void StartPlayback()
        {
            _currentFile = 0;
            Player.Source = MediaSource.CreateFromStorageFile(_fileList[0]);
            Player.MediaPlayer.MediaEnded += NextMedia;
            Player.MediaPlayer.MediaFailed += NextMedia;
        }

        private async void NextMedia(MediaPlayer sender, object args)
        {
            _currentFile = (_currentFile + 1) % _fileList.Count;
            await uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Player.Source = MediaSource.CreateFromStorageFile(_fileList[_currentFile]);
            });
        }
    }
}
