using System;

namespace Showcase
{
    public class NewsModel
    {
        public class ThumbnailModel
        {
            public String Source { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public String Name { get { return name; } }
        public String Url { get { return url; } }
        public ThumbnailModel Thumbnail { get { return thumbnail; } }

        private String name;
        private String url;
        private ThumbnailModel thumbnail;

        public NewsModel(String name, String url, ThumbnailModel thumbnail)
        {
            this.name = name;
            this.url = url;
            this.thumbnail = thumbnail;
        }
    }
}
