// 2. Create StartSceneManager.cs - For the start scene
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    

    void Start()
    {

    }

    void Update()
    {
        // Check for controller input
        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Two))
        {
            Debug.Log("Start Test!");
            OnStartTests();
        }
    }


    void OnStartTests()
    {
        // Find or create GlobalTestManager
        GlobalTestManager manager = FindObjectOfType<GlobalTestManager>();
        if (manager == null)
        {
            GameObject managerGO = new GameObject("GlobalTestManager");
            manager = managerGO.AddComponent<GlobalTestManager>();
        }

        Debug.Log("Starting tests via controller input");

        // Start the test sequence
        manager.StartTests();
    }
}