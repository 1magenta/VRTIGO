using UnityEngine;
using System.IO;
using System;

public class HeadStability_BgChanger : MonoBehaviour
{
    public Camera cam;
    public GameObject Passthrough;
    public OVREyeGaze LeftEyeGaze;
    public OVREyeGaze RightEyeGaze;
    //public StartSystem startMenu;
    public GameObject smiley;
    private Transform centerEyeAnchor;
    private Vector3 headsetPosition = Vector3.zero;
    private Quaternion headsetRotation = Quaternion.identity;

    private bool autoStartEnabled = true;

    [Header("Logging Settings")]
    public float logRate = 50f;

    private float timer = 0f;
    private int state = 0; // 0 = solid, 1 = clear, 2 = skybox
    public string stateString = "solid";
    
    private string path, pathleft, pathright, headposfile, headrotfile;

    void OnEnable()
    {
        Time.fixedDeltaTime = 1f/ logRate;

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black; // Set to your preferred solid color
        Passthrough.SetActive(false);

        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();///
        if (cameraRig != null)
        {
            centerEyeAnchor = cameraRig.centerEyeAnchor;
            Debug.Log("OVRCameraRig found in the scene!");
        }
        else
        {
            Debug.LogError("OVRCameraRig not found in the scene!");
        }


        //if (startMenu.recording)
        //{
        //    // Initialize file paths
        //    path = Path.Combine(Application.persistentDataPath, "HeadStability");
        //    path = Path.Combine(path, StartSystem.playerName);
        //    path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        //    Directory.CreateDirectory(path);

        //    pathleft = Path.Combine(path, "LeftEyeRotation.txt");
        //    pathright = Path.Combine(path, "RightEyeRotation.txt");
        //    headposfile = Path.Combine(path, "HeadPosition.txt");
        //    headrotfile = Path.Combine(path, "HeadRotation.txt");
        //}

        if (autoStartEnabled)
        {
            InitializeStandaloneLogging();
        }
    }

    void InitializeStandaloneLogging()
    {
        path = GlobalTestManager.GetTestDataPath("HeadStability");

        pathleft = Path.Combine(path, "LeftEyeRotation.txt");
        pathright = Path.Combine(path, "RightEyeRotation.txt");
        headposfile = Path.Combine(path, "HeadPosition.txt");
        headrotfile = Path.Combine(path, "HeadRotation.txt");

        Debug.Log($"HeadStability data path: {path}");
    }

    void Update()
    {
        //if (!startMenu.running) return; // Only update when startMenu is running
        if (!autoStartEnabled) return; // Only run when standalone


        timer += Time.deltaTime;

        if (state == 0 && timer >= 10f)
        {
            //Passthrough.SetActive(true);
            stateString = "passthrough";
            cam.clearFlags = CameraClearFlags.Depth;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0); // Fully transparent
            Passthrough.SetActive(true);
            Debug.Log("Switching to clear");
            state = 1;
            timer = 0f;
        }
        else if (state == 1 && timer >= 10f)
        {
            Passthrough.SetActive(false);
            stateString = "skybox";
            cam.clearFlags = CameraClearFlags.Skybox;
            Debug.Log("Switching to skybox");
            state = 2;
            timer = 0f;
        }
        else if (state == 2 && timer >= 10f)
        {
            smiley.SetActive(false);
            this.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (Time.frameCount % 60 == 0)
        {
            float actualFPS = 1f / Time.deltaTime;
            Debug.Log($"HeadStability - Actual FPS: {actualFPS:F1}, Target log rate: {logRate}Hz");
        }

        if (centerEyeAnchor != null)
        {
            headsetPosition = centerEyeAnchor.position;
            headsetRotation = centerEyeAnchor.rotation;
        }

        //if (startMenu.recording && startMenu.running)
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
        Vector3 leftEyeEuler = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
        Vector3 rightEyeEuler = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);

        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

        try
        {
            // Log all data streams with same timestamp for perfect synchronization
            File.AppendAllText(pathleft, timestamp + ", " + $"{leftEyeEuler.x}, {leftEyeEuler.y}, {leftEyeEuler.z}, {stateString}\n");
            File.AppendAllText(pathright, timestamp + ", " + $"{rightEyeEuler.x}, {rightEyeEuler.y}, {rightEyeEuler.z}, {stateString}\n");
            File.AppendAllText(headposfile, timestamp + ", " + $"{headsetPosition.x}, {headsetPosition.y}, {headsetPosition.z}, {stateString}\n");
            File.AppendAllText(headrotfile, timestamp + ", " + $"{headsetRotation.eulerAngles.x}, {headsetRotation.eulerAngles.y}, {headsetRotation.eulerAngles.z}, {stateString}\n");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write tracking data: {ex.Message}");
        }
    }

    //void RecordTrackingData()
    //{
    //    if (startMenu.recording) //&& LeftEyeGaze.EyeTrackingEnabled && RightEyeGaze.EyeTrackingEnabled)
    //    {
    //        Vector3 leftEyeEuler = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
    //        Vector3 rightEyeEuler = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);

    //        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");


    //        try
    //        {
    //            File.AppendAllText(pathleft, timestamp + ", " + $"{leftEyeEuler.x}, {leftEyeEuler.y}, {leftEyeEuler.z}, {stateString}\n");
    //            File.AppendAllText(pathright, timestamp + ", " + $"{rightEyeEuler.x}, {rightEyeEuler.y}, {rightEyeEuler.z}, {stateString}\n");
    //            File.AppendAllText(headposfile, timestamp + ", " + $"{headsetPosition.x}, {headsetPosition.y}, {headsetPosition.z}, {stateString}\n");
    //            File.AppendAllText(headrotfile, timestamp + ", " + $"{headsetRotation.eulerAngles.x}, {headsetRotation.eulerAngles.y}, {headsetRotation.eulerAngles.z}, {stateString}\n");
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.LogError($"Failed to write tracking data: {ex.Message}");
    //        }
    //    }
    //}

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

