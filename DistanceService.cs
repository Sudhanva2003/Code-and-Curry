using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Code_Curry.Services
{
    public class DistanceService
    {
        private readonly string _apiKey;

        // Constructor to get the API key from appsettings.json
        public DistanceService(IConfiguration configuration)
        {
            _apiKey = configuration.GetValue<string>("GoogleMapsApi:ApiKey");
        }

        public async Task<int> GetDistanceAsync(string address1, string address2)
        {
            string apiUrl = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={Uri.EscapeDataString(address1)}&destinations={Uri.EscapeDataString(address2)}&key={_apiKey}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(responseBody);

                // Extract the distance in meters from the response
                var distanceInMeters = jsonResponse["rows"][0]["elements"][0]["distance"]["value"].Value<int>();

                // Convert meters to kilometers and round to the nearest integer
                int distanceInKilometers = (int)Math.Round(distanceInMeters / 1000.0);

                return distanceInKilometers;
            }
        }
    }
}