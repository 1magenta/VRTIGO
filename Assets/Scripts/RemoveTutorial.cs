using UnityEngine;

public class RemoveTutorial : MonoBehaviour
{
    private GameObject tutorial;

    void Start()
    {
        // Find the tutorial object once at the start
        tutorial = GameObject.Find("Tutorial");

        if (tutorial == null)
        {
            Debug.LogWarning("Tutorial object not found in the scene.");
        }
    }

    public void DisableTutorial()
    {
        if (tutorial != null)
        {
            // Toggle the active state of the tutorial object
            tutorial.SetActive(!tutorial.activeSelf);
        }
        else
        {
            Debug.LogWarning("Tutorial object not found in the scene.");
        }
    }
}
