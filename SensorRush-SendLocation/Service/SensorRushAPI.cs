using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Globalization;
using System.Net;
using SensorRush_SendLocation.Model;


namespace SensorRush_SendLocation.Service
{
    public class SensorRushAPI
    {
        /// <summary>
        /// Sensor Rush API Key (This is user specific - you do not have one visit: http://www.sensorrush.com)
        /// </summary>
        private const string apiKey = "123456";
        
        /// <summary>
        /// Sensor Rush base Insert URL
        /// </summary>
        private static string insertURLWebApi = "http://sensorrush.net/demo/"; //for demo pursposes we use http - real applications can use https

        /// <summary>
        /// Sends Location data to the Sensor Rush API
        /// </summary>
        /// <param name="locationSourceName">Name of Location source</param>
        /// <param name="locationData">A list of Location data</param>
        /// <returns>An HttpResponseMessage<</returns>
        public static async Task<HttpResponseMessage> SendLocationViaApi(string locationSourceName, List<MyLocation.LocationData> locationData, System.Threading.CancellationToken ct)
        {
            //serialise the list of data into a JSON string
            var jsonData = JsonConvert.SerializeObject(locationData, Formatting.Indented, new JsonSerializerSettings());
            //need to convert into a string content - ready for sending
            StringContent dataToSend = new StringContent("'" + jsonData + "'", System.Text.Encoding.UTF8, "application/json"); //need "'" arond the json string so that it can accept json properly

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(String.Format(insertURLWebApi + "{0}/{1}/Insert", apiKey, locationSourceName)); //Insert api
            HttpResponseMessage response = new HttpResponseMessage();

            //code snippet from http://stackoverflow.com/a/30852496
            try
            {
                response = await client.PostAsync("", dataToSend, ct); //this sends JSON string via HTTP POST with cancellation token
            }
            catch (OperationCanceledException cex)
            {
                if (response == null)
                {
                    response = new HttpResponseMessage();
                    response.StatusCode = HttpStatusCode.NotImplemented;
                    response.ReasonPhrase = string.Format("API Insert cancelled by user", cex);
                }
            }
            catch (Exception ex)
            {
                if (response == null)
                {
                    response = new HttpResponseMessage();
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.ReasonPhrase = string.Format("Sent Failed {0}", ex);
                }

            }

            return response;
        }

    }
}