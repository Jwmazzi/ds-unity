using System.Collections.Generic;
using System.Collections;

using Newtonsoft.Json.Linq;

using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Elevation;
using Esri.GameEngine.Map;

using UnityEngine.Networking;
using UnityEngine;

using EsriPS.Toolkits;


public class Baseline : MonoBehaviour
{
	public float Longitude;
	public float Latitude;
	public float Altitude;

	private Camera viewCamera;

	private string renderContainerName = "RenderContainer";
	private GameObject renderContainer;

	public GameObject markerPrefab;
	public string markerURL;

	void Start()
	{
		viewCamera = Camera.main;

		BuildMap();

		var gdeltFS = new FeatureService("https://services9.arcgis.com/q5uyFfTZo3LFL04P/ArcGIS/rest/services/GDELT_V2/FeatureServer/0");

        StartCoroutine(gdeltFS.RequestFeatures("1=1", SetMarkers, markerPrefab));
    }

    void BuildMap()
    {
		var viewMode = ArcGISMapType.Global;

		var arcGISMap = new ArcGISMap(viewMode);

		arcGISMap.Basemap = ArcGISBasemap.CreateLightGrayCanvas();

		arcGISMap.Elevation = new ArcGISMapElevation(new ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Elevation"));

		var arcGISMapViewComponent = gameObject.AddComponent<ArcGISMapViewComponent>();
		arcGISMapViewComponent.Position = new LatLon(Latitude, Longitude, Altitude);
		arcGISMapViewComponent.ViewMode = viewMode;

		var cameraGameObject = Camera.main.gameObject;
		cameraGameObject.AddComponent<ArcGISCameraComponent>();
		var locationComponent = cameraGameObject.AddComponent<ArcGISLocationComponent>();
		cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();
        cameraGameObject.AddComponent<ArcGISRebaseComponent>();

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
		GameObject locationMarker = Instantiate(prefab, renderContainer.transform);

		locationMarker.name = name;

        ArcGISLocationComponent location = locationMarker.AddComponent<ArcGISLocationComponent>();
		location.Position = new LatLon(lat, lon, alt);

		return locationMarker;
	}

	IEnumerator SetMarkers(string dataString, GameObject markerPrefab)
    {
		var results = JObject.Parse(dataString);
		var features = results["features"].Children();

		foreach (var f in features)
		{
			var attributes = f.SelectToken("attributes");

			// Unpack Common Properties
			var gi = attributes.SelectToken("globaleventid").ToString();

			// Actor 1
			var a1 = attributes.SelectToken("actor1name").ToString();
			var actor1name = string.Format("{0}-A1-{1}", gi, a1);
			var actor1lat = (float)attributes.SelectToken("actor1geo_lat");
			var actor1lon = (float)attributes.SelectToken("actor1geo_long");
			CreateMarker(actor1name, actor1lat, actor1lon, 0, markerPrefab);

			// Actor 2
			var a2 = attributes.SelectToken("actor2name").ToString();
			var actor2name = string.Format("{0}-A2-{1}", gi, a2);
			var actor2lat = (float)attributes.SelectToken("actor2geo_lat");
			var actor2lon = (float)attributes.SelectToken("actor2geo_long");
			CreateMarker(actor2name, actor2lat, actor2lon, 0, markerPrefab);

			yield return null;
		}
	}

}
