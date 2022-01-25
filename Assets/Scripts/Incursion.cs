using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Location;
using Esri.Unity;
using System;
using EsriPS.Toolkits;
using Newtonsoft.Json.Linq;


public class Incursion : MonoBehaviour
{
	public float Longitude;
	public float Latitude;
	public float Altitude;

	public GameObject markerPrefab;

	private Camera viewCamera;

	private string renderContainerName = "RenderContainer";
	private GameObject renderContainer;

	void Start()
	{
		viewCamera = Camera.main;

		//viewCamera.orthographic = true;

		BuildMap();

		var gdeltFS = new FeatureService("https://services1.arcgis.com/BteRGjYsGtVEXzaX/ArcGIS/rest/services/NYC_OSM_Cameras/FeatureServer/0/query");

		StartCoroutine(gdeltFS.RequestFeaturesCR("1=1", SetMarkers));
	}

	void BuildMap()
	{

		var viewMode = Esri.GameEngine.Map.ArcGISMapType.Local;
		var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(viewMode);

		arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap("https://www.arcgis.com/sharing/rest/content/items/8d569fbc4dc34f68abae8d72178cee05/data", "");

		//arcGISMap.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Elevation", ""));

		var extentCenter = new ArcGISPosition(Longitude, Latitude, Altitude, Esri.ArcGISRuntime.Geometry.SpatialReference.WGS84());
		var extent = new ArcGISExtentCircle(extentCenter, 100000);
		try
		{
			arcGISMap.ClippingArea = extent;
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
		}

		var buildings = new Esri.GameEngine.Layers.ArcGIS3DModelLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Buildings", 1.0f, true, "");
		arcGISMap.Layers.Add(buildings);

		var arcGISMapViewComponent = gameObject.AddComponent<ArcGISMapViewComponent>();

		arcGISMapViewComponent.Position = new LatLon(Latitude, Longitude, 0);
		arcGISMapViewComponent.ViewMode = viewMode;

		var cameraGameObject = Camera.main.gameObject;
		var cameraComponent = cameraGameObject.AddComponent<ArcGISCameraComponent>();
		var locationComponent = cameraGameObject.AddComponent<ArcGISLocationComponent>();
		cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();
		//cameraGameObject.AddComponent<ArcGISRebaseComponent>();

		locationComponent.Position = new LatLon(Latitude, Longitude, Altitude);
		locationComponent.Rotation = new Rotator(65, 68, 0);

		var rendererGameObject = new GameObject(renderContainerName);
		rendererGameObject.AddComponent<ArcGISRendererComponent>();
		renderContainer = rendererGameObject;

		cameraGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);
		rendererGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);

		arcGISMapViewComponent.RendererView.Map = arcGISMap;
	}

	private GameObject CreateMarker(float lat, float lon, float alt)
	{
		GameObject locationMarker = Instantiate(markerPrefab, renderContainer.transform);

		locationMarker.name = "Spotter";

		ArcGISLocationComponent location = locationMarker.AddComponent<ArcGISLocationComponent>();
		location.Position = new LatLon(lat, lon, 100);

		return locationMarker;
	}


	IEnumerator SetMarkers(string dataString)
	{
		var results = JObject.Parse(dataString);
		var features = results["features"].Children();

		foreach (var f in features)
		{
			var geom = f.SelectToken("geometry");

			var x = float.Parse(geom.SelectToken("x").ToString());
			var y = float.Parse(geom.SelectToken("y").ToString());

			CreateMarker(x, y, 0);

			yield return null;
		}
	}


}
