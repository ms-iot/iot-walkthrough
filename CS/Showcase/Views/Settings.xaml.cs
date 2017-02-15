using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace Showcase
{   public sealed partial class Settings : Page
    {
        public ObservableCollection<String> NewsCategories { get { return newsCategories; } }

        private ObservableCollection<String> newsCategories;
        // Remove spaces and replace - with _ to get the category parameter name for the API call
        private readonly Dictionary<string, string[]> categories = new Dictionary<string, string[]>
        {
            ["en-US"] = new string[]
            {
                "Business",
                "Entertainment",
                "Entertainment - Movie And TV",
                "Entertainment - Music",
                "Health",
                "Politics",
                "Science And Technology",
                "Technology",
                "Science",
                "Sports",
                "Sports - Golf",
                "Sports - MLB",
                "Sports - NBA",
                "Sports - NFL",
                "Sports - NHL",
                "Sports - Soccer",
                "Sports - Tennis",
                "Sports - CFB",
                "Sports - CBB",
                "US",
                "US - Northeast",
                "US - South",
                "US - Midwest",
                "US - West",
                "World",
                "World - Africa",
                "World - Americas",
                "World - Asia",
                "World - Europe",
                "World - Middle East",
            },
            ["en-GB"] = new string[]
            {
                "Business",
                "Entertainment",
                "Health",
                "Politics",
                "Science And Technology",
                "Sports",
                "UK",
                "World",
            },
        };

        public Settings()
        {
            this.InitializeComponent();
            newsCategories = new ObservableCollection<string>(categories["en-US"]);
        }
    }
}
