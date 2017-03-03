using System.Diagnostics;
using System;
using Windows.Devices.Gpio;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Background;

namespace Showcase
{
    /// <summary>
    /// Main page layout, containing a navigation panel and a frame for main content.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 21;
        private GpioPin pin;
        private MediaElement speechElement = new MediaElement();
        private SpeechSynthesizer synth = new SpeechSynthesizer();
        private CoreDispatcher uiThreadDispatcher = null;
        private VoiceRecognition voiceRecognitionTask = new VoiceRecognition();

        public MainPage()
        {
            this.InitializeComponent();
            Debug.WriteLine("Running tasks:");
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                Debug.WriteLine(task.Value.Name);
            }
            uiThreadDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            InitGPIO();

            voiceRecognitionTask.recognitionCallback += VoiceCommandCallback;
            Unloaded += MainPage_Unloaded;
            TTS("Welcome");
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await AppServiceBridge.InitAsync();
            ContentFrame.Navigate(typeof(NewsAndWeather));
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            pin.Dispose();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                Debug.WriteLine("No GPIO controller found");
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            SetLed(GpioPinValue.Low);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private async void TTS(string text)
        {
            var stream = await synth.SynthesizeTextToStreamAsync(text);

            this.speechElement.SetSource(stream, stream.ContentType);
            this.speechElement.Play();
        }

        private async void VoiceCommand_Click(object sender, RoutedEventArgs e)
        {
            SetLed(GpioPinValue.High);
            await voiceRecognitionTask.RunVoiceRecognition(uiThreadDispatcher);
        }

        private void ContentNavigate(Type page)
        {
            if (ContentFrame.CurrentSourcePageType != page)
            {
                ContentFrame.Navigate(page);
                ContentFrame.BackStack.Clear();
            }
        }

        private void WiFiConnect_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(WiFiConnection));
        }

        private void NewsAndWeather_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(NewsAndWeather));
        }

        private void SlideShow_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(SlideShow));
        }

        private void PlayMedia_Click(object sender, RoutedEventArgs e)
        {
            ContentNavigate(typeof(MediaPlayerPage));
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

        private void VoiceCommandCallback(String command)
        {
            SetLed(GpioPinValue.Low);
            Debug.WriteLine("Heard you say " + command);
            if (command == VoiceRecognition.SLIDESHOW_COMMAND)
                ContentNavigate(typeof(SlideShow));
        }

        private void SetLed(GpioPinValue value)
        {
            if (pin != null)
            {
                pin.Write(value);
            }
        }
    }
}
