using ShowcaseBridgeService;
using System;
using System.Collections.Generic;
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
        public ObservableCollection<NewsModel> NewsGrid { get { return this.news; } }

        private ObservableCollection<NewsModel> news = new ObservableCollection<NewsModel>();
        private AppServiceConnection _service = AppServiceConnectionFactory.GetConnection();
        private CoreDispatcher uiThreadDispatcher = null;
        private DispatcherTimer weatherTimer = new DispatcherTimer();

        public NewsAndWeather()
        {
            this.InitializeComponent();
            uiThreadDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            weatherTimer.Tick += (object sender, object e) => { UpdateWeather(); };
            weatherTimer.Interval = TimeSpan.FromSeconds(20);
            weatherTimer.Start();

            UpdateWeather();
            ConnectToAppService();
            ShowNews();
        }

        private async void ConnectToAppService()
        {
            _service.RequestReceived += (AppServiceConnection sender, AppServiceRequestReceivedEventArgs args) => {
                Debug.WriteLine("Request received!");
            };
            var status = await _service.OpenAsync();
            Debug.WriteLine("Status: " + status);
            Debug.Assert(status == AppServiceConnectionStatus.Success);
            _service.RequestReceived += Connection_RequestReceived;
        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet message = args.Request.Message;
            await uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                object temperature, humidity, pressure;

                if (message.TryGetValue("temperature", out temperature))
                {
                    Temperature.Text = FormatTemperature((double)temperature);
                }

                if (message.TryGetValue("humidity", out humidity))
                {
                    Humidity.Text = FormatHumidity((double)humidity);
                }

                if (message.TryGetValue("pressure", out pressure))
                {
                    Pressure.Text = FormatPressure((double)pressure);
                }
            });
        }

        private async void ShowNews()
        {
            BingNews bing = new BingNews();
            List<NewsModel> bingNews;
            try
            {
                bingNews = await bing.GetNews();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Debug.WriteLine("Could not contact Bing server");
                return;
            }
            news.Clear();
            foreach (var n in bingNews)
            {
                news.Add(n);
            }
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
