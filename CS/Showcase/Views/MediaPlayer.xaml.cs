using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Showcase
{
    public sealed partial class MediaPlayer : Page
    {
        public MediaPlayer()
        {
            this.InitializeComponent();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Player.Source = null;
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            // FIXME File picker doesn't work on IoT, remove when media discovery/auto import is implemented
            var picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mp3");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Player.Source = MediaSource.CreateFromStorageFile(file);
            }
        }

        private async void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(KnownFolders.PicturesLibrary.Path);
            IReadOnlyList<StorageFile> fileList = await KnownFolders.PicturesLibrary.GetFilesAsync();
            Player.Source = MediaSource.CreateFromStorageFile(fileList[0]);
        }
    }
}
