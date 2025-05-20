using UnityEngine;
using TMPro;

public class ActiveCamerasChecker : MonoBehaviour
{
    // Assign this in the Inspector to your TMP Text UI element
    public TextMeshProUGUI activeCamerasText;

    void Update()
    {
        Camera[] activeCameras = GetActiveCameras();

        if (activeCameras.Length > 0)
        {
            string cameraNames = "Active Cameras:\n";
            foreach (Camera cam in activeCameras)
            {
                cameraNames += cam.name + "\n";
            }

            // Display the active camera names in the TMP text
            activeCamerasText.text = cameraNames;
        }
        else
        {
            activeCamerasText.text = "No active cameras found!";
        }
    }

    // Helper function to get all active cameras
    Camera[] GetActiveCameras()
    {
        Camera[] allCameras = Camera.allCameras;
        return System.Array.FindAll(allCameras, cam => cam.isActiveAndEnabled);
    }
}
