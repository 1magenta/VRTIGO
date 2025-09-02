using UnityEngine;
using TMPro;
using System.IO;
using System;

public class NystagmusDisplayRot : MonoBehaviour
{
    // OVREyeGaze components for tracking left and right eye rotation
    public OVREyeGaze LeftEyeGaze;
    public OVREyeGaze RightEyeGaze;

    // UI elements to display rotation values
    public TextMeshProUGUI LRotationX;
    public TextMeshProUGUI LRotationY;
    public TextMeshProUGUI LRotationZ;
    public TextMeshProUGUI RRotationX;
    public TextMeshProUGUI RRotationY;
    public TextMeshProUGUI RRotationZ;

    // Visual indicator of recording state
    public GameObject Sphere;

    public MoveBetweenTwoTransforms MoveBetweenTwoTransforms;

    [Header("Logging Settings")]
    public float logRate = 50f; // Hz


    // File paths for saving rotation data
    public string pathleft, pathright, path;
    public string headposfile, headrotfile;

    // Variables to manage recording state
    //private int record, onoff;
    private Transform centerEyeAnchor;
    //public StartSystem startMenu;

    private bool autoStartEnabled = true;

    // Called when the script instance is being loaded
    void OnEnable()
    {
        Time.fixedDeltaTime = 1f / logRate;

        // Locate the OVRCameraRig
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            centerEyeAnchor = cameraRig.centerEyeAnchor;
            Debug.Log("OVRCameraRig found in the scene!");
        }
        else
        {
            Debug.LogError("OVRCameraRig not found in the scene!");
        }

        // Initialize file paths - simplified for standalone
        if (autoStartEnabled)
        {
            InitializeStandaloneLogging();
        }

        if (autoStartEnabled && MoveBetweenTwoTransforms != null)
        {
            MoveBetweenTwoTransforms.enabled = true;
        }

        //// Initialize file paths only once
        //if (startMenu.recording)
        //{
        //    // Use Application.persistentDataPath to ensure cross-platform compatibility
        //    path = Path.Combine(Application.persistentDataPath, "TestOfNystagmus");
        //    path = Path.Combine(path, StartSystem.playerName);
        //    path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));


        //    // Create directory for storing data
        //    try
        //    {
        //        Directory.CreateDirectory(path);
        //        Debug.Log($"Directory created: {path}");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Debug.LogError($"Failed to create directory: {ex.Message}");
        //    }

        //    pathleft = Path.Combine(path, "LeftEyeRotation.txt");
        //    pathright = Path.Combine(path, "RightEyeRotation.txt");
        //    headposfile = Path.Combine(path, "HeadPosition.txt");
        //    headrotfile = Path.Combine(path, "HeadRotation.txt");

        //}

        //if (startMenu.running)
        //{
        //    MoveBetweenTwoTransforms.enabled = true;
        //}

        //// Log initial status
        //Debug.Log($"Recording enabled: {startMenu.recording}");
        //Debug.Log($"Left eye tracking: {(LeftEyeGaze != null ? LeftEyeGaze.EyeTrackingEnabled.ToString() : "null")}");
        //Debug.Log($"Right eye tracking: {(RightEyeGaze != null ? RightEyeGaze.EyeTrackingEnabled.ToString() : "null")}");
    }

    void InitializeStandaloneLogging()
    {
        path = GlobalTestManager.GetTestDataPath("TestOfNystagmus");

        pathleft = Path.Combine(path, "LeftEyeRotation.txt");
        pathright = Path.Combine(path, "RightEyeRotation.txt");
        headposfile = Path.Combine(path, "HeadPosition.txt");
        headrotfile = Path.Combine(path, "HeadRotation.txt");
    }

    void Update()
    {
        // Toggle recording on button press (Right Controller, Button One)
        // if ((OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) && record == 0)
        // {
        //     record = 1;
        //     //Sphere.gameObject.GetComponent<Renderer>().material.color = Color.green; // Change sphere color to green
        //     MoveBetweenTwoTransforms.enabled = true; // Enable movement script
        // }

        // // Stop recording on button press (Right Controller, Button Two)
        // if ((OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) && record == 1)
        // {
        //     record = 0;
        //     //Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red; // Change sphere color to red
        //     MoveBetweenTwoTransforms.enabled = false; // disable movement script
        // }

        // Record eye rotation data if recording is active and eye tracking is enabled

        //if (!LeftEyeGaze.EyeTrackingEnabled)
        //{
        //    Debug.LogError("Left eye tracking enabled: " + LeftEyeGaze.EyeTrackingEnabled);
        //}

        //if (!RightEyeGaze.EyeTrackingEnabled)
        //{
        //    Debug.LogError("Right eye tracking enabled: " + RightEyeGaze.EyeTrackingEnabled);
        //}
    }


    void FixedUpdate()
    {

        // Debug logging frequency
        if (Time.frameCount % 60 == 0)
        {
            float actualFPS = 1f / Time.deltaTime;
            Debug.Log($"Nystagmus - Actual FPS: {actualFPS:F1}, Target log rate: {logRate}Hz");
        }

        // Log all tracking data at consistent frequency
        //if (startMenu.recording) //&& LeftEyeGaze.EyeTrackingEnabled && RightEyeGaze.EyeTrackingEnabled)
        //{
        //    LogAllData();
        //}

        if (autoStartEnabled)
        {
            LogAllData();
        }
    }

    private void LogAllData()
    {
        Vector3 headsetPosition = Vector3.zero;
        Quaternion headsetRotation = Quaternion.identity;

        if (centerEyeAnchor != null)
        {
            headsetPosition = centerEyeAnchor.position;
            headsetRotation = centerEyeAnchor.rotation;
        }

        // Process eye rotation data
        Vector3 leftEyeEuler = LeftEyeGaze.transform.rotation.eulerAngles;
        Vector3 leftEyeConverted = ConvertToMinus180To180(leftEyeEuler);

        Vector3 rightEyeEuler = RightEyeGaze.transform.rotation.eulerAngles;
        Vector3 rightEyeConverted = ConvertToMinus180To180(rightEyeEuler);

        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

            // Log all data streams with same timestamp for perfect synchronization
            File.AppendAllText(pathleft, timestamp + ", " + MoveBetweenTwoTransforms.phase + ", " + leftEyeConverted.x + ", " + leftEyeConverted.y + ", " + leftEyeConverted.z + "\n");
            File.AppendAllText(pathright, timestamp + ", " + MoveBetweenTwoTransforms.phase + ", " + rightEyeConverted.x + ", " + rightEyeConverted.y + ", " + rightEyeConverted.z + "\n");
            File.AppendAllText(headposfile, timestamp + ", " + MoveBetweenTwoTransforms.phase + ", " + headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
            File.AppendAllText(headrotfile, timestamp + ", " + MoveBetweenTwoTransforms.phase + ", " + headsetRotation.eulerAngles.x + ", " + headsetRotation.eulerAngles.y + ", " + headsetRotation.eulerAngles.z + "\n");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write tracking data: {ex.Message}");
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
