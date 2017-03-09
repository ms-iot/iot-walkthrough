using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.ApplicationModel.AppService;
using Windows.Data.Json;
using Windows.System.Threading;

namespace Showcase
{
    class BingNews
    {
        private const String ENDPOINT = "https://api.cognitive.microsoft.com/bing/v5.0/news/";
        private string _key;
        private long _keyVersion = -1;
        private object _keyVersionLock = new object();
        private ThreadPoolTimer _timer;
        private object _timerLock = new object();
        private bool _started;

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

        private void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            args.Request.Message.TryGetValue("bingKey", out object key);
            if (key != null)
            {
                lock (_keyVersionLock)
                {
                    if (args.Request.Message.TryGetValue("version", out object version))
                    {
                        var receivedVersion = (long)version;
                        if (receivedVersion >= _keyVersion)
                        {
                            _keyVersion = receivedVersion;
                        }
                    }
                    else if (_keyVersion != -1)
                    {
                        // Do nothing if we have already received a key with version information
                        // and the newer one has no version.
                        return;
                    }
                    _key = (string)key;
                }
                InitTimer();
            }
        }

        private NewsModel NewsFromJsonObject(JsonObject json)
        {
            NewsModel.ThumbnailModel thumbnail = null;
            if (json.ContainsKey("image"))
            {
                thumbnail = new NewsModel.ThumbnailModel();
                var jsonThumbnail = json.GetNamedObject("image").GetNamedObject("thumbnail");
                thumbnail.Source = jsonThumbnail.GetNamedString("contentUrl");
                thumbnail.Width = (int)jsonThumbnail.GetNamedNumber("width");
                thumbnail.Height = (int)jsonThumbnail.GetNamedNumber("height");
            }
            return new NewsModel(json.GetNamedString("name"), json.GetNamedString("url"), thumbnail);
        }

        private async void FetchNews()
        {
            JsonObject json = await new HttpHelper(BuildRequest()).GetJsonAsync();
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
            Uri uri = new Uri(String.Format("{0}?category={1}&mkt={2}", ENDPOINT, "Sports", "en-US"));
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Add("Ocp-Apim-Subscription-Key", _key);
            return req;
        }

        public EventHandler NewsUpdate;
    }
}
