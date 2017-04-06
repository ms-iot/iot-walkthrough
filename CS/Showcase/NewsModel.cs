namespace Showcase
{
    public class NewsModel
    {
        public string Name { get { return name; } }
        public string Url { get { return url; } }
        public string Thumbnail { get { return thumbnail; } }

        private string name;
        private string url;
        private string thumbnail;

        public NewsModel(string name, string url, string thumbnail)
        {
            this.name = name;
            this.url = url;
            this.thumbnail = thumbnail;
        }
    }
}
