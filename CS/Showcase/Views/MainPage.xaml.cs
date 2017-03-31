using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Collections.Generic;
using Windows.UI.Core;

namespace Showcase
{
    public sealed partial class MainPage : Page
    {
        private VoiceCommand _voiceCommand = new VoiceCommand();
        private Dictionary<string, RoutedEventHandler> _voiceCallbacks;

        public MainPage()
        {
            this.InitializeComponent();

            _voiceCommand.Init(CoreWindow.GetForCurrentThread().Dispatcher);
            _voiceCallbacks = new Dictionary<string, RoutedEventHandler>()
            {
                { "Start slideShow", SlideShow_Click },
                { "Start media player", MediaPlayer_Click },
                { "Start WiFI connection", WiFiConnection_Click },
                { "Start news and weather", NewsAndWeather_Click },
            };
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await AppServiceBridge.InitAsync();
            new TextToSpeech("Welcome").Play();
            _voiceCommand.AddCommands(_voiceCallbacks);
            ContentNavigate(typeof(NewsAndWeather));
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            _voiceCommand.RemoveCommands(_voiceCallbacks);
        }

        private void ContentNavigate(Type page)
        {
            Splitter.IsPaneOpen = false;
            if (ContentFrame.CurrentSourcePageType != page)
            {
                ContentFrame.Navigate(page);
                ContentFrame.BackStack.Clear();
            }
        }

        private async void VoiceCommand_Click(object sender, RoutedEventArgs e)
        {
            await _voiceCommand.RunVoiceRecognition();
        }

        private void SlideShow_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(SlideShow));
        }

        private void MediaPlayer_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(MediaPlayerPage));
        }

        private void WiFiConnection_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(WiFiConnection));
        }

        private void NewsAndWeather_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(NewsAndWeather));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(Settings));
        }

        private void Footer_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(WebViewPage), ((HyperlinkButton)sender).Tag.ToString());
        }

        private void PanelToggle_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }
    }
}
