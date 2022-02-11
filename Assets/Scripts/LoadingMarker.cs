using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingMarker : MonoBehaviour
{
    public string globaleventid;
    public string goldsteinscale;
    public string category;
    public string summary;
    public string title;


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
        GameObject categoryGO = GameObject.Find("Category");
        TextMeshProUGUI categoryTMP = categoryGO.GetComponent<TextMeshProUGUI>();
        categoryTMP.text = category;

        GameObject titleGO = GameObject.Find("Title");
        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.text = title;

        GameObject summaryGO = GameObject.Find("Summary");
        TextMeshProUGUI summaryTMP = summaryGO.GetComponent<TextMeshProUGUI>();
        summaryTMP.text = summary;
    }

}
