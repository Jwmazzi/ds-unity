using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingMarker : MonoBehaviour
{
    public string Running;

    // Start is called before the first frame update
    void OnMouseOver()
    {
        GameObject go = transform.gameObject;

        UpdateUI(go);
        GenerateLine();
    }

    private void OnMouseExit()
    {
        //UpdateUI();
    }

    private void GenerateLine()
    {

    }

    private void UpdateUI(GameObject go)
    {
        GameObject target = GameObject.Find("CityTarget");
        TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();

        tmp.text = go.name;
    }

}
