//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.IO;
//using System;

//public class NystagmusTaskManager : MonoBehaviour
//{
//    [Header("Task Settings")]
//    public string nextSceneName = "TestofSkew"; // Next scene in sequence

//    [Header("UI References")]
//    public GameObject completionUI;
//    public TMPro.TextMeshProUGUI progressText;

//    // References to existing scripts
//    private NystagmusDisplayRot displayRotation;
//    private MoveBetweenTwoTransforms movementScript;

//    // Task state
//    private float startTime;
//    private bool taskCompleted = false;
//    private bool testPhaseStarted = false;

//    void Start()
//    {
//        // Find existing components
//        displayRotation = FindObjectOfType<NystagmusDisplayRot>();
//        movementScript = FindObjectOfType<MoveBetweenTwoTransforms>();

//        if (displayRotation == null || movementScript == null)
//        {
//            Debug.LogError("Required components not found!");
//            return;
//        }

//        InitializeTask();
//    }

//    void InitializeTask()
//    {
//        startTime = Time.time;

//        // Enable the existing systems
//        if (displayRotation != null)
//        {
//            displayRotation.enabled = true;
//        }

//        if (movementScript != null)
//        {
//            movementScript.enabled = true;
//        }

//        //Debug.Log("Nystagmus task started");
//    }

//    void Update()
//    {
//        if (taskCompleted) return;

//        // Update progress display
//        if (progressText != null)
//        {
//            int currentCycle = movementScript != null ? movementScript.counter : 0;
//            progressText.text = $"Nystagmus Test\nCompleted cycles: {currentCycle}/3";
//        }

//        // Check completion based on movement script counter only
//        if (movementScript != null && movementScript.counter >= 3)
//        {
//            CompleteTask();
//        }
//    }

//    void CompleteTask()
//    {
//        if (taskCompleted) return;

//        taskCompleted = true;

//        Debug.Log("Nystagmus task completed");

//        // Disable movement
//        if (movementScript != null)
//        {
//            movementScript.enabled = false;
//        }

//        // Show completion message briefly, then transition
//        StartCoroutine(TransitionToNextTask());
//    }

//    System.Collections.IEnumerator TransitionToNextTask()
//    {
//        if (completionUI != null)
//        {
//            completionUI.SetActive(true);
//        }

//        if (progressText != null)
//        {
//            progressText.text = "Nystagmus Test Complete!\nTransitioning to next test...";
//        }

//        yield return new WaitForSeconds(3f);

//        // Load next scene
//        //if (!string.IsNullOrEmpty(nextSceneName))
//        //{
//        //    SceneManager.LoadScene(nextSceneName);
//        //}
//        //else
//        //{
//        //    Debug.Log("All tests completed!");
//        //}
//        GlobalTestManager.Instance.CompleteCurrentTest();
//    }
//}


using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class NystagmusTaskManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject completionUI;
    public TMPro.TextMeshProUGUI progressText;

    // References to existing scripts
    private NystagmusDisplayRot displayRotation;
    private MoveBetweenTwoTransforms movementScript;

    // Task state
    private float startTime;
    private bool taskCompleted = false;
    private bool testPhaseStarted = false;

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

        // Enable the display rotation system (this will handle the introduction sequence)
        if (displayRotation != null)
        {
            displayRotation.enabled = true;
        }

        Debug.Log("Nystagmus task manager initialized - waiting for test to begin");
    }

    void Update()
    {
        if (taskCompleted) return;

        // Check if the test phase has started
        if (!testPhaseStarted && displayRotation != null)
        {
            testPhaseStarted = true;
            Debug.Log("Nystagmus test phase has begun - now monitoring for completion");
        }

        // Only check completion after test has actually started
        if (testPhaseStarted && movementScript != null && movementScript.counter >= 3)
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

        // Use global manager for scene transition
        GlobalTestManager.Instance.CompleteCurrentTest();
    }

    // Optional: Method to get detailed progress information
    public string GetDetailedProgress()
    {
        if (!testPhaseStarted)
        {
            return "Introduction sequence in progress";
        }

        if (movementScript != null)
        {
            int cycles = movementScript.counter;
            string phase = movementScript.phase;
            return $"Test active - Phase: {phase}, Cycles: {cycles}/3";
        }

        return "Test in progress";
    }

    // Optional: Method to check if introduction is complete
    public bool IsIntroductionComplete()
    {
        return testPhaseStarted;
    }
}