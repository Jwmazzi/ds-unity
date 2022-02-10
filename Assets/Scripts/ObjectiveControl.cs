using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                targetLight.enabled = false;
            }
        }
    }
}
