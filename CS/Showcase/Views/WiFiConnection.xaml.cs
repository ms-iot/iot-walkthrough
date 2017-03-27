using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Showcase
{
    public sealed partial class WiFiConnection : Page
    {
        private WiFiAdapter adapter;
        private ObservableCollection<WiFiNetworkDisplay> networks;

        public WiFiConnection()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            networks = new ObservableCollection<WiFiNetworkDisplay>();
            ResultsListView.ItemsSource = networks;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            Debug.Assert(access == WiFiAccessStatus.Allowed, "WiFi access not allowed");
            var adapters = await WiFiAdapter.FindAllAdaptersAsync();
            foreach (var i in adapters)
            {
                Debug.WriteLine("Found adapter: " + i.NetworkAdapter.NetworkAdapterId);
            }
            if (adapters.Count == 0)
            {
                ConnectionStatusText.Text = "No WiFi adapters found";
                return;
            }
            adapter = adapters[0];
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanButton.IsEnabled = false;
            ScanButton.Content = "Scanning...";
            networks.Clear();
            await adapter.ScanAsync();
            ScanButton.IsEnabled = true;
            ScanButton.Content = "Scan networks";
            foreach (var network in adapter.NetworkReport.AvailableNetworks)
            {
                networks.Add(new WiFiNetworkDisplay(network, adapter));
            }
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedNetwork = ResultsListView.SelectedItem as WiFiNetworkDisplay;
            if (selectedNetwork == null)
            {
                return;
            }

            if (selectedNetwork.AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                    selectedNetwork.AvailableNetwork.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
            {
                NetworkKeyInfo.Visibility = Visibility.Collapsed;
            }
            else
            {
                NetworkKeyInfo.Visibility = Visibility.Visible;
            }
            ConnectionControls.Visibility = Visibility.Visible;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            var selectedNetwork = ResultsListView.SelectedItem as WiFiNetworkDisplay;
            if (selectedNetwork == null || adapter == null)
            {
                ConnectionStatusText.Text = "No network selected";
                return;
            }
            WiFiReconnectionKind reconnectionKind = IsAutomaticReconnection.IsChecked.GetValueOrDefault() ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual;

            WiFiConnectionResult result;
            if (selectedNetwork.AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                    selectedNetwork.AvailableNetwork.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
            {
                result = await adapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind);
            }
            else
            {
                // Only the password potion of the credential need to be supplied
                var credential = new PasswordCredential();
                try
                {
                    credential.Password = NetworkKey.Password;
                } catch (ArgumentException)
                {
                    ConnectionStatusText.Text = "Password is invalid.";
                    return;
                }

                result = await adapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind, credential);
            }

            var requiresWebview = false;
            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                var connectedProfile = await adapter.NetworkAdapter.GetConnectedProfileAsync();
                var level = connectedProfile.GetNetworkConnectivityLevel();
                if (level == NetworkConnectivityLevel.ConstrainedInternetAccess || level == NetworkConnectivityLevel.LocalAccess)
                {
                    ConnectionStatusText.Text = string.Format("Limited access on {0}.", selectedNetwork.Ssid);
                    requiresWebview = true;
                }
                else
                {
                    ConnectionStatusText.Text = string.Format("Successfully connected to {0}.", selectedNetwork.Ssid);
                }
            }
            else
            {
                ConnectionStatusText.Text = string.Format("Could not connect to {0}. Error: {1}", selectedNetwork.Ssid, result.ConnectionStatus);
            }
            webView.Visibility = requiresWebview ? Visibility.Visible : Visibility.Collapsed;

            // Since a connection attempt was made, update the connectivity level displayed for each
            foreach (var network in networks)
            {
                network.UpdateConnectivityLevel();
            }
        }
    }
}
