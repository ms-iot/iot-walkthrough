using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace Showcase
{   public sealed partial class Settings : Page
    {
        public ObservableCollection<String> NewsRegions { get { return _newsRegion; } }
        public ObservableCollection<String> NewsCategories { get { return _newsCategories; } }

        private ObservableCollection<String> _newsRegion;
        private ObservableCollection<String> _newsCategories;

        // Documentation is available at https://msdn.microsoft.com/en-us/library/dn783426.aspx .
        private readonly Dictionary<string, string> REGIONS = new Dictionary<string, string>
        {
            ["Argentina - Spanish"] = "es-AR",
            ["Australia - English"] = "en-AU",
            ["Austria - German"] = "de-AT",
            ["Belgium - Dutch"] = "nl-BE",
            ["Belgium - French"] = "fr-BE",
            ["Brazil - Portuguese"] = "pt-BR",
            ["Canada - English"] = "en-CA",
            ["Canada - French"] = "fr-CA",
            ["Chile - Spanish"] = "es-CL",
            ["Denmark - Danish"] = "da-DK",
            ["Finland - Finnish"] = "fi-FI",
            ["France - French"] = "fr-FR",
            ["Germany - German"] = "de-DE",
            ["Hong Kong SAR Traditional - Chinese"] = "zh-HK",
            ["India - English"] = "en-IN",
            ["Indonesia - English"] = "en-ID",
            ["Ireland - English"] = "en-IE",
            ["Italy - Italian"] = "it-IT",
            ["Japan - Japanese"] = "ja-JP",
            ["Korea - Korean"] = "ko-KR",
            ["Malaysia - English"] = "en-MY",
            ["Mexico - Spanish"] = "es-MX",
            ["Netherlands - Dutch"] = "nl-NL",
            ["New Zealand - English"] = "en-NZ",
            ["Norway - Norwegian"] = "no-NO",
            ["People's republic of China - Chinese"] = "zh-CN",
            ["Poland - Polish"] = "pl-PL",
            ["Portugal - Portuguese"] = "pt-PT",
            ["Republic of the Philippines - English"] = "en-PH",
            ["Russia - Russian"] = "ru-RU",
            ["Saudi Arabia - Arabic"] = "ar-SA",
            ["South Africa - English"] = "en-ZA",
            ["Spain - Spanish"] = "es-ES",
            ["Sweden - Swedish"] = "sv-SE",
            ["Switzerland - French"] = "fr-CH",
            ["Switzerland - German"] = "de-CH",
            ["Taiwan Traditional - Chinese"] = "zh-TW",
            ["Turkey - Turkish"] = "tr-TR",
            ["United Kingdom - English"] = "en-GB",
            ["United States - English"] = "en-US",
            ["United States - Spanish"] = "es-US",
        };

        // Remove spaces and replace - with _ to get the category parameter name for the API call.
        // Documentation is available at https://msdn.microsoft.com/en-us/library/dn760793.aspx#category .
        private readonly Dictionary<string, string[]> CATEGORIES = new Dictionary<string, string[]>
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

            _newsRegion = new ObservableCollection<string>(REGIONS.Keys);
            _newsCategories = new ObservableCollection<string>();
        }

        private void RegionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _newsCategories.Clear();
            if (CATEGORIES.TryGetValue(REGIONS[(string)e.AddedItems[0]], out string[] categories))
            {
                foreach (var category in categories)
                {
                    _newsCategories.Add(category);
                }
                NewsCategoriesCombo.IsEnabled = true;
                NewsCategoriesTextBlock.Text = "Category";
            }
            else
            {
                NewsCategoriesCombo.IsEnabled = false;
                NewsCategoriesTextBlock.Text = "Category setting not available in this region";
            }
        }
    }
}
