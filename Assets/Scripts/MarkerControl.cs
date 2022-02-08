using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MarkerControl : MonoBehaviour
{
    void OnMouseOver()
    {
        DisplayMarkerInfo(true);
    }

    private void OnMouseExit()
    {
        DisplayMarkerInfo(false);
    }

    private void DisplayMarkerInfo(bool enabled)
    {
        Component[] componentList = transform.gameObject.GetComponentsInChildren(typeof(TextMeshProUGUI), true);

        foreach (var component in componentList)
        {
            var tmp = component as TextMeshProUGUI;

            tmp.enabled = enabled;
            //tmp.transform.LookAt(Camera.main.transform.position);
        }
    }
}
