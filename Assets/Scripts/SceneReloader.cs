using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("SceneReloaded", 0) == 0)
        {
            Invoke("ReloadScene", 1.0f); // Call ReloadScene after 1 second
        }
        else
        {
            PlayerPrefs.SetInt("SceneReloaded", 0);
        }
    }

    void ReloadScene()
    {
        PlayerPrefs.SetInt("SceneReloaded", 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
