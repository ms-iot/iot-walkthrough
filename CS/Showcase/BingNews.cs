using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.ApplicationModel.AppService;
using Windows.Data.Json;
using Windows.Foundation.Collections;
using Windows.System.Threading;

namespace Showcase
{
    class BingNews
    {
        private const String ENDPOINT = "https://api.cognitive.microsoft.com/bing/v5.0/news/";
        private ThreadPoolTimer _timer;
        private object _timerLock = new object();
        private bool _started;
        // Properties
        private string _key;
        private string _market = "en-US";
        private string _category;

        public class NewsUpdateEventArgs : EventArgs
        {
            private List<NewsModel> _updatedNews;

            public NewsUpdateEventArgs(List<NewsModel> updatedNews)
            {
                _updatedNews = updatedNews;
            }

            public List<NewsModel> UpdatedNews { get { return _updatedNews; } }
        }

        public BingNews()
        {
            AppServiceBridge.RequestReceived += PropertyUpdate;
            AppServiceBridge.RequestUpdate("ConfigNewsMarket");
            AppServiceBridge.RequestUpdate("ConfigNewsCategory");
            AppServiceBridge.RequestUpdate("bingKey");
        }

        public void Start()
        {
            _started = true;
            InitTimer();
        }

        public void Stop()
        {
            _started = false;
            lock (_timerLock)
            {
                if (_timer != null)
                {
                    _timer.Cancel();
                }
            }
        }

        private void InitTimer()
        {
            lock (_timerLock)
            {
                if (_started && _timer == null && _key != null)
                {
                    _timer = ThreadPoolTimer.CreatePeriodicTimer((ThreadPoolTimer timer) =>
                    {
                        FetchNews();
                    }, TimeSpan.FromMinutes(5));
                    FetchNews();
                }
            }
        }

        private bool TryGetValue(ValueSet set, string key, ref string target)
        {
            set.TryGetValue(key, out object value);
            if (value != null)
            {
                target = (string)value;
                return true;
            }
            return false;
        }

        private void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            TryGetValue(message, "ConfigNewsMarket", ref _market);
            TryGetValue(message, "ConfigNewsCategory", ref _category);
            if (TryGetValue(message, "bingKey", ref _key))
            {
                InitTimer();
            }
        }

        private NewsModel NewsFromJsonObject(JsonObject json)
        {
            string thumbnail = json.GetNamedObject("image", null)?.GetNamedObject("thumbnail").GetNamedString("contentUrl");
            return new NewsModel(json.GetNamedString("name"), json.GetNamedString("url"), thumbnail);
        }

        private async void FetchNews()
        {
            JsonObject json = await new HttpHelper(BuildRequest()).TryGetJsonAsync();
            if (json == null)
            {
                return;
            }

            var jsonNews = json.GetNamedArray("value");
            var news = new List<NewsModel>();
            foreach (var x in jsonNews)
            {
                news.Add(NewsFromJsonObject(x.GetObject()));
            }
            NewsUpdate?.Invoke(this, new NewsUpdateEventArgs(news));
        }

        private HttpRequestMessage BuildRequest()
        {
            var uri = $"{ENDPOINT}?mkt={_market}";
            if (!String.IsNullOrEmpty(_category))
            {
                uri += $"&category={ _category.Replace(" ", "").Replace("-", "_")}";
            }
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Add("Ocp-Apim-Subscription-Key", _key);
            return req;
        }

        public EventHandler NewsUpdate;
    }
}
