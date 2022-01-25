// /*******************************************************************************
//  * Copyright 2012-2021 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EsriPS.Toolkits
{
	public class RouteService
    {
        public string serviceURL = "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World/solve";
        // API Key
        public string apiKey = "";
        public bool isWaitingForRequestToComplete = false;

        private HttpClient client = new HttpClient();

        /// <summary>method <c>RequestRoute</c> requests route from a route service.
        /// </summary>
	/// var routeService = new RouteService();
	/// routeService.apiKey = "YOUR_APIKEY";
	/// var origin = new double[3] { -74.0118224427977, 40.7054087814595, 3000 };
        /// var destination = new double[3] { -73.5718714010298, 45.50386767847, 208.59833486937 };
        /// var results = await routeService.RequestRoute(origin, destination)
	///
        /// This function returns JObject of the route request result
        /// The client can iterate through the JObject hierarchy
        /// 
        /// var features = results["routes"].SelectToken("features");
        /// 
        /// foreach (var f in features)
        /// {
        ///     var attributes = f["attributes"];
        ///     var geometry = f["geometry"];
        ///     var paths = geometry["paths"];
        ///     foreach (var path in paths)
        ///     {
        ///         var count = 0;
        ///         foreach (var point in path)
        ///         {
        ///             //Debug.Log(point);
        ///             var x = double.Parse(point[0].ToString());
        ///             var y = double.Parse(point[1].ToString());
        ///             count++;
        ///         }
        ///     }
        /// }
        public async Task<JObject> RequestRoute(double[] origin, double[] destination)
        {
            isWaitingForRequestToComplete = true;

            var org = new JArray();
            org.Add(origin[0]);
            org.Add(origin[1]);
            org.Add(origin[2]);

            var dst = new JArray();
            dst.Add(destination[0]);
            dst.Add(destination[1]);
            dst.Add(destination[2]);

            var stopFeatures = new JArray();
            stopFeatures.Add(org);
            stopFeatures.Add(dst);

            //"-117.195677,34.056383;-117.918976,33.812092"
            string stopString = origin[0].ToString() + "," + origin[1].ToString() + ";" + destination[0].ToString() + "," + destination[1].ToString();

            /*
              var params = {
                stops: fjson,
                f: 'json',
                impedanceAttributeName: 'WalkTime',
                restrictionAttributeNames: 'Prefer: Primary Paths, Prohibit: Two Way',
                restrictUTurns: 'esriNFSBAllowBacktrack',
                directionsOutputType: 'esriDOTFeatureSets',
                directionsStyleName: 'NA Campus',
                doNotLocateOnRestrictedElements: true,
                outputLines: 'esriNAOutputLineTrueShapeWithMeasure',
                outSR: 3857,
                preserveFirstStop: true,
                preserveLastStop: true,
                returnBarriers: false,
                returnDirections: true,
                returnPolygonBarriers: false,
                returnPolylineBarriers: false,
                returnRoutes: true,
                returnStops: true,
                returnZ: true,
                startTimeIsUTC: true,
                useTimeWindows: false,
                timeWindowsAreUTC: false,
                startTime: 0,
                startTimeIsUTC: true,
                outputGeometryPrecisionUnits: 'esriMeters',
                directionsTimeAttributeName: 'WalkTime',
                directionsLengthUnits: 'esriNAUMeters'
              }
            */

            var parameters = @"{ 
                                'returnDirections': true,
                                'returnRoutes': true,
                                'returnStops': true,
                                'returnZ': true
                           }";


            JObject pObject = JObject.Parse(parameters);
            string paramString = JsonConvert.SerializeObject(pObject);

            IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("stops", stopString),
                new KeyValuePair<string, string>("token", apiKey),
                new KeyValuePair<string, string>("f", "json"),
                new KeyValuePair<string, string>("params", paramString)
            };

            HttpContent content = new FormUrlEncodedContent(payload);

            HttpResponseMessage resp = await client.PostAsync($"{serviceURL}", content);
            resp.EnsureSuccessStatusCode();
            string respBody = await resp.Content.ReadAsStringAsync();
            var results = JObject.Parse(respBody);

            isWaitingForRequestToComplete = false;

            return results;
        }
    }
}
