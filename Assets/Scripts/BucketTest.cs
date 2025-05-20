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
   // public int record
    public GameObject Sphere;
    public GameObject circularUI; // UI element for the test
    public float rotationSpeed = 50f; // Speed of rotation adjustment

    private Transform centerEyeAnchor;

    public StartSystem startMenu;

    void OnEnable()
    {
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
        //record = 0;

        Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;

        // Set the circular UI to a random initial Z-axis rotation
        float randomZRotation = Random.Range(0f, 360f);
        circularUI.transform.rotation = Quaternion.Euler(0f, 0f, randomZRotation);
        Debug.Log($"Circular UI initial Z rotation: {randomZRotation}");
    }

    void Update()
    {
        Vector3 headsetPosition = Vector3.zero;
        Quaternion headsetRotation = Quaternion.identity;

        if (centerEyeAnchor != null)
        {
            headsetPosition = centerEyeAnchor.position;
            headsetRotation = centerEyeAnchor.rotation;
        }

        // if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) && record == 0)
        // {
        //     record = 1;
        //     Sphere.gameObject.GetComponent<Renderer>().material.color = Color.green;
        // }

        // if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) && record == 1)
        //  {
        //      record = 0;
        //      Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;
        //  }

        if (startMenu.running){
            // Handle joystick input for rotating the circular UI
        float joystickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
        if (Mathf.Abs(joystickInput) > 0.1f) // Add a dead zone for joystick input
        {
            circularUI.transform.Rotate(0f, 0f, -joystickInput * rotationSpeed * Time.deltaTime);
        }

        }
       
         

        if (startMenu.recording)
        {
            HeadPosX.text = "Head X Position: " + headsetPosition.x.ToString();
            HeadPosY.text = "Head Y Position: " + headsetPosition.y.ToString();
            HeadPosZ.text = "Head Z Position: " + headsetPosition.z.ToString();

            HeadRotX.text = "Head X Rotation: " + headsetRotation.eulerAngles.x.ToString();
            HeadRotY.text = "Head Y Rotation: " + headsetRotation.eulerAngles.y.ToString();
            HeadRotZ.text = "Head Z Rotation: " + headsetRotation.eulerAngles.z.ToString();


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

            Angle.text = "Angle Difference: " + normalizedAngle.ToString("F2") + "°";


            try
            {
                string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");


                File.AppendAllText(headposfile, timestamp + ", " + headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
                File.AppendAllText(headrotfile, timestamp + ", " + headsetRotation.eulerAngles.x + ", " + headsetRotation.eulerAngles.y + ", " + headsetRotation.eulerAngles.z + "\n");
                File.AppendAllText(bucketfile, timestamp + ", " + normalizedAngle + "\n");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error writing data: {ex.Message}");
            }
        }
        
        
    }
}
