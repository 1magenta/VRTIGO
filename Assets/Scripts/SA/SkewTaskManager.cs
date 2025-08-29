using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class SkewTaskManager : MonoBehaviour
{
    [Header("Task Settings")]
    public string nextSceneName = "TestofNystagmus"; // Next scene in sequence

    [Header("UI References")]
    public GameObject completionUI;
    public TMPro.TextMeshProUGUI progressText;

    // References to existing script
    private SkewDisplayRot skewDisplay;

    // Task state tracking
    private bool taskCompleted = false;

    void Start()
    {
        // Find existing component
        skewDisplay = FindObjectOfType<SkewDisplayRot>();

        if (skewDisplay == null)
        {
            Debug.LogError("SkewDisplayRotation component not found!");
            return;
        }

        InitializeTask();
    }

    void InitializeTask()
    {
        // Enable the skew display system
        if (skewDisplay != null)
        {
            skewDisplay.enabled = true;
        }

        Debug.Log("TestofSkew task started");
    }

    void Update()
    {
        if (taskCompleted || skewDisplay == null) return;

        // Update progress based on current phase
        UpdateProgressDisplay();

        // Check if skew test has completed (phase becomes "None")
        if (skewDisplay.phase == "None")
        {
            CompleteTask();
        }
    }

    void UpdateProgressDisplay()
    {
        if (progressText == null || skewDisplay == null) return;

        string currentPhase = skewDisplay.phase;
        int cycleCount = skewDisplay.counter;

        string phaseDescription = currentPhase switch
        {
            "Both" => "Both Objects Visible",
            "BothActive" => "Both Objects Active",
            "LeftActive" => "Left Object Only",
            "RightActive" => "Right Object Only",
            "BothAfterLeft" => "Both Objects Active",
            "BothAfterRight" => "Both Objects Active",
            "TestComplete" => "✓ TEST SEQUENCE COMPLETE",
            "None" => "Test Complete",
            _ => "Test of Skew"
        };

        if (currentPhase == "TestComplete")
        {
            progressText.text = $"Test of Skew\n{phaseDescription}\nPreparing to transition...";
        }
        else
        {
            progressText.text = $"Test of Skew\n{phaseDescription}\nCycle: {cycleCount + 1}/2";
        }
    }

    void CompleteTask()
    {
        if (taskCompleted) return;

        taskCompleted = true;

        Debug.Log("TestofSkew task completed");

        // Start transition
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
            progressText.text = "Test of Skew Complete!\nTransitioning to next test...";
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
