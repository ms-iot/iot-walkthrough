using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private ThreadPoolTimer _timer;

        public class NewsUpdateEventArgs : EventArgs
        {
            private List<NewsModel> _updatedNews;

            public NewsUpdateEventArgs(List<NewsModel> updatedNews)
            {
                _updatedNews = updatedNews;
            }

            public List<NewsModel> UpdatedNews { get { return _updatedNews; } }
        }

        public void Init()
        {
            AppServiceBridge.RequestReceived += PropertyUpdate;
            AppServiceBridge.RequestUpdate("bingKey");
        }

        private void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            args.Request.Message.TryGetValue("bingKey", out object key);
            if (key != null)
            {
                _key = (string)key;
                if (_timer == null)
                {
                    _timer = ThreadPoolTimer.CreatePeriodicTimer((ThreadPoolTimer timer) =>
                    {
                        FetchNews();
                    }, TimeSpan.FromMinutes(5));
                    FetchNews();
                }
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
            HttpClient client = new HttpClient();
            List<NewsModel> news = new List<NewsModel>();
            var response = await client.SendAsync(BuildRequest());
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Bing news returned error code " + response.StatusCode);
                return;
            }
            JsonObject json = JsonObject.Parse(await response.Content.ReadAsStringAsync());
            JsonArray jsonNews = json.GetNamedArray("value");
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
