using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Showcase
{
    class OneDriveItemController
    {
        private readonly string clientId = Keys.ONE_DRIVE_CLIENT_ID;
        private readonly string RETURN_URL = "https://login.live.com/oauth20_desktop.srf";
        private readonly string BASE_URL = "https://api.onedrive.com/v1.0";
        private readonly string[] SCOPES = new string[] { "onedrive.readonly", "wl.signin", "offline_access" };

        private IOneDriveClient _oneDriveClient;
        public IOneDriveClient Client { get { return _oneDriveClient; } }

        public async Task InitAsync()
        {
            var authProvider = new MsaAuthenticationProvider(this.clientId, RETURN_URL, SCOPES, new CredentialVault(this.clientId));
            try
            {
                await authProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
            }
            catch (ServiceException e)
            {
                Debug.WriteLine("OneDrive auth error: " + e);
                throw new Exception($"OneDrive login failed: {e.Message}", e);
            }
            _oneDriveClient = new OneDriveClient(BASE_URL, authProvider);
        }

        /// <summary>
        /// Get photos for a directory ID.
        /// </summary>
        /// <param name="id">ID of the parent item or null for the root.</param>
        /// <returns>Photos in the specified item ID.</returns>
        public async Task<List<OneDriveItemModel>> GetImagesAsync(string id)
        {
            List<OneDriveItemModel> results = new List<OneDriveItemModel>();
            if (_oneDriveClient == null)
            {
                return results;
            }

            IItemRequestBuilder folder;
            Item item;
            try
            {
                folder = string.IsNullOrEmpty(id) ? _oneDriveClient.Drive.Root : _oneDriveClient.Drive.Items[id];
                item = await folder.Request().Expand("children").GetAsync();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to get OneDrive folder: {e.Message}", e);
            }

            if (item.Children == null)
            {
                return results;
            }

            try
            {
                var items = item.Children.CurrentPage.Where(child => child.Image != null);
                foreach (var child in items)
                {
                    results.Add(new OneDriveItemModel(child));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to enumerate images: {e.Message}", e);
            }

            return results;
        }
    }
}
