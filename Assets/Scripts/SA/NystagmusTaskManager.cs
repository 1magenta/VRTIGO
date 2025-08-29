using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class NystagmusTaskManager : MonoBehaviour
{
    [Header("Task Settings")]
    public string nextSceneName = "TestofSkew"; // Next scene in sequence

    [Header("UI References")]
    public GameObject completionUI;
    public TMPro.TextMeshProUGUI progressText;

    // References to existing scripts
    private NystagmusDisplayRot displayRotation;
    private MoveBetweenTwoTransforms movementScript;

    // Task state
    private float startTime;
    private bool taskCompleted = false;

    void Start()
    {
        // Find existing components
        displayRotation = FindObjectOfType<NystagmusDisplayRot>();
        movementScript = FindObjectOfType<MoveBetweenTwoTransforms>();

        if (displayRotation == null || movementScript == null)
        {
            Debug.LogError("Required components not found!");
            return;
        }

        InitializeTask();
    }

    void InitializeTask()
    {
        startTime = Time.time;

        // Enable the existing systems
        if (displayRotation != null)
        {
            displayRotation.enabled = true;
        }

        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        Debug.Log("Nystagmus task started");
    }

    void Update()
    {
        if (taskCompleted) return;

        // Update progress display
        if (progressText != null)
        {
            int currentCycle = movementScript != null ? movementScript.counter : 0;
            progressText.text = $"Nystagmus Test\nCompleted cycles: {currentCycle}/3";
        }

        // Check completion based on movement script counter only
        if (movementScript != null && movementScript.counter >= 3)
        {
            CompleteTask();
        }
    }

    void CompleteTask()
    {
        if (taskCompleted) return;

        taskCompleted = true;

        Debug.Log("Nystagmus task completed");

        // Disable movement
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // Show completion message briefly, then transition
        StartCoroutine(TransitionToNextTask());
    }

    System.Collections.IEnumerator TransitionToNextTask()
    {
        if (completionUI != null)
        {
            completionUI.SetActive(true);
        }

        if (progressText != null)
        {
            progressText.text = "Nystagmus Test Complete!\nTransitioning to next test...";
        }

        yield return new WaitForSeconds(3f);

        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("All tests completed!");
        }
    }
}