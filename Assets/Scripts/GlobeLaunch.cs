using System.Collections.Generic;
using System.Collections;

using Newtonsoft.Json.Linq;

using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Elevation;
using Esri.GameEngine.Map;

using UnityEngine.Networking;
using UnityEngine;
using TMPro;

using EsriPS.Toolkits;

public class GlobeLaunch : MonoBehaviour
{
	public float Longitude;
	public float Latitude;
	public float Altitude;

	public GameObject locationPrefab;
	public string locationURL;

	private string renderContainerName = "AGSContainer";
	private GameObject renderContainer;

	private ArcGISMapViewComponent mapViewComponent;

	void Start()
	{
		BuildGlobe();

		var gdeltFS = new FeatureService(locationURL);

		StartCoroutine(gdeltFS.RequestFeatures("1=1", SetMarkers, locationPrefab));
	}

	void BuildGlobe()
	{
		var viewMode = ArcGISMapType.Global;

		var arcGISMap = new ArcGISMap(viewMode);

		arcGISMap.Basemap = ArcGISBasemap.CreateLightGrayCanvas();
		//arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap("https://services.arcgisonline.com/arcgis/rest/services/Canvas/World_Dark_Gray_Base/MapServer", "AAPKff9b6add588e4aa2a8a3d64f6973bc6bLglVRf6iyQZTPkJ13AEwP8ia5TrZ8An9v-Xn3UeGSwnmpwhPpbshMSvT1JZiYBJU");

		var arcGISMapViewComponent = gameObject.AddComponent<ArcGISMapViewComponent>();
		arcGISMapViewComponent.Position = new LatLon(Latitude, Longitude, Altitude);
		arcGISMapViewComponent.ViewMode = viewMode;
		mapViewComponent = arcGISMapViewComponent;

		var cameraGameObject = Camera.main.gameObject;
		cameraGameObject.AddComponent<ArcGISCameraComponent>();
		var locationComponent = cameraGameObject.AddComponent<ArcGISLocationComponent>();
		cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();
		//cameraGameObject.AddComponent<ArcGISRebaseComponent>();

		locationComponent.Position = new LatLon(Latitude, Longitude, Altitude);
		locationComponent.Rotation = new Rotator(0, 0, 0);

		var rendererGameObject = new GameObject(renderContainerName);
		rendererGameObject.AddComponent<ArcGISRendererComponent>();
		renderContainer = rendererGameObject;

		cameraGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);
		rendererGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);

		arcGISMapViewComponent.RendererView.Map = arcGISMap;
	}

	private GameObject CreateMarker(string name, float lat, float lon, float alt, GameObject prefab)
	{
		GameObject locationMarker = Instantiate(locationPrefab, renderContainer.transform);

		locationMarker.name = name;

		ArcGISLocationComponent location = locationMarker.AddComponent<ArcGISLocationComponent>();
		location.Position = new LatLon(lat, lon, alt);

		return locationMarker;
	}

	IEnumerator SetMarkers(string dataString, GameObject prefab)
	{
		var results = JObject.Parse(dataString);
		var features = results["features"].Children();

		int idx = 0;
		foreach (var f in features)
		{
			var attributes = f.SelectToken("attributes");

			if (idx == 0)
				Debug.Log(attributes);

			// Unpack Common Properties
			var gi        = attributes.SelectToken("globaleventid").ToString();
			var goldstien = attributes.SelectToken("goldsteinscale").ToString();
			var category  = attributes.SelectToken("category").ToString();
			var title   = attributes.SelectToken("title").ToString();

			// Actor 1
			var a1 = attributes.SelectToken("actor1name").ToString();
			var actor1name = string.Format("{0}-A1-{1}", gi, a1);
			var actor1lat = (float)attributes.SelectToken("actor1geo_lat");
			var actor1lon = (float)attributes.SelectToken("actor1geo_long");
			GameObject g1 = CreateMarker(actor1name, actor1lat, actor1lon, 0, prefab);

			g1.gameObject.GetComponent<LoadingMarker>().goldsteinscale = goldstien;
			g1.gameObject.GetComponent<LoadingMarker>().category = category;
			g1.gameObject.GetComponent<LoadingMarker>().title = title;

			//// Actor 2
			//var a2 = attributes.SelectToken("actor2name").ToString();
			//var actor2name = string.Format("{0}-A2-{1}", gi, a2);
			//var actor2lat = (float)attributes.SelectToken("actor2geo_lat");
			//var actor2lon = (float)attributes.SelectToken("actor2geo_long");
			//GameObject g2 = CreateMarker(actor2name, actor2lat, actor2lon, 0, prefab);

			idx += 1;

			yield return null;
		}
	}

	private Vector3 GenerateBezier(Vector3 start, Vector3 mid, Vector3 end, float t)
	{
		return Vector3.Lerp(Vector3.Lerp(start, mid, t), Vector3.Lerp(mid, end, t), t);
	}

	public void BuildArc(GameObject s, GameObject e)
    {
		var sLoc = s.GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;
		var eLoc = e.GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;

		float sLon = (float)sLoc.Position.Longitude;
		float sLat = (float)sLoc.Position.Latitude;
		Vector3 sV = sLoc.transform.position;

		float eLon = (float)eLoc.Position.Longitude;
		float eLat = (float)eLoc.Position.Latitude;
		Vector3 eV = eLoc.transform.position;

		float midLat = (eLat + sLat) / 2;
		float midLon = (eLon + sLon) / 2;

		var line = new GameObject("arcLine");
		line.transform.SetParent(renderContainer.transform, false);
		var lineRenderer = line.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.widthMultiplier = 10000;
		lineRenderer.positionCount = 100;

		var peak = new GameObject("arcPeak");
		var midLocation = peak.AddComponent<ArcGISLocationComponent>();
		peak.transform.SetParent(renderContainer.transform, false);
		midLocation.Position = new LatLon(midLat, midLon, 5000);

		Vector3 midV = midLocation.transform.position;

		float xDif = midV.x - sV.x;
		float yDif = midV.y - sV.y;
		float zDif = midV.z - sV.z;

		var points = new Vector3[100];
		for (int i = 0; i < points.Length; i++)
		{
			var x = GenerateBezier(sV, midV, eV, (float)((float)i / (float)points.Length));
			points[i] = x;
		}

		lineRenderer.SetPositions(points);
	}
}
