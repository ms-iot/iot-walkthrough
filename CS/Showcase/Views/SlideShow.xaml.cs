using System;
using System.Diagnostics;
using Windows.Graphics.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Showcase
{
    public sealed partial class SlideShow : Page
    {
        private readonly String[] imageList = { "http://openwalls.com/image/11878/windows_background_1920x1200.jpg", "http://wallpaperswide.com/download/windows_10_hero_4k-wallpaper-2880x1800.jpg" };
        private int currentImage = 0;
        private int imageTime = 10000;
        private ThreadPoolTimer startFadeTimer;
        private CoreDispatcher uiDispatcher;

        public SlideShow()
        {
            this.InitializeComponent();
            uiDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // FIXME handle dynamic resizing
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var width = bounds.Width * scaleFactor;
            var height = bounds.Height * scaleFactor;
            ImageGrid.Width = width;
            ImageGrid.Height = height;
            //ForegroundImage.RenderTransformOrigin = new Windows.Foundation.Point(width / 2, height / 2);
            Debug.WriteLine("Resolution: " + width + "x" + height);
            BackgroundImage.Source = new BitmapImage(new Uri(imageList[0]));
            Play();
        }

        private async void StartFadeAnimation(ThreadPoolTimer timer = null)
        {
            await uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ForegroundImage.Source = BackgroundImage.Source;
                ForegroundImage.Opacity = 1;
                currentImage = (currentImage + 1) % imageList.Length;
                BackgroundImage.Source = new BitmapImage(new Uri(imageList[currentImage]));
                SlideShowFade.Begin();
            });
        }

        private void SlideShow_Tapped(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void Play()
        {
            startFadeTimer = ThreadPoolTimer.CreatePeriodicTimer(StartFadeAnimation, TimeSpan.FromMilliseconds(imageTime));
        }

        private void Pause()
        {
            startFadeTimer.Cancel();
            startFadeTimer = null;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
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

        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            TimeDropdown.IsOpen = true;
            Debug.WriteLine(TimeDropdown.IsOpen);
        }

        private void SetTimePopup_ItemClick(object sender, ItemClickEventArgs e)
        {
            TextBlock item = e.ClickedItem as TextBlock;

            switch (item.Text)
            {
                case "Faster transition":
                    Debug.WriteLine("Prev fade time " + FadeAnimation.Duration.TimeSpan.Milliseconds);
                    FadeAnimation.Duration = TimeSpan.FromMilliseconds(FadeAnimation.Duration.TimeSpan.TotalMilliseconds / 2);
                    Debug.WriteLine("Fade time " + FadeAnimation.Duration.TimeSpan.Milliseconds);
                    break;

                case "Faster image switch":
                    imageTime /= 2;
                    Debug.WriteLine("Switch time " + imageTime);
                    Pause();
                    Play();
                    break;

                default:
                    Debug.WriteLine("Unknown popup option " + item.Text + "selected");
                    break;
            }
        }
    }
}
