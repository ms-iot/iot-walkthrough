using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Showcase
{
    class BingNews
    {
        private const String ENDPOINT = "https://api.cognitive.microsoft.com/bing/v5.0/news/";

        public static NewsModel NewsFromJsonObject(JsonObject json)
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

        public async Task<List<NewsModel>> GetNews()
        {
            HttpClient client = new HttpClient();
            List<NewsModel> news = new List<NewsModel>();
            var response = await client.SendAsync(BuildRequest());
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Bing news returned error code " + response.StatusCode);
                return news;
            }
            JsonObject json = JsonObject.Parse(await response.Content.ReadAsStringAsync());
            JsonArray jsonNews = json.GetNamedArray("value");
            foreach (var x in jsonNews)
            {
                news.Add(NewsFromJsonObject(x.GetObject()));
            }
            return news;
        }

        private HttpRequestMessage BuildRequest()
        {
            Uri uri = new Uri(String.Format("{0}?category={1}&mkt={2}", ENDPOINT, "Sports", "en-US"));
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Add("Ocp-Apim-Subscription-Key", Keys.BING_API_KEY);
            return req;
        }
    }
}
