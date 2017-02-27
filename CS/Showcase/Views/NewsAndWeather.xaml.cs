using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Showcase
{
    /// <summary>
    /// Page with customizable news and local/outside weather information.
    /// </summary>
    public sealed partial class NewsAndWeather : Page
    {
        public ObservableCollection<NewsModel> NewsGrid { get { return _news; } }

        private ObservableCollection<NewsModel> _news = new ObservableCollection<NewsModel>();
        private CoreDispatcher uiThreadDispatcher = null;
        private DispatcherTimer weatherTimer = new DispatcherTimer();
        private BingNews _bing = new BingNews();

        public NewsAndWeather()
        {
            this.InitializeComponent();
            uiThreadDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            weatherTimer.Tick += (object sender, object e) => { UpdateWeather(); };
            weatherTimer.Interval = TimeSpan.FromSeconds(20);

            _bing.NewsUpdate += NewsUpdate;
            _bing.Init();

            UpdateWeather();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            weatherTimer.Start();
            AppServiceBridge.RequestReceived += Connection_RequestReceived;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            weatherTimer.Stop();
            AppServiceBridge.RequestReceived -= Connection_RequestReceived;
        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet message = args.Request.Message;

            Debug.WriteLine("RequestReceived");

            if (message.TryGetValue("temperature", out object temperature) | message.TryGetValue("humidity", out object humidity) | message.TryGetValue("pressure", out object pressure))
            {
                await uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    if (temperature != null)
                    {
                        Temperature.Text = FormatTemperature((double)temperature);
                    }

                    if (humidity != null)
                    {
                        Humidity.Text = FormatHumidity((double)humidity);
                    }

                    if (pressure != null)
                    {
                        Pressure.Text = FormatPressure((double)pressure);
                    }
                });
            }
        }

        private async void NewsUpdate(object sender, EventArgs args)
        {
            BingNews.NewsUpdateEventArgs news = (BingNews.NewsUpdateEventArgs)args;
            await uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                _news.Clear();
                foreach (var x in news.UpdatedNews)
                {
                    _news.Add(x);
                }
            });
        }

        private void News_ItemClick(object sender, ItemClickEventArgs e)
        {
            NewsModel news = (NewsModel)e.ClickedItem;
            Frame.Navigate(typeof(WebViewPage), news.Url);
        }

        private async void UpdateWeather()
        {
            var weatherProvider = new OpenWeatherMap();
            var weather = await weatherProvider.GetWeather();
            if (weather != null)
            {
                OutsideIcon.Source = new BitmapImage(new Uri(weather.Icon));
                OutsideCondition.Text = weather.Condition;
                OutsideTemperature.Text = FormatTemperature(weather.Temperature);
                OutsideHumidity.Text = FormatHumidity(weather.Humidity);
                OutsidePressure.Text = FormatPressure(weather.Pressure);
            }
        }

        private string FormatTemperature(double temperature)
        {
            if (true)  // TODO Add temperature unit setting
            {
                return temperature.ToString("N1") + " °C";
            }
        }

        private string FormatHumidity(double humidity)
        {
            return humidity.ToString("N1") + "%";
        }

        private string FormatPressure(double pressure)
        {
            return pressure.ToString("N0") + " Pa";
        }
    }
}
