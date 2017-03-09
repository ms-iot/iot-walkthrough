using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Showcase
{
    internal class HttpHelper
    {
        private HttpRequestMessage _request;

        public HttpHelper(HttpRequestMessage request)
        {
            _request = request;
        }

        public async Task<JsonObject> TryGetJsonAsync()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            try
            {
                response = await client.SendAsync(_request);
            }
            catch (Exception e)
            {
                ShowError("Sending request failed: " + e.Message);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                ShowError("Request returned status " + response.StatusCode);
                return null;
            }

            try
            {
                return JsonObject.Parse(await response.Content.ReadAsStringAsync());
            }
            catch (COMException e)
            {
                ShowError("Parsing JSON failed: " + e.Message);
                return null;
            }
        }

        private void ShowError(string e)
        {
            Debug.WriteLine(String.Format("HTTP request for {0} failed: {1}", _request.RequestUri, e));
        }
    }
}