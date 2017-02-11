using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Showcase
{
    public sealed partial class WebViewPage : Page
    {
        public WebViewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string uriString = e.Parameter as string;
            if (uriString == null)
            {
                throw new ArgumentException("WebViewPage must receive a string as argument");
            }

            WebContent.Source = new Uri(uriString);
            WebContent.LoadCompleted += (object sender, NavigationEventArgs args) =>
            {
                LoadingAnimation.Visibility = Visibility.Collapsed;
            };
        }
    }
}
