
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
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace EsriPS.Toolkits
{
    public class FeatureService
    {
        public FeatureService(string serviceURL)
        {
            _serviceURL = serviceURL;
        }

        private string _serviceURL;

        public string apiKey = "";
        public bool isWaitingForFSToComplete = false;

        private HttpClient client = new HttpClient();

        /// <summary>method <c>RequestFeatures</c> requests features from a feature service.
        /// </summary>
        ///
        /// var results = await FeatureService.RequestFeatures(whereClause)
        /// This function returns JObject of the feature request results
        /// The client can iterate through the JObject hierarchy
        ///
        /// var features = results["features"].Children();
        /// foreach (var f in features)
        /// {
        ///     var attributes = f.SelectToken("attributes");
        ///     var name = attributes.SelectToken("name").ToString();
        ///     var geometry = f.SelectToken("geometry");
        ///     float pos_x = 0.0f;
        ///     float.TryParse(attributes.SelectToken("x").ToString(), out pos_x);
        ///     float pos_y = 0.0f;
        ///     float.TryParse(attributes.SelectToken("y").ToString(), out pos_y);
        ///     float pos_z = 0.0f;
        ///     float.TryParse(attributes.SelectToken("z").ToString(), out pos_z);
        /// }
        public async Task<JObject> RequestFeatures(string whereClause)
        {
            IEnumerable<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("where", whereClause),
                new KeyValuePair<string, string>("outFields", "*"),
                new KeyValuePair<string, string>("token", apiKey),
                new KeyValuePair<string, string>("f", "json")
            };

            HttpContent content = new FormUrlEncodedContent(queries);
            HttpResponseMessage resp = await client.PostAsync($"{_serviceURL}/query", content);

            resp.EnsureSuccessStatusCode();

            string respBody = await resp.Content.ReadAsStringAsync();
            var results = JObject.Parse(respBody);
            return results;
        }

        /// <summary>
        /// Calls the /query operation for the Feature Service with specified whereClause and return the response text to respHandler.
        /// When the goal is to asynchronously add the queried featuers to the globe, it is possible that the logic would exist as another
        /// funciton that would be best called as a Coroutine in Unity.
        /// This version of RequestFeatures would be called as follows:
        /// StartCoroutine(featureService.RequestFeaturesCR("1=1"), HandlerFunction)
        /// </summary>
        /// <param name="whereClause"></param>
        /// <param name="respHandler"></param>
        /// <returns>IEnumerator</returns>
        public IEnumerator RequestFeaturesCR(string whereClause, ResponseHandler respHandler)
        {
            WWWForm form = new WWWForm();
            form.AddField("where", whereClause);
            form.AddField("outFields", "*");
            form.AddField("f", "json");

            var queryURL = $"{_serviceURL}/query";

            using (UnityWebRequest www = UnityWebRequest.Post(queryURL, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    yield return respHandler(www.downloadHandler.text);
                }
            }
        }

        public delegate IEnumerator ResponseHandler(string responseText);

        /// <summary>method <c>UpdateFeatures</c> update features on a feature service.
        /// </summary>
        ///
        /// var results = await FeatureService.RequestFeatures(whereClause)
        /// This function returns JObject of the feature request results
        /// The client can iterate through the JObject hierarchy
        ///
        /// var features = results["features"].Children();
        /// foreach (var f in features)
        /// {
        ///     var attributes = f.SelectToken("attributes");
        ///     var name = attributes.SelectToken("name").ToString();
        ///     var geometry = f.SelectToken("geometry");
        ///     float pos_x = 0.0f;
        ///     float.TryParse(attributes.SelectToken("x").ToString(), out pos_x);
        ///     float pos_y = 0.0f;
        ///     float.TryParse(attributes.SelectToken("y").ToString(), out pos_y);
        ///     float pos_z = 0.0f;
        ///     float.TryParse(attributes.SelectToken("z").ToString(), out pos_z);
        /// }
        public async Task<JObject> UpdateFeatures(int objectId, GameObject gameObject)
        {
            isWaitingForFSToComplete = true;

            var attributes = new JObject();
            attributes.Add("OBJECTID", objectId);
            attributes.Add("x", gameObject.transform.position.x);
            attributes.Add("y", gameObject.transform.position.y);
            attributes.Add("z", gameObject.transform.position.z);

            var obj = new JObject();
            obj.Add("attributes", attributes);

            JArray adds = new JArray(obj);
            string addsString = JsonConvert.SerializeObject(adds);

            IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("updates", addsString),
                new KeyValuePair<string, string>("token", apiKey),
                new KeyValuePair<string, string>("f", "json")
            };

            HttpContent content = new FormUrlEncodedContent(payload);
            HttpResponseMessage resp = await client.PostAsync($"{_serviceURL}/applyEdits", content);
            resp.EnsureSuccessStatusCode();
            string respBody = await resp.Content.ReadAsStringAsync();
            var results = JObject.Parse(respBody);
            isWaitingForFSToComplete = false;
            return results;
        }
    }
}
