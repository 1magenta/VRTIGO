using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System;

public class HeadRotation : MonoBehaviour
{
    public TextMeshProUGUI HeadPosX;
    public TextMeshProUGUI HeadPosY;
    public TextMeshProUGUI HeadPosZ;
    public TextMeshProUGUI HeadRotX;
    public TextMeshProUGUI HeadRotY;
    public TextMeshProUGUI HeadRotZ;

    public string headposfile, headrotfile, path;
    public int record, start, onoff;
    public GameObject Sphere;

    private Transform centerEyeAnchor;

    void Start()
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
        if (onoff == 0)
        {
            path = Path.Combine(Application.persistentDataPath, "HeadStability");
            path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

            onoff = 1;
        }

        // Create directory and handle any errors
        try
        {
            Directory.CreateDirectory(path);
            Debug.Log($"Directory created: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create directory: {ex.Message}");
        }

        // Initialize file paths
        headposfile = Path.Combine(path, "HeadPosition.txt");
        headrotfile = Path.Combine(path, "HeadRotation.txt");
        Debug.Log($"HeadPosFile: {headposfile}");
        Debug.Log($"HeadRotFile: {headrotfile}");

        // Initialize other variables
        record = 0;
        start = 0;

        // Set Sphere color to red initially
        Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;
    }

    void Update()
    {
        Vector3 headsetPosition = Vector3.zero;
        Quaternion headsetRotation = Quaternion.identity;

        // Retrieve headset position and rotation
        if (centerEyeAnchor != null)
        {
            headsetPosition = centerEyeAnchor.position;
            headsetRotation = centerEyeAnchor.rotation;

            //Debug.Log($"Headset Position: {headsetPosition}");
            //Debug.Log($"Headset Rotation: {headsetRotation.eulerAngles}");
        }

        // Toggle recording on/off with Button.One
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) && record == 0)
        {
            record = 1;
            Sphere.gameObject.GetComponent<Renderer>().material.color = Color.green;
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) && record == 1)
        {
            record = 0;
            Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }

        // Switch scene with PrimaryIndexTrigger
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) && record == 1)
        {
            SceneManager.LoadScene("EyeTrackingScene");
        }

        // Log and save position/rotation data when recording
        if (record == 1)
        {
            // Update UI text
            HeadPosX.text = "Head X Position: " + headsetPosition.x.ToString();
            HeadPosY.text = "Head Y Position: " + headsetPosition.y.ToString();
            HeadPosZ.text = "Head Z Position: " + headsetPosition.z.ToString();

            HeadRotX.text = "Head X Rotation: " + headsetRotation.x.ToString();
            HeadRotY.text = "Head Y Rotation: " + headsetRotation.y.ToString();
            HeadRotZ.text = "Head Z Rotation: " + headsetRotation.z.ToString();

            // Append position data to file
            try
            {
                File.AppendAllText(headposfile, headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
                Debug.Log("Head position written successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write head position: {ex.Message}");
                HeadPosX.text = "Failed to write head position";
                HeadPosX.text = $"Failed to write head position: {ex.Message}";

            }

            // Append rotation data to file
            try
            {
                File.AppendAllText(headrotfile, headsetRotation.x + ", " + headsetRotation.y + ", " + headsetRotation.z + "\n");
                Debug.Log("Head rotation written successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write head rotation: {ex.Message}");
                HeadRotX.text = "Failed to write head rotation";
                HeadRotX.text = $"Failed to write head rotation: {ex.Message}";
            }
        }
    }
}

// using UnityEngine;
// using TMPro;
// using System.IO;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System;
// using System.Threading.Tasks;
// using UnityEngine.SceneManagement;

// public class HeadRotation : MonoBehaviour
// {
//     public TextMeshProUGUI HeadPosX;
//     public TextMeshProUGUI HeadPosY;
//     public TextMeshProUGUI HeadPosZ;
//     public TextMeshProUGUI HeadRotX;
//     public TextMeshProUGUI HeadRotY;
//     public TextMeshProUGUI HeadRotZ;

//     public string headposfile, headrotfile, path;
//     public int record, start, onoff;
//     public GameObject Sphere;

//     private Transform centerEyeAnchor;

//     // Box API constants
//     private const string BoxUploadUrl = "https://upload.box.com/api/2.0/files/content";
//     private const string AccessToken = "LvprFHotAGhKC6EiraKu68ZsE9EzSiJM"; // Replace with your Box token
//     private const string FolderId = "304247713409"; // "0" is the root folder ID

//     void Start()
//     {
//         // Locate the OVRCameraRig and CenterEyeAnchor
//         OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
//         if (cameraRig != null)
//         {
//             centerEyeAnchor = cameraRig.centerEyeAnchor;
//             Debug.Log("OVRCameraRig found in the scene!");
//         }
//         else
//         {
//             Debug.LogError("OVRCameraRig not found in the scene!");
//         }

//         // Initialize file path using Application.persistentDataPath
//         if (onoff == 0)
//         {
//             path = Path.Combine(Application.persistentDataPath, "HeadStability");
//             path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

//             onoff = 1;
//         }

//         // Create directory and handle any errors
//         try
//         {
//             Directory.CreateDirectory(path);
//             Debug.Log($"Directory created: {path}");
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"Failed to create directory: {ex.Message}");
//         }

//         // Initialize file paths
//         headposfile = Path.Combine(path, "HeadPosition.txt");
//         headrotfile = Path.Combine(path, "HeadRotation.txt");
//         Debug.Log($"HeadPosFile: {headposfile}");
//         Debug.Log($"HeadRotFile: {headrotfile}");

//         // Initialize other variables
//         record = 0;
//         start = 0;

//         // Set Sphere color to red initially
//         Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;
//     }

//     void Update()
//     {
//         Vector3 headsetPosition = Vector3.zero;
//         Quaternion headsetRotation = Quaternion.identity;

//         // Retrieve headset position and rotation
//         if (centerEyeAnchor != null)
//         {
//             headsetPosition = centerEyeAnchor.position;
//             headsetRotation = centerEyeAnchor.rotation;
//         }

//         // Toggle recording on/off with Button.One
//         if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) && record == 0)
//         {
//             record = 1;
//             Sphere.gameObject.GetComponent<Renderer>().material.color = Color.green;
//         }

//         if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) && record == 1)
//         {
//             record = 0;
//             Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;

//             // Upload files to Box when recording stops
//             UploadFilesToBox();
//         }

//         // Switch scene with PrimaryIndexTrigger
//         if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) && record == 1)
//         {
//             SceneManager.LoadScene("EyeTrackingScene");
//         }

//         // Log and save position/rotation data when recording
//         if (record == 1)
//         {
//             // Update UI text
//             HeadPosX.text = "Head X Position: " + headsetPosition.x.ToString();
//             HeadPosY.text = "Head Y Position: " + headsetPosition.y.ToString();
//             HeadPosZ.text = "Head Z Position: " + headsetPosition.z.ToString();

//             HeadRotX.text = "Head X Rotation: " + headsetRotation.x.ToString();
//             HeadRotY.text = "Head Y Rotation: " + headsetRotation.y.ToString();
//             HeadRotZ.text = "Head Z Rotation: " + headsetRotation.z.ToString();

//             // Append position data to file
//             try
//             {
//                 File.AppendAllText(headposfile, headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
//                 Debug.Log("Head position written successfully.");
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"Failed to write head position: {ex.Message}");
//             }

//             // Append rotation data to file
//             try
//             {
//                 File.AppendAllText(headrotfile, headsetRotation.x + ", " + headsetRotation.y + ", " + headsetRotation.z + "\n");
//                 Debug.Log("Head rotation written successfully.");
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"Failed to write head rotation: {ex.Message}");
//             }
//         }
//     }

//     private async void UploadFilesToBox()
//     {
//         await UploadFileToBox(headposfile);
//         await UploadFileToBox(headrotfile);
//     }

//     private async Task UploadFileToBox(string filePath)
//     {
//         if (!File.Exists(filePath))
//         {
//             Debug.LogError("File not found: " + filePath);
//             return;
//         }

//         using (var client = new HttpClient())
//         {
//             client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

//             using (var form = new MultipartFormDataContent())
//             {
//                 form.Add(new StringContent(FolderId), "parent_id");
//                 form.Add(new ByteArrayContent(File.ReadAllBytes(filePath)), "file", Path.GetFileName(filePath));

//                 var response = await client.PostAsync(BoxUploadUrl, form);
//                 if (response.IsSuccessStatusCode)
//                 {
//                     Debug.Log($"File uploaded successfully to Box: {Path.GetFileName(filePath)}");
//                 }
//                 else
//                 {
//                     Debug.LogError($"Failed to upload file to Box. Status: {response.StatusCode}");
//                 }
//             }
//         }
//     }
// }












// FOR USE WITH START UI SCREEN BUTTON INSTEAD OF CONTROLLER BASED
// using UnityEngine;
// using TMPro;
// using System.IO;
// using System.Collections;

// public class HeadRotation : MonoBehaviour
// {
//     public TextMeshProUGUI HeadPosX;
//     public TextMeshProUGUI HeadPosY;
//     public TextMeshProUGUI HeadPosZ;
//     public TextMeshProUGUI HeadRotX;
//     public TextMeshProUGUI HeadRotY;
//     public TextMeshProUGUI HeadRotZ;

//     //public GameObject Sphere;
//     public GameObject UIElement; // Reference to the UI element that includes the button
//     public string headposfile, headrotfile, path;

//     private Transform centerEyeAnchor;
//     private bool isRecording = false;

//     void Start()
//     {
//         // Locate the OVRCameraRig and CenterEyeAnchor
//         OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
//         if (cameraRig != null)
//         {
//             centerEyeAnchor = cameraRig.centerEyeAnchor;
//             Debug.Log("OVRCameraRig found in the scene!");
//         }
//         else
//         {
//             Debug.LogError("OVRCameraRig not found in the scene!");
//         }

//         // Initialize file path using Application.persistentDataPath
//         path = Path.Combine(Application.persistentDataPath, "HeadStability");
//         path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

//         // Create directory and handle any errors
//         try
//         {
//             Directory.CreateDirectory(path);
//             Debug.Log($"Directory created: {path}");
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError($"Failed to create directory: {ex.Message}");
//         }

//         // Initialize file paths
//         headposfile = Path.Combine(path, "HeadPosition.txt");
//         headrotfile = Path.Combine(path, "HeadRotation.txt");
//         Debug.Log($"HeadPosFile: {headposfile}");
//         Debug.Log($"HeadRotFile: {headrotfile}");

//         // Set Sphere color to red initially
//         //Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;

//         // Ensure UI is active at the start
//         UIElement.SetActive(true);
//     }

//     void Update()
//     {
//         if (isRecording)
//         {
//             RecordData();
//         }
//     }

//     public void StartRecording()
//     {
//         if (!isRecording)
//         {
//             isRecording = true;
//             //Sphere.gameObject.GetComponent<Renderer>().material.color = Color.green;
//             UIElement.SetActive(false); // Hide UI
//             StartCoroutine(StopRecordingAfterDuration(60)); // Automatically stop after 60 seconds
//         }
//     }

//     private void RecordData()
//     {
//         Vector3 headsetPosition = Vector3.zero;
//         Quaternion headsetRotation = Quaternion.identity;

//         // Retrieve headset position and rotation
//         if (centerEyeAnchor != null)
//         {
//             headsetPosition = centerEyeAnchor.position;
//             headsetRotation = centerEyeAnchor.rotation;
//         }

//         // Update UI text
//         HeadPosX.text = "Head X Position: " + headsetPosition.x.ToString();
//         HeadPosY.text = "Head Y Position: " + headsetPosition.y.ToString();
//         HeadPosZ.text = "Head Z Position: " + headsetPosition.z.ToString();

//         HeadRotX.text = "Head X Rotation: " + headsetRotation.eulerAngles.x.ToString();
//         HeadRotY.text = "Head Y Rotation: " + headsetRotation.eulerAngles.y.ToString();
//         HeadRotZ.text = "Head Z Rotation: " + headsetRotation.eulerAngles.z.ToString();

//         // Append position and rotation data to files
//         try
//         {
//             File.AppendAllText(headposfile, headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
//             File.AppendAllText(headrotfile, headsetRotation.eulerAngles.x + ", " + headsetRotation.eulerAngles.y + ", " + headsetRotation.eulerAngles.z + "\n");
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError($"Failed to write data: {ex.Message}");
//         }
//     }

//     private IEnumerator StopRecordingAfterDuration(int duration)
//     {
//         yield return new WaitForSeconds(duration);
//         StopRecording();
//     }

//     private void StopRecording()
//     {
//         isRecording = false;
//         //Sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;
//         UIElement.SetActive(true); // Show UI again
//     }
// }
