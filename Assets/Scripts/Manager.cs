using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    public string City;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadClick()
    {
        City = "New York City";
        // TODO - Load Starting Location, Difficult Based on GDELT Scores, Ect.
        SceneManager.LoadScene("Incursion", LoadSceneMode.Single);
    }

}
