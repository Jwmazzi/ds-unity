using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveControl : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Provided the Scene has been loaded, if an Enemy hits this Objective, signal to the client that this Objective
        // has been destroyed. TODO - Push update back to AGOL for Dashboard.
        if (Manager.Instance.IncursionReady)
        {
            if (other.tag == "Enemy")
            {
                var targetPS = transform.gameObject.GetComponentInChildren<ParticleSystem>();
                var main = targetPS.main;
                main.startColor = Color.red;

                var targetLight = transform.gameObject.GetComponentInChildren<Light>();
                targetLight.color = Color.red;

                var tmps = transform.gameObject.GetComponentsInChildren<TextMeshProUGUI>();


                foreach (var tmp in tmps)
                {
                    if (tmp.name == "ObjectiveDescription")
                        tmp.text = $"This objective has been captured. There are {Manager.Instance.Objectives.Count - 1} objectives remaining!";
                }

                var enemyPS = other.transform.gameObject.GetComponentInChildren<ParticleSystem>();
                enemyPS.Stop();
            }
        }
    }
}
