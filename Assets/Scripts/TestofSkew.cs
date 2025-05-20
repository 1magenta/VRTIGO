using UnityEngine;
using System.IO;
using System;
using TMPro;

public class TestofSkew : MonoBehaviour
{
    public Camera leftEyeCamera;
    public Camera rightEyeCamera;

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

    public TextMeshProUGUI title;

    // File paths for saving rotation data
    public string pathleft, pathright, path;

    // Flag to track recording state
    private bool isRecording = false;
    private bool isLeftEye = false;

    void Start()
    {
        // Initialize file path using Application.persistentDataPath
        path = Path.Combine(Application.persistentDataPath, "TestofSkew");
        path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

        // Create directory if it doesn't exist
        try
        {
            Directory.CreateDirectory(path);
            Debug.Log($"Directory created: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create directory: {ex.Message}");
        }

        // Initialize file paths for left and right eye data
        pathleft = Path.Combine(path, "LeftEyeRotation.txt");
        pathright = Path.Combine(path, "RightEyeRotation.txt");

        // Ensure the cameras are found
        if (leftEyeCamera == null || rightEyeCamera == null)
        {
            Debug.LogError("LeftEyeCamera or RightEyeCamera not found!");
            title.text = "LeftEyeCamera or RightEyeCamera not found!";
            return;
        }

        title.text = "Cameras Initialized";

        leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;  // Set for left-eye only
        rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;  // Set for right-eye only

    }

    void Update()
    {
        // Toggle recording on/off with Button.One
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            isRecording = !isRecording;

            if (isRecording)
            {
                // Render black for the left eye
                // leftEyeCamera.clearFlags = CameraClearFlags.SolidColor;
                // leftEyeCamera.backgroundColor = Color.black;
                // leftEyeCamera.cullingMask = 1 << LayerMask.NameToLayer("LeftEyeLayer"); // Ensure only left-eye layer is rendered

                // Set the right camera as the Main Camera
                // rightEyeCamera.tag = "MainCamera";
                // leftEyeCamera.tag = "Untagged"; // Optionally, remove the Main Camera tag from the left eye

                leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;  // Set for left-eye only
                rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;  // Set for right-eye only

                leftEyeCamera.depth = 0;   // Lower depth for left-eye
                rightEyeCamera.depth = 1;  // Higher depth for right-eye

                // Disable the left camera object
                leftEyeCamera.gameObject.SetActive(false);
                rightEyeCamera.gameObject.SetActive(true);

                Debug.Log("Recording Started: Left Eye Covered");
                title.text = "Recording Started: Left Eye Covered";

                isLeftEye = true;
            }
            else
            {
                // Restore normal rendering for the left eye
                // leftEyeCamera.clearFlags = CameraClearFlags.Skybox;
                // leftEyeCamera.cullingMask = ~0; // Enable rendering layers for left eye

                // // Set the left camera back as the Main Camera
                // leftEyeCamera.tag = "MainCamera";
                // rightEyeCamera.tag = "Untagged"; // Optionally, remove the Main Camera tag from the right eye
                
                leftEyeCamera.depth = 1;   // Lower depth for left-eye
                rightEyeCamera.depth = 1;  // Higher depth for right-eye

                // Enable the left camera object again
                leftEyeCamera.gameObject.SetActive(true);
                rightEyeCamera.gameObject.SetActive(false);

                Debug.Log("Recording Stopped");
                title.text = "Recording Stopped";
                isLeftEye = false;
            }
        }

        // Toggle eye coverage with Button.Two
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            if (isLeftEye)
            {
                // Restore normal rendering for the left eye
                // leftEyeCamera.clearFlags = CameraClearFlags.Skybox;
                // leftEyeCamera.cullingMask = ~0; // Enable rendering layers for left eye

                // // Render black for the right eye
                // rightEyeCamera.clearFlags = CameraClearFlags.SolidColor;
                // rightEyeCamera.backgroundColor = Color.black;
                // rightEyeCamera.cullingMask = 1 << LayerMask.NameToLayer("RightEyeLayer"); // Ensure only right-eye layer is rendered

                // // Set the right camera as the Main Camera
                // rightEyeCamera.tag = "MainCamera";
                // leftEyeCamera.tag = "Untagged"; // Optionally, remove the Main Camera tag from the left eye

                // Disable the right camera object
                leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;  // Set for left-eye only
                rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;  // Set for right-eye only

                leftEyeCamera.depth = 1;   // Lower depth for left-eye
                rightEyeCamera.depth = 0;  // Higher depth for right-eye


                leftEyeCamera.gameObject.SetActive(true);
                rightEyeCamera.gameObject.SetActive(false);



                Debug.Log("Right Eye Covered");
                title.text = "Right Eye Covered";
                isLeftEye = false;
            }
            else
            {
                // Render black for the left eye
                // leftEyeCamera.clearFlags = CameraClearFlags.SolidColor;
                // leftEyeCamera.backgroundColor = Color.black;
                // leftEyeCamera.cullingMask = 1 << LayerMask.NameToLayer("LeftEyeLayer"); // Ensure only left-eye layer is rendered

                // // Restore normal rendering for the right eye
                // rightEyeCamera.clearFlags = CameraClearFlags.Skybox;
                // rightEyeCamera.cullingMask = ~0; // Enable rendering layers for right eye

                // // Set the left camera as the Main Camera
                // leftEyeCamera.tag = "MainCamera";
                // rightEyeCamera.tag = "Untagged"; // Optionally, remove the Main Camera tag from the right eye

                leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;  // Set for left-eye only
                rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;  // Set for right-eye only

                leftEyeCamera.depth = 0;   // Lower depth for left-eye
                rightEyeCamera.depth = 1;  // Higher depth for right-eye

                // Disable the left camera object
                rightEyeCamera.gameObject.SetActive(true);
                leftEyeCamera.gameObject.SetActive(false);

                

                Debug.Log("Left Eye Covered");
                title.text = "Left Eye Covered";
                isLeftEye = true;
            }
        }

            // If recording, track and log the left eye rotation (if it's enabled)
        if (isRecording && LeftEyeGaze.isActiveAndEnabled)
        {
            // Process and display left eye rotation
            Vector3 leftEyeEuler = LeftEyeGaze.transform.rotation.eulerAngles;
            Vector3 leftEyeConverted = ConvertToMinus180To180(leftEyeEuler);

            LRotationX.text = "Left eye X Rotation: " + leftEyeConverted.x.ToString();
            LRotationY.text = "Left eye Y Rotation: " + leftEyeConverted.y.ToString();
            LRotationZ.text = "Left eye Z Rotation: " + leftEyeConverted.z.ToString();

            // Save left eye data to file
            try
            {
                File.AppendAllText(pathleft, leftEyeConverted.x + ", " + leftEyeConverted.y + ", " + leftEyeConverted.z + "\n");
                Debug.Log("Left eye rotation written successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write left eye rotation: {ex.Message}");
                LRotationX.text = "Failed to write left eye rotation";
                LRotationX.text = $"Failed to write left eye rotation: {ex.Message}";
            }

            // If the left eye is active, set right eye data to 0
            RRotationX.text = "Right eye X Rotation: 0";
            RRotationY.text = "Right eye Y Rotation: 0";
            RRotationZ.text = "Right eye Z Rotation: 0";

            try
            {
                // Save right eye data as zeros to file
                File.AppendAllText(pathright, "0, 0, 0\n");
                Debug.Log("Right eye rotation written as zeros.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write right eye rotation: {ex.Message}");
                RRotationX.text = "Failed to write right eye rotation";
                RRotationX.text = $"Failed to write right eye rotation: {ex.Message}";
            }
        }

        // If recording, track and log the right eye rotation (if it's enabled)
        if (isRecording && RightEyeGaze.isActiveAndEnabled)
        {
            // Process and display right eye rotation
            Vector3 rightEyeEuler = RightEyeGaze.transform.rotation.eulerAngles;
            Vector3 rightEyeConverted = ConvertToMinus180To180(rightEyeEuler);

            RRotationX.text = "Right eye X Rotation: " + rightEyeConverted.x.ToString();
            RRotationY.text = "Right eye Y Rotation: " + rightEyeConverted.y.ToString();
            RRotationZ.text = "Right eye Z Rotation: " + rightEyeConverted.z.ToString();

            // Save right eye data to file
            try
            {
                File.AppendAllText(pathright, rightEyeConverted.x + ", " + rightEyeConverted.y + ", " + rightEyeConverted.z + "\n");
                Debug.Log("Right eye rotation written successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write right eye rotation: {ex.Message}");
                RRotationX.text = "Failed to write right eye rotation";
                RRotationX.text = $"Failed to write right eye rotation: {ex.Message}";
            }

            // If the right eye is active, set left eye data to 0
            LRotationX.text = "Left eye X Rotation: 0";
            LRotationY.text = "Left eye Y Rotation: 0";
            LRotationZ.text = "Left eye Z Rotation: 0";

            try
            {
                // Save left eye data as zeros to file
                File.AppendAllText(pathleft, "0, 0, 0\n");
                Debug.Log("Left eye rotation written as zeros.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write left eye rotation: {ex.Message}");
                LRotationX.text = "Failed to write left eye rotation";
                LRotationX.text = $"Failed to write left eye rotation: {ex.Message}";
            }
        }

    }

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
