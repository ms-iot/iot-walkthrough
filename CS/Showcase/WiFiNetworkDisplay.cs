// Copyright (c) Microsoft. All rights reserved.
// Based on https://github.com/ms-iot/samples/blob/develop/WiFiConnector/CS/DisplayHelpers.cs

using System;
using System.ComponentModel;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

namespace Showcase
{
    public class WiFiNetworkDisplay : INotifyPropertyChanged
    {
        private WiFiAdapter adapter;

        public WiFiNetworkDisplay(WiFiAvailableNetwork availableNetwork, WiFiAdapter adapter)
        {
            AvailableNetwork = availableNetwork;
            this.adapter = adapter;
            UpdateWiFiImage();
            UpdateConnectivityLevel();
        }

        private void UpdateWiFiImage()
        {
            int bars = AvailableNetwork.SignalBars;
            if (AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211)
            {
                WiFiThumbnail = Char.ConvertFromUtf32(Math.Min(bars, 3) + 0xEB60);
            }
            else
            {
                if (bars < 3)
                {
                    WiFiThumbnail = Char.ConvertFromUtf32(bars + 0xE872);
                }
                else
                {
                    WiFiThumbnail = "\xE701";
                }
            }

            OnPropertyChanged("WiFiThumbnail");
        }

        public async void UpdateConnectivityLevel()
        {
            string connectivityLevel = "Not Connected";
            string connectedSsid = null;

            var connectedProfile = await adapter.NetworkAdapter.GetConnectedProfileAsync();
            if (connectedProfile != null &&
                connectedProfile.IsWlanConnectionProfile &&
                connectedProfile.WlanConnectionProfileDetails != null)
            {
                connectedSsid = connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid();
            }

            if (!string.IsNullOrEmpty(connectedSsid))
            {
                if (connectedSsid.Equals(AvailableNetwork.Ssid))
                {
                    connectivityLevel = connectedProfile.GetNetworkConnectivityLevel().ToString();
                }
            }

            ConnectivityLevel = connectivityLevel;

            OnPropertyChanged("ConnectivityLevel");
        }

        public String Ssid
        {
            get
            {
                return availableNetwork.Ssid;
            }
        }

        public String Bssid
        {
            get
            {
                return availableNetwork.Bssid;

            }
        }

        public String ChannelCenterFrequency
        {
            get
            {
                return string.Format("{0}kHz", availableNetwork.ChannelCenterFrequencyInKilohertz);
            }
        }

        public String Rssi
        {
            get
            {
                return string.Format("{0}dBm", availableNetwork.NetworkRssiInDecibelMilliwatts);
            }
        }

        public String SecuritySettings
        {
            get
            {
                return string.Format("Authentication: {0}; Encryption: {1}", availableNetwork.SecuritySettings.NetworkAuthenticationType, availableNetwork.SecuritySettings.NetworkEncryptionType);
            }
        }
        public String ConnectivityLevel
        {
            get;
            private set;
        }

        public String WiFiThumbnail
        {
            get;
            private set;
        }


        private WiFiAvailableNetwork availableNetwork;
        public WiFiAvailableNetwork AvailableNetwork
        {
            get
            {
                return availableNetwork;
            }

            private set
            {
                availableNetwork = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
