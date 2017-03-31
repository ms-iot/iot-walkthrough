using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Showcase
{
    public sealed partial class SlideShow : Page
    {
        private readonly Color BLACK = Color.FromArgb(255, 0, 0, 0);
        private readonly Color WHITE = Color.FromArgb(255, 255, 255, 255);

        private int currentImage = 0;
        private ThreadPoolTimer startFadeTimer;
        private DispatcherTimer _hideControlsTimer;
        private CoreDispatcher _uiDispatcher;
        private OneDriveItemController _oneDrive = new OneDriveItemController();
        private List<OneDriveItemModel> _images;
        private VoiceCommand _voiceCommand = new VoiceCommand();
        private Dictionary<string, RoutedEventHandler> _voiceCallbacks;
        private bool _whiteBackground;

        public SlideShow()
        {
            this.InitializeComponent();
            _uiDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            SetBackground(BLACK);

            _voiceCallbacks = new Dictionary<string, RoutedEventHandler>()
            {
                { "Toggle", Toggle_Click },
                { "Previous", Previous_Click },
                { "Next", Next_Click },
            };

            _hideControlsTimer = new DispatcherTimer();
            _hideControlsTimer.Tick += (object timer, object args) =>
            {
                SlideShowControls.Visibility = Visibility.Collapsed;
            };
            _hideControlsTimer.Interval = TimeSpan.FromSeconds(10);
        }

        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            AppServiceBridge.RequestReceived += PropertyUpdate;
            AppServiceBridge.RequestUpdate(new List<string> { "ConfigSlideShowBackgroundColor", "ConfigSlideShowStrech", "ConfigSlideShowDuration" });
            try
            {
                await _oneDrive.InitAsync();
                _images = await _oneDrive.GetImagesAsync(null);
            }
            catch (Exception e)
            {
                ShowError(e.Message);
                return;
            }
            if (_images.Count == 0)
            {
                ShowError("No images found in user's OneDrive.");
                return;
            }

            _hideControlsTimer.Start();
            ForegroundImage.Source = BackgroundImage.Source = await LoadImage(_images[0].Id);
            Play();

            _voiceCommand.AddCommands(_voiceCallbacks);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            AppServiceBridge.RequestReceived -= PropertyUpdate;
            Pause();
            _hideControlsTimer.Stop();
            _voiceCommand.RemoveCommands(_voiceCallbacks);
            _images = null;
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            SlideShowControls.Visibility = Visibility.Collapsed;
        }

        private async Task RunOnUi(DispatchedHandler f)
        {
            await _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, f);
        }

        private async void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            if (message.TryGetValue("ConfigSlideShowBackgroundColor", out object backgroundColor))
            {
                _whiteBackground = (string)backgroundColor == "white";
                await RunOnUi(() => { SetBackground(_whiteBackground ? WHITE : BLACK); });
            }
            if (message.TryGetValue("ConfigSlideShowStrech", out object stretch) && stretch != null)
            {
                await RunOnUi(() => { SetStretch((string)stretch); });
            }
            if (message.TryGetValue("ConfigSlideShowDuration", out object duration) && duration != null)
            {
                await RunOnUi(() => { SetTransitionDelay(double.Parse((string)duration)); });
            }
        }

        private void OnPointerMoved(object sender, RoutedEventArgs e)
        {
            if (_images != null && _images.Count != 0)
            {
                _hideControlsTimer.Stop();
                _hideControlsTimer.Start();
                SlideShowControls.Visibility = Visibility.Visible;
            }
        }

        private async void StartFadeAnimation(ThreadPoolTimer timer = null)
        {
            if (_images != null)
            {
                await _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ForegroundImage.Source = BackgroundImage.Source;
                    ForegroundImage.Opacity = 1;
                    currentImage = (currentImage + 1) % _images.Count;
                    BackgroundImage.Source = await LoadImage(_images[currentImage].Id);
                    SlideShowFade.Begin();
                });
            }
        }

        private void Play()
        {
            var timespan = TimeSpan.FromMilliseconds(5 * FadeAnimation.Duration.TimeSpan.TotalMilliseconds);
            startFadeTimer = ThreadPoolTimer.CreatePeriodicTimer(StartFadeAnimation, timespan);
        }

        private void Pause()
        {
            startFadeTimer?.Cancel();
            startFadeTimer = null;
        }

        private void ResetTimer()
        {
            if (startFadeTimer != null)
            {
                Pause();
                Play();
            }
        }

        private void SetTransitionDelay(double milliseconds)
        {
            ForegroundFadeAnimation.Duration = FadeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);
            ResetTimer();
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (startFadeTimer != null)
            {
                PlayPauseButton.Content = "\xE768";
                Pause();
            }
            else
            {
                PlayPauseButton.Content = "\xE769";
                Play();
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
            currentImage = (currentImage + _images.Count - 2) % _images.Count;
            StartFadeAnimation();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
            StartFadeAnimation();
        }

        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            SetTimePopup.IsOpen = true;
        }

        private async void SetTimePopup_ItemClick(object sender, ItemClickEventArgs e)
        {
            TextBlock item = e.ClickedItem as TextBlock;
            var currentDelay = FadeAnimation.Duration.TimeSpan.TotalMilliseconds;

            switch (item.Text)
            {
                case "Slower transition":
                    currentDelay *= 2;
                    break;

                case "Faster transition":
                    currentDelay /= 2;
                    break;

                default:
                    Debug.WriteLine("Unknown popup option " + item.Text + "selected");
                    return;
            }

            SetTransitionDelay(currentDelay);
            await AppServiceBridge.SendMessageAsync(new ValueSet
            {
                ["ConfigSlideShowDuration"] = currentDelay.ToString("N3")
            });
        }

        private void SetStretchType_Click(object sender, RoutedEventArgs e)
        {
            SetStretchTypePopup.IsOpen = true;
        }

        private void SetStretch(string stretchName)
        {
            Stretch stretch;

            switch (stretchName)
            {
                case "None":
                    stretch = Stretch.None;
                    break;

                case "Stretch":
                    stretch = Stretch.Fill;
                    break;

                case "Fit":
                    stretch = Stretch.Uniform;
                    break;

                case "Fill":
                    stretch = Stretch.UniformToFill;
                    break;

                default:
                    Debug.WriteLine($"Unknown stretch {stretchName} selected");
                    return;
            }
            ForegroundImage.Stretch = BackgroundImage.Stretch = stretch;
        }

        private async void SetStretchTypePopup_ItemClick(object sender, ItemClickEventArgs e)
        {
            var stretch = ((TextBlock)e.ClickedItem).Text;
            SetStretch(stretch);
            await AppServiceBridge.SendMessageAsync(new ValueSet
            {
                ["ConfigSlideShowStrech"] = stretch
            });
        }

        private async void ToggleBackground_Click(object sender, RoutedEventArgs e)
        {
            _whiteBackground = !_whiteBackground;
            SetBackground(_whiteBackground ? WHITE : BLACK);
            await AppServiceBridge.SendMessageAsync(new ValueSet
            {
                ["ConfigSlideShowBackgroundColor"] = _whiteBackground ? "white" : "black"
            });
        }

        private void SetBackground(Color color)
        {
            ForegroundImageGrid.Background = ImageGrid.Background = new SolidColorBrush(color);
        }

        private async Task<BitmapImage> LoadImage(string id)
        {
            var bitmap = new BitmapImage();
            using (var response = await _oneDrive.Client.Drive.Items[id].Content.Request().GetAsync())
            {
                if (response is MemoryStream)
                {
                    await bitmap.SetSourceAsync(((MemoryStream)response).AsRandomAccessStream());
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await response.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        await bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    }
                }
            }
            return bitmap;
        }
    }
}
