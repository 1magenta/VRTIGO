using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections;

public class SkewDisplayRotation : MonoBehaviour
{
    // OVREyeGaze components for tracking left and right eye rotation
    public OVREyeGaze LeftEyeGaze;
    public OVREyeGaze RightEyeGaze;
    public GameObject LeftEyeGazeObject;

    public GameObject LeftObject1; // object 1
    public GameObject RightObject2; // object 2

    [Header("Logging Settings")]
    public float logRate = 50f; // Hz - consistent logging frequency

    // File paths for saving rotation data
    public string pathleft, pathright, path;
    public string headposfile, headrotfile;

    private string activeObject = "None"; // Tracks which object is active (LeftObject1, RightObject2)

    private float now;
    private float phaseEndTime;
    private string phase = "Both"; // Start with Both active
    public StartSystem startMenu;
    public int counter = 0;

    void OnEnable()
    {
        // Set fixed timestep to achieve desired logging rate
        Time.fixedDeltaTime = 1f / logRate;

        if (startMenu.recording)
        {
            // Initialize file paths only once
            path = Path.Combine(Application.persistentDataPath, "TestOfSkew");
            path = Path.Combine(path, StartSystem.playerName);
            path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

            // Create directory for storing data
            Directory.CreateDirectory(path);
            pathleft = Path.Combine(path, "LeftEyeRotation.txt");
            pathright = Path.Combine(path, "RightEyeRotation.txt");
            headposfile = Path.Combine(path, "HeadPosition.txt");
            headrotfile = Path.Combine(path, "HeadRotation.txt");
        }

        // Set initial object states and start the cycle
        LeftObject1.SetActive(true);
        RightObject2.SetActive(true);
        if (startMenu.running)
        {
            StartCoroutine(StartPhase(1f)); // Phase duration is 1 second for the initial part
        }
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        // Debug logging frequency
        if (Time.frameCount % 60 == 0)
        {
            float actualFPS = 1f / Time.deltaTime;
            Debug.Log($"SkewTest - Actual FPS: {actualFPS:F1}, Target log rate: {logRate}Hz");
        }

        // Log all tracking data at consistent frequency
        if (startMenu.recording) 
        {
            LogAllData();
        }
    }

    private void LogAllData()
    {
        // Process and save left eye data
        Vector3 leftEyeEuler = LeftEyeGaze.transform.rotation.eulerAngles;
        Vector3 leftEyeConverted = ConvertToMinus180To180(leftEyeEuler);

        // Process and save right eye data
        Vector3 rightEyeEuler = RightEyeGaze.transform.rotation.eulerAngles;
        Vector3 rightEyeConverted = ConvertToMinus180To180(rightEyeEuler);

        // Head movement recording (headset position and rotation)
        Vector3 headsetPosition = LeftEyeGazeObject.transform.position; // Assuming this is your head position
        Quaternion headsetRotation = LeftEyeGazeObject.transform.rotation; // Assuming this is your head rotation

        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

        try
        {
            // Log all data streams with same timestamp for perfect synchronization
            File.AppendAllText(pathleft, timestamp + ", " + $"{leftEyeConverted.x}, {leftEyeConverted.y}, {leftEyeConverted.z}, {activeObject}\n");
            File.AppendAllText(pathright, timestamp + ", " + $"{rightEyeConverted.x}, {rightEyeConverted.y}, {rightEyeConverted.z}, {activeObject}\n");
            File.AppendAllText(headposfile, timestamp + ", " + $"{headsetPosition.x}, {headsetPosition.y}, {headsetPosition.z}, {activeObject}\n");
            File.AppendAllText(headrotfile, timestamp + ", " + $"{headsetRotation.eulerAngles.x}, {headsetRotation.eulerAngles.y}, {headsetRotation.eulerAngles.z}, {activeObject}\n");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write tracking data: {ex.Message}");
        }
    }

    IEnumerator StartPhase(float duration)
    {
        phaseEndTime = Time.time + duration;

        while (Time.time < phaseEndTime)
        {
            yield return null; // Wait until the phase duration has elapsed
        }

        Debug.Log(phase);

        if (phase == "Both")
        {
            // Both objects are active for 5 seconds
            LeftObject1.SetActive(true);
            RightObject2.SetActive(true);
            activeObject = "BothActive"; // Both objects are active
            phase = "LeftActive"; // Transition to LeftActive phase
            StartCoroutine(StartPhase(5f)); // Wait for 5 seconds with both objects active
        }
        else if (phase == "LeftActive")
        {
            // Left object active, Right object deactivated for 5 seconds
            LeftObject1.SetActive(true);
            RightObject2.SetActive(false);
            activeObject = "LeftActive"; // Record active object
            phase = "BothAfterLeft"; // Transition back to Both phase
            StartCoroutine(StartPhase(5f)); // Wait for 5 seconds with LeftObject1 active
        }
        else if (phase == "BothAfterLeft")
        {
            // Both objects are active for 5 seconds
            LeftObject1.SetActive(true);
            RightObject2.SetActive(true);
            activeObject = "BothActive"; // Both objects are active
            phase = "RightActive"; // Transition to RightActive phase
            StartCoroutine(StartPhase(5f)); // Wait for 5 seconds with both objects active
        }
        else if (phase == "RightActive")
        {
            // Right object active, Left object deactivated for 5 seconds
            LeftObject1.SetActive(false);
            RightObject2.SetActive(true);
            activeObject = "RightActive"; // Record active object
            phase = "BothAfterRight"; // Transition back to Both phase
            StartCoroutine(StartPhase(5f)); // Wait for 5 seconds with RightObject2 active
        }
        else if (phase == "BothAfterRight")
        {
            // Both objects are active for 5 seconds
            LeftObject1.SetActive(true);
            RightObject2.SetActive(true);
            activeObject = "BothActive"; // Both objects are active
            if (counter == 0)
            {
                phase = "LeftActive";
                counter = 1;
            }
            else
            {
                phase = "None"; // Transition to None phase (end of cycle) after 1 repeat
            }
            StartCoroutine(StartPhase(5f)); // Wait for 5 seconds with both objects active
        }
        else if (phase == "None")
        {
            // Both objects are deactivated (end of cycle)
            LeftObject1.SetActive(false);
            RightObject2.SetActive(false);
            activeObject = "None"; // Both objects are inactive
            Debug.Log("Cycle completed.");
        }
    }

    // Converts a Vector3 of Euler angles to the range [-180, 180]
    Vector3 ConvertToMinus180To180(Vector3 eulerAngles)
    {
        return new Vector3(
            ConvertAngleToMinus180To180(eulerAngles.x),
            ConvertAngleToMinus180To180(eulerAngles.y),
            ConvertAngleToMinus180To180(eulerAngles.z)
        );
    }

    // Converts a single angle to the range [-180, 180]
    float ConvertAngleToMinus180To180(float angle)
    {
        return (angle > 180) ? angle - 360 : angle;
    }
}