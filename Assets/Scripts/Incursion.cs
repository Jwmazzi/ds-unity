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
using UnityEngine.AI;

public class Incursion : MonoBehaviour
{
	public float Longitude;
	public float Latitude;
	public float Altitude;

	public GameObject cameraPrefab;
	public string cameraURL;

	public GameObject objectivePrefab;
	public string objectiveURL;

	public GameObject enemyPrefab;
	public string enemeyURL;

	public GameObject firePrefab;
	public string fireURL;

	public GameObject barrierPrefab;

	private Camera viewCamera;

	private string targetCity;

	private string renderContainerName = "RenderContainer";
	private GameObject renderContainer;

	private List<string> adjustedObjects = new List<string>();

	private void Start()
	{
		viewCamera = Camera.main;

		targetCity = Manager.Instance.City;

		BuildMap();

        var cameraFS = new FeatureService($"{cameraURL}\\query");
        StartCoroutine(cameraFS.RequestFeatures("1=1", SetMarkers, cameraPrefab));

        var enemyFS = new FeatureService($"{enemeyURL}\\query");
		StartCoroutine(enemyFS.RequestFeatures("1=1", HandleEnemeies, enemyPrefab));

		var objectiveFS = new FeatureService($"{objectiveURL}\\query");
		StartCoroutine(objectiveFS.RequestFeatures("1=1", HandleObjectvies, objectivePrefab));
	}

    private void Update()
    {
		if (CheckLoad() < 1000)
            return;

		var manager = Manager.Instance;

        if (manager.Objectives.Count > 0)
        {
            foreach (GameObject enemy in Manager.Instance.Enemies)
            {
                NavMeshAgent enemyAgent = enemy.GetComponent(typeof(NavMeshAgent)) as NavMeshAgent;

                if (enemyAgent != null)
                {
					enemyAgent.enabled = true;

                    if (enemyAgent.hasPath == false)
                        enemyAgent.SetDestination(Manager.Instance.Objectives[0].transform.position);
                }
            }
        }

		if (Input.GetMouseButtonDown(0))
        {
			RaycastHit hit;
			Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);

			// TODO - Cooldown

			if (Physics.Raycast(ray, out hit))
            {
				Instantiate(barrierPrefab, hit.point, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)), renderContainer.transform);
            }
        }

    }

    private void BuildMap()
	{

		var viewMode = Esri.GameEngine.Map.ArcGISMapType.Local;
		var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(viewMode);

		arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap("https://www.arcgis.com/sharing/rest/content/items/8d569fbc4dc34f68abae8d72178cee05/data", "");

		//var extentCenter = new ArcGISPosition(Longitude, Latitude, Altitude, Esri.ArcGISRuntime.Geometry.SpatialReference.WGS84());
		//var extent = new ArcGISExtentCircle(extentCenter, 100000);
		//try
		//{
		//	arcGISMap.ClippingArea = extent;
		//}
		//catch (Exception e)
		//{
		//	Debug.Log(e.Message);
		//}

		var buildings = new Esri.GameEngine.Layers.ArcGIS3DModelLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Buildings", 1.0f, true, "");
		arcGISMap.Layers.Add(buildings);

		var arcGISMapViewComponent = gameObject.AddComponent<ArcGISMapViewComponent>();

		arcGISMapViewComponent.Position = new LatLon(Latitude, Longitude, 15);
		arcGISMapViewComponent.ViewMode = viewMode;

		var cameraGameObject = Camera.main.gameObject;
		var cameraComponent = cameraGameObject.AddComponent<ArcGISCameraComponent>();
		var locationComponent = cameraGameObject.AddComponent<ArcGISLocationComponent>();
        cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();
        //cameraGameObject.AddComponent<ArcGISRebaseComponent>();

        locationComponent.Position = new LatLon(Latitude, Longitude, Altitude);
		locationComponent.Rotation = new Rotator(0, 65, 0);

		var rendererGameObject = new GameObject(renderContainerName);
		rendererGameObject.AddComponent<ArcGISRendererComponent>();
		renderContainer = rendererGameObject;

		cameraGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);
		rendererGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);

		arcGISMapViewComponent.RendererView.Map = arcGISMap;
	}

	private int CheckLoad()
    {
		var containerGameObject = GameObject.Find(renderContainerName);

		Transform[] children = containerGameObject.transform.GetComponentsInChildren<Transform>();

		return children.Length;
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
			var geom = f.SelectToken("geometry");

			var x = float.Parse(geom.SelectToken("x").ToString());
			var y = float.Parse(geom.SelectToken("y").ToString());

			CreateMarker("Camera", y, x, 25, markerPrefab);

			yield return null;
		}
	}

	IEnumerator HandleEnemeies(string dataString, GameObject enemyPrefab)
    {
		var results = JObject.Parse(dataString);
		var features = results["features"].Children();

		foreach (var f in features)
		{
			var geom = f.SelectToken("geometry");

			var x = float.Parse(geom.SelectToken("x").ToString());
			var y = float.Parse(geom.SelectToken("y").ToString());

			GameObject enemy = CreateMarker("Enemy", y, x, 0, enemyPrefab);

            //NavMeshAgent enemyAgent = enemy.GetComponent(typeof(NavMeshAgent)) as NavMeshAgent;

            //if (enemyAgent != null)
            //    enemyAgent.updatePosition = false;

            Manager.Instance.Enemies.Add(enemy);

			yield return null;
		}
	}

	IEnumerator HandleObjectvies(string dataString, GameObject objectivePrefab)
    {
		var results = JObject.Parse(dataString);
		var features = results["features"].Children();

		foreach (var f in features)
		{
			var geom = f.SelectToken("geometry");

			var x = float.Parse(geom.SelectToken("x").ToString());
			var y = float.Parse(geom.SelectToken("y").ToString());

			GameObject objective = CreateMarker("Objective", y, x, 25, objectivePrefab);

			Manager.Instance.Objectives.Add(objective);

			yield return null;
		}
	}

	private void HandleSceneMeshes()
    {
		Transform[] allChildren = renderContainer.transform.GetComponentsInChildren<Transform>();

		for (int i = 0; i < allChildren.Length; i++)
        {
			GameObject go = allChildren[i].gameObject;
			MeshCollider mc = go.GetComponent(typeof(MeshCollider)) as MeshCollider;

			if (mc == null)
			{
				MeshCollider newMeshCollider = go.AddComponent(typeof(MeshCollider)) as MeshCollider;
				newMeshCollider.convex = true;
			}
		}
	}

}
