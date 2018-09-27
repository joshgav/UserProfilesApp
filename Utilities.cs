using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace ProfilesApp
{
    public static class Utilities
    {
        public static bool IsAzure()
        {
            bool isAzure = false;
            var response = QueryIMDS("/instance");
            if (!String.IsNullOrEmpty(response)) {
                isAzure = true;
            }
            return isAzure;
        }

        private static string QueryIMDS(string path)
        {
            const string api_version = "2018-04-02";
            const string ip_address = "169.254.169.254";

            string imdsUri = string.Format(
              "http://{0}/metadata{1}?api-version={2}",
              ip_address, path, api_version);

            string jsonResult = string.Empty;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Metadata", "True");
                httpClient.Timeout = TimeSpan.FromSeconds(3);
                try
                {
                    jsonResult = httpClient.GetStringAsync(imdsUri).Result;
                }
                catch (AggregateException ex)
                {
                    // handle response failures
                    Console.WriteLine("IMDS request failed: " + ex.InnerException.Message);
                }
            }
            return jsonResult;
        }
    }
}
