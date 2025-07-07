using UnityEngine;
using TMPro;
using System.Collections;

public class EyeTrackingDebug : MonoBehaviour
{
    [Header("UI Debug Display")]
    public TextMeshProUGUI debugDisplay;
    //public TextMeshProUGUI linkStatusDisplay;

    [Header("Eye Tracking References")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    private float updateInterval = 0.5f; // Update every 0.5 seconds

    void Start()
    {
        StartCoroutine(ContinuousEyeTrackingCheck());
    }

    IEnumerator ContinuousEyeTrackingCheck()
    {
        while (true)
        {
            UpdateLinkStatus();
            UpdateEyeTrackingStatus();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void UpdateLinkStatus()
    {
        string linkInfo = "=== QUEST LINK STATUS ===\n";

        // Check if running through Link
        bool isLink = Application.platform == RuntimePlatform.WindowsEditor ||
                     Application.platform == RuntimePlatform.WindowsPlayer;

        linkInfo += $"Platform: {Application.platform}\n";
        linkInfo += $"Is Quest Link: {isLink}\n";

        if (OVRManager.instance != null)
        {
            linkInfo += $"OVR Manager Present: True\n";
            linkInfo += $"HMD Present: {OVRManager.hasInputFocus}\n";
        }
        else
        {
            linkInfo += $"OVR Manager: NULL\n";
        }

        // Check OVR Plugin status
        linkInfo += $"OVR Plugin Eye Support: {OVRPlugin.eyeTrackingSupported}\n";

//#if UNITY_ANDROID && !UNITY_EDITOR
//        linkInfo += $"Has Eye Permission: {OVRPlugin.hasEyeTrackingPermission}\n";
//#else
//        linkInfo += "Eye Permission: N/A (Link/Editor)\n";
//#endif

//        if (linkStatusDisplay != null)
//            linkStatusDisplay.text = linkInfo;
    }

    void UpdateEyeTrackingStatus()
    {
        string eyeInfo = "=== EYE TRACKING STATUS ===\n";

        // Auto-find eye gazes if not assigned
        if (leftEyeGaze == null || rightEyeGaze == null)
        {
            FindEyeGazeComponents();
        }

        // Check OVR Plugin instead of OVRManager for eye tracking
        eyeInfo += $"OVRPlugin Eye Tracking: {OVRPlugin.eyeTrackingSupported}\n";

        // Left Eye Status
        if (leftEyeGaze != null)
        {
            eyeInfo += $"\nLEFT EYE:\n";
            eyeInfo += $"  Enabled: {leftEyeGaze.EyeTrackingEnabled}\n";
            eyeInfo += $"  Confidence: {leftEyeGaze.Confidence:F3}\n";
            eyeInfo += $"  Position: {leftEyeGaze.transform.position}\n";
            eyeInfo += $"  Rotation: {leftEyeGaze.transform.rotation.eulerAngles}\n";
        }
        else
        {
            eyeInfo += "\nLEFT EYE: NOT FOUND\n";
        }

        // Right Eye Status
        if (rightEyeGaze != null)
        {
            eyeInfo += $"\nRIGHT EYE:\n";
            eyeInfo += $"  Enabled: {rightEyeGaze.EyeTrackingEnabled}\n";
            eyeInfo += $"  Confidence: {rightEyeGaze.Confidence:F3}\n";
            eyeInfo += $"  Position: {rightEyeGaze.transform.position}\n";
            eyeInfo += $"  Rotation: {rightEyeGaze.transform.rotation.eulerAngles}\n";
        }
        else
        {
            eyeInfo += "\nRIGHT EYE: NOT FOUND\n";
        }

        // Overall status
        bool eyeTrackingWorking = (leftEyeGaze != null && leftEyeGaze.EyeTrackingEnabled) ||
                                 (rightEyeGaze != null && rightEyeGaze.EyeTrackingEnabled);

        eyeInfo += $"\n=== OVERALL STATUS ===\n";
        eyeInfo += $"Eye Tracking Working: {eyeTrackingWorking}\n";

        if (!eyeTrackingWorking)
        {
            eyeInfo += "\n=== TROUBLESHOOTING ===\n";
            eyeInfo += "• Check Quest Features Eye Tracking = Required\n";
            eyeInfo += "• Verify Quest Pro/3 (Quest 2 unsupported)\n";
            eyeInfo += "• Run eye calibration in Quest settings\n";
            eyeInfo += "• Consider building to device vs Link\n";
        }

        if (debugDisplay != null)
            debugDisplay.text = eyeInfo;

        // Console logging for key status changes
        Debug.Log($"Eye Tracking - Left: {leftEyeGaze?.EyeTrackingEnabled}, Right: {rightEyeGaze?.EyeTrackingEnabled}");
    }

    void FindEyeGazeComponents()
    {
        OVREyeGaze[] eyeGazes = FindObjectsOfType<OVREyeGaze>();

        foreach (OVREyeGaze eyeGaze in eyeGazes)
        {
            if (eyeGaze.Eye == OVREyeGaze.EyeId.Left && leftEyeGaze == null)
            {
                leftEyeGaze = eyeGaze;
                Debug.Log($"Auto-found Left Eye Gaze: {eyeGaze.name}");
            }
            else if (eyeGaze.Eye == OVREyeGaze.EyeId.Right && rightEyeGaze == null)
            {
                rightEyeGaze = eyeGaze;
                Debug.Log($"Auto-found Right Eye Gaze: {eyeGaze.name}");
            }
        }
    }

    // Force enable eye tracking - Updated for newer SDK
    [ContextMenu("Force Enable Eye Tracking")]
    public void ForceEnableEyeTracking()
    {
        Debug.Log("Eye tracking is controlled through Quest Features settings in OVRManager");
        Debug.Log("Check: Quest Features > Eye Tracking Support = 'Required'");

        // Try to request permissions
//#if UNITY_ANDROID && !UNITY_EDITOR
//        try
//        {
//            OVRPlugin.RequestEyeTrackingPermission();
//            Debug.Log("Requested eye tracking permission");
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"Permission request failed: {e.Message}");
//        }
//#endif
    }
}