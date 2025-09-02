using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class HeadStabilityTaskManager : MonoBehaviour
{
    [Header("Task Settings")]
    public string nextSceneName = "TestofSkew"; // Next scene in sequence

    [Header("UI References")]
    public GameObject completionUI;
    public TMPro.TextMeshProUGUI progressText;

    // References to existing script
    private HeadStability_BgChanger backgroundChanger;

    // Task state tracking
    private bool taskCompleted = false;
    private float phaseStartTime;

    void Start()
    {
        // Find existing component
        backgroundChanger = FindObjectOfType<HeadStability_BgChanger>();

        if (backgroundChanger == null)
        {
            Debug.LogError("CameraBackgroundChanger component not found!");
            return;
        }

        InitializeTask();
    }

    void InitializeTask()
    {
        phaseStartTime = Time.time;

        // Enable the background changer system
        if (backgroundChanger != null)
        {
            backgroundChanger.enabled = true;
        }

        Debug.Log("HeadStability task started");
    }

    void Update()
    {
        if (taskCompleted || backgroundChanger == null) return;

        // Update progress based on current state
        UpdateProgressDisplay();

        // Check if background changer has completed all phases
        if (!backgroundChanger.enabled && backgroundChanger.smiley != null && !backgroundChanger.smiley.activeSelf)
        {
            CompleteTask();
        }
    }

    void UpdateProgressDisplay()
    {
        if (progressText == null || backgroundChanger == null) return;

        string currentPhase = backgroundChanger.stateString;
        float elapsedTime = Time.time - phaseStartTime;

        string phaseDescription = currentPhase switch
        {
            "solid" => "Phase 1: Solid Background",
            "passthrough" => "Phase 2: Passthrough View",
            "skybox" => "Phase 3: Skybox Environment",
            _ => "Head Stability Test"
        };

        progressText.text = $"{phaseDescription}\nElapsed: {elapsedTime:F0}s";
    }

    void CompleteTask()
    {
        if (taskCompleted) return;

        taskCompleted = true;

        Debug.Log("HeadStability task completed");

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
            progressText.text = "Head Stability Test Complete!\nTransitioning to next test...";
        }

        yield return new WaitForSeconds(3f);

        //// Load next scene
        //if (!string.IsNullOrEmpty(nextSceneName))
        //{
        //    SceneManager.LoadScene(nextSceneName);
        //}
        //else
        //{
        //    Debug.Log("All tests completed!");
        //}
        GlobalTestManager.Instance.CompleteCurrentTest();
    }
}