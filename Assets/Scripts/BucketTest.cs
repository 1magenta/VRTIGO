using UnityEngine;
using TMPro;
using System.IO;

public class BucketTest : MonoBehaviour
{
    public TextMeshProUGUI HeadPosX;
    public TextMeshProUGUI HeadPosY;
    public TextMeshProUGUI HeadPosZ;
    public TextMeshProUGUI HeadRotX;
    public TextMeshProUGUI HeadRotY;
    public TextMeshProUGUI HeadRotZ;
    public TextMeshProUGUI Angle;

    public string headposfile, headrotfile, bucketfile, path;
    public GameObject Sphere;
    public GameObject circularUI; // UI element for the test
    public float rotationSpeed = 50f; // Speed of rotation adjustment

    [Header("Logging Settings")]
    public float logRate = 50f; // Hz - consistent logging frequency

    private Transform centerEyeAnchor;
    public StartSystem startMenu;

    void OnEnable()
    {
        // Set fixed timestep to achieve desired logging rate
        Time.fixedDeltaTime = 1f / logRate;

        // Locate the OVRCameraRig and CenterEyeAnchor
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

        // Initialize file path using Application.persistentDataPath
        if (startMenu.recording)
        {
            path = Path.Combine(Application.persistentDataPath, "BucketTest");
            path = Path.Combine(path, StartSystem.playerName);
            path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

            try
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Directory created: {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create directory: {ex.Message}");
            }

            headposfile = Path.Combine(path, "HeadPosition.txt");
            headrotfile = Path.Combine(path, "HeadRotation.txt");
            bucketfile = Path.Combine(path, "BucketAngle.txt");
            Debug.Log($"HeadPosFile: {headposfile}");
            Debug.Log($"HeadRotFile: {headrotfile}");
        }

        Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;

        // Set the circular UI to a random initial Z-axis rotation
        float randomZRotation = Random.Range(0f, 360f);
        circularUI.transform.rotation = Quaternion.Euler(0f, 0f, randomZRotation);
        Debug.Log($"Circular UI initial Z rotation: {randomZRotation}");
    }

    void Update()
    {
        // Keep joystick input in Update for responsive controls
        if (startMenu.running)
        {
            // Handle joystick input for rotating the circular UI
            float joystickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
            if (Mathf.Abs(joystickInput) > 0.1f) // Add a dead zone for joystick input
            {
                circularUI.transform.Rotate(0f, 0f, -joystickInput * rotationSpeed * Time.deltaTime);
            }
        }
    }

    void FixedUpdate()
    {
        // Debug logging frequency
        if (Time.frameCount % 60 == 0)
        {
            float actualFPS = 1f / Time.deltaTime;
            Debug.Log($"BucketTest - Actual FPS: {actualFPS:F1}, Target log rate: {logRate}Hz");
        }

        // Get headset position and rotation
        Vector3 headsetPosition = Vector3.zero;
        Quaternion headsetRotation = Quaternion.identity;

        if (centerEyeAnchor != null)
        {
            headsetPosition = centerEyeAnchor.position;
            headsetRotation = centerEyeAnchor.rotation;
        }

        // Log all data synchronously at consistent frequency
        if (startMenu.recording)
        {
            LogAllData(headsetPosition, headsetRotation);
        }

        // Update UI display (can be done at lower frequency but keep for consistency)
        UpdateUIDisplay(headsetPosition, headsetRotation);
    }

    private void LogAllData(Vector3 headsetPosition, Quaternion headsetRotation)
    {
        // Get the Z-axis rotation of the circular UI and normalize to 0-90 degrees
        float zRotation = circularUI.transform.eulerAngles.z % 360f; // Normalize to 0-360
        if (zRotation < 0) zRotation += 360f; // Handle negative rotations

        // Normalize to 0-90 degrees using modular symmetry
        float normalizedAngle = zRotation;
        if (zRotation > 90f && zRotation <= 180f)
        {
            normalizedAngle = 180f - zRotation;
        }
        else if (zRotation > 180f && zRotation <= 270f)
        {
            normalizedAngle = zRotation - 180f;
        }
        else if (zRotation > 270f)
        {
            normalizedAngle = 360f - zRotation;
        }

        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

            // Log all data streams with same timestamp for perfect synchronization
            File.AppendAllText(headposfile, timestamp + ", " + headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
            File.AppendAllText(headrotfile, timestamp + ", " + headsetRotation.eulerAngles.x + ", " + headsetRotation.eulerAngles.y + ", " + headsetRotation.eulerAngles.z + "\n");
            File.AppendAllText(bucketfile, timestamp + ", " + normalizedAngle + "\n");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error writing data: {ex.Message}");
        }
    }

    private void UpdateUIDisplay(Vector3 headsetPosition, Quaternion headsetRotation)
    {
        // Update UI text (can be done less frequently if needed for performance)
        HeadPosX.text = "Head X Position: " + headsetPosition.x.ToString();
        HeadPosY.text = "Head Y Position: " + headsetPosition.y.ToString();
        HeadPosZ.text = "Head Z Position: " + headsetPosition.z.ToString();

        HeadRotX.text = "Head X Rotation: " + headsetRotation.eulerAngles.x.ToString();
        HeadRotY.text = "Head Y Rotation: " + headsetRotation.eulerAngles.y.ToString();
        HeadRotZ.text = "Head Z Rotation: " + headsetRotation.eulerAngles.z.ToString();

        // Get normalized angle for display
        float zRotation = circularUI.transform.eulerAngles.z % 360f;
        if (zRotation < 0) zRotation += 360f;

        float normalizedAngle = zRotation;
        if (zRotation > 90f && zRotation <= 180f)
        {
            normalizedAngle = 180f - zRotation;
        }
        else if (zRotation > 180f && zRotation <= 270f)
        {
            normalizedAngle = zRotation - 180f;
        }
        else if (zRotation > 270f)
        {
            normalizedAngle = 360f - zRotation;
        }

        Angle.text = "Angle Difference: " + normalizedAngle.ToString("F2") + "°";
    }
}