using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface[] surfaces;

    private bool built = false;
    private int totalHits = 0;

    private void LateUpdate()
    {
        //UpdateTargets();

        //if (totalHits > 1000 && built == false)
        //{
        //    for (int i = 0; i < surfaces.Length; i++)
        //    {
        //        surfaces[i].BuildNavMesh();
        //    }

        //    built = true;
        //}
    }

    private void UpdateTargets()
    {
        var containerGameObject = GameObject.Find("RenderContainer");
        int hits = 0;

        Transform[] children = containerGameObject.transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            GameObject go = children[i].gameObject;

            NavMeshObstacle nm = go.GetComponent(typeof(NavMeshObstacle)) as NavMeshObstacle;

            if (nm == null)
            {
                hits = hits + 1;
                NavMeshObstacle newModifier = go.AddComponent(typeof(NavMeshObstacle)) as NavMeshObstacle;
                newModifier.carving = true;
            }
        }

        Debug.Log("Hits: " + hits);

        totalHits += hits;

    }

}
