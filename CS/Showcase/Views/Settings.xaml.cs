using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
                "All",
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
                "All",
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

        private CoreDispatcher _uiThreadDispatcher;

        public Settings()
        {
            this.InitializeComponent();

            _uiThreadDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            _newsRegion = new ObservableCollection<string>(REGIONS.Keys);
            _newsCategories = new ObservableCollection<string>();

            AppServiceBridge.RequestReceived += PropertyUpdate;
            AppServiceBridge.RequestUpdate("ConfigNewsRegion");
            AppServiceBridge.RequestUpdate("ConfigNewsCategory");
            AppServiceBridge.RequestUpdate("ConfigWeatherContryCode");
            AppServiceBridge.RequestUpdate("ConfigWeatherZipCode");
        }

        private async Task RunOnUi(DispatchedHandler f)
        {
            await _uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, f);
        }

        private async void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            if (message.TryGetValue("ConfigNewsRegion", out object region))
            {
                if (region == null)
                {
                    await RunOnUi(() => { Enable(NewsRegionCombo); });
                }
                else
                {
                    string regionName = null;
                    foreach (var pair in REGIONS)
                    {
                        if (pair.Value == (string)region)
                        {
                            regionName = pair.Key;
                            break;
                        }
                    }
                    await RunOnUi(async () =>
                    {
                        if (WeatherCountryFromNewsCheckbox.IsChecked.GetValueOrDefault())
                        {
                            await SetWeatherCountryCodeFromNews();
                        }
                        WeatherCountryFromNewsCheckbox.IsEnabled = true;
                        Enable(NewsRegionCombo, regionName);
                        if (!CATEGORIES.ContainsKey((string)region))
                        {
                            SetNewsCategoryAvailable(false);
                        }
                    });
                }
            }
            if (message.TryGetValue("ConfigNewsCategory", out object category))
            {
                await RunOnUi(() => {
                    if (NewsRegionCombo.SelectedValue != null && CATEGORIES.ContainsKey(REGIONS[(string)NewsRegionCombo.SelectedValue]))
                    {
                        SetNewsCategoryAvailable(true);
                        NewsCategoryCombo.SelectedItem = String.IsNullOrEmpty((string)category) ? "All" : category;
                    }
                    else
                    {
                        SetNewsCategoryAvailable(false);
                    }
                });
            }
            if (message.TryGetValue("ConfigWeatherContryCode", out object countryCode))
            {
                if (countryCode == null)
                {
                    await RunOnUi(() => { WeatherCountryTextBox.IsEnabled = true; });
                }
                else
                {
                    await RunOnUi(() =>
                    {
                        WeatherCountryTextBox.Text = (string)countryCode;
                        if ((string)countryCode == GetNewsCountryCode())
                        {
                            WeatherCountryTextBox.IsEnabled = false;
                            WeatherCountryFromNewsCheckbox.IsChecked = true;
                        }
                        else
                        {
                            WeatherCountryTextBox.IsEnabled = true;
                            WeatherCountryFromNewsCheckbox.IsChecked = false;
                        }
                    });
                }
            }
            if (message.TryGetValue("ConfigWeatherZipCode", out object zip))
            {
                await RunOnUi(() => { Enable(WeatherZipTextBox, (string)zip); });
            }
        }

        private async void RegionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _newsCategories.Clear();
            if (e.AddedItems.Count == 0)
            {
                SetNewsCategoryAvailable(false);
                return;
            }
            var region = REGIONS[(string)e.AddedItems[0]];
            var configs = new ValueSet
            {
                ["ConfigNewsRegion"] = region
            };
            if (CATEGORIES.TryGetValue(region, out string[] categories))
            {
                foreach (var category in categories)
                {
                    _newsCategories.Add(category);
                }
                SetNewsCategoryAvailable(true);
            }
            else
            {
                configs["ConfigNewsCategory"] = "";
                SetNewsCategoryAvailable(false);
            }
            WeatherCountryFromNewsCheckbox.IsEnabled = true;
            await AppServiceBridge.SendMessageAsync(configs);
        }

        private async void NewsCategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                await AppServiceBridge.SendMessageAsync(new ValueSet
                {
                    ["ConfigNewsCategory"] = (string)e.AddedItems[0]
                });
            }
        }

        private async void WeatherCountryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await AppServiceBridge.SendMessageAsync(new ValueSet
            {
                ["ConfigWeatherContryCode"] = ((TextBox)sender).Text
            });
        }

        private async void WeatherZipTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await AppServiceBridge.SendMessageAsync(new ValueSet
            {
                ["ConfigWeatherZipCode"] = ((TextBox)sender).Text
            });
        }

        private async void WeatherCountryFromNewsCheckbox_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WeatherCountryTextBox.IsEnabled = false;
            await SetWeatherCountryCodeFromNews();
        }

        private void WeatherCountryFromNewsCheckbox_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WeatherCountryTextBox.IsEnabled = true;
            WeatherCountryTextBox.Text = "";
        }

        private void Enable(ComboBox box, object selectedItem = null)
        {
            box.IsEnabled = true;
            box.PlaceholderText = "";
            box.SelectedItem = selectedItem;
        }

        private void Enable(TextBox box, string text = null)
        {
            box.IsEnabled = true;
            box.PlaceholderText = "";
            box.Text = text == null ? "" : text;
        }

        private void SetNewsCategoryAvailable(bool available)
        {
            NewsCategoryCombo.IsEnabled = available;
            NewsCategoryCombo.PlaceholderText = available ? "" : "Setting not available in this region";
        }

        private string GetCountryCode(string country)
        {
            return REGIONS[country].Split('-')[1].ToLower();
        }

        private string GetNewsCountryCode()
        {
            return GetCountryCode((string)NewsRegionCombo.SelectedItem);
        }

        private async Task SetWeatherCountryCodeFromNews()
        {
            var countryCode = GetNewsCountryCode();
            WeatherCountryTextBox.Text = countryCode;
            await AppServiceBridge.SendMessageAsync(new ValueSet
            {
                ["ConfigWeatherContryCode"] = countryCode
            });
        }
    }
}
