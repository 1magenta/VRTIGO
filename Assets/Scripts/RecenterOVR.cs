using UnityEngine;

public class RecenterOVR : MonoBehaviour
{
    // Reference to the OVRCameraRig 
    public OVRCameraRig cameraRig;

    // Optional reference point for recentering (if null, will use Vector3.zero)
    public Transform referencePoint;

    // Reference forward direction (default is forward along Z axis)
    public Vector3 referenceForward = Vector3.forward;

    // If cameraRig not assigned, find it in the scene
    private void Start()
    {
        if (cameraRig == null)
        {
            cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig == null)
            {
                Debug.LogError("No OVRCameraRig found in the scene!");
            }
        }

        // Normalize the reference forward direction
        referenceForward.Normalize();
    }

    // Call this method from your UI button
    public void RecenterCamera()
    {
        if (cameraRig == null || cameraRig.trackingSpace == null || cameraRig.centerEyeAnchor == null)
        {
            Debug.LogError("Cannot recenter: Missing required components");
            return;
        }

        Debug.Log("Performing simple and precise recentering");

        // Get references to key transforms
        Transform trackingSpace = cameraRig.trackingSpace;
        Transform centerEye = cameraRig.centerEyeAnchor;

        // Get the current position of the head in world space
        Vector3 headPosition = centerEye.position;

        // Calculate target position (where we want the user to be after recentering)
        Vector3 targetPosition = (referencePoint != null) ? referencePoint.position : Vector3.zero;

        // Calculate the position offset needed move the tracking space so that the head ends up at the target position
        Vector3 positionOffset = trackingSpace.position + (targetPosition - headPosition);

        // Preserve the Y position of the tracking space (maintain proper height)
        positionOffset.y = trackingSpace.position.y;

        // Calculate the rotation needed to align the user with the reference forward
        // Get the current horizontal forward direction of the head
        Vector3 headForward = centerEye.forward;
        headForward.y = 0;
        headForward.Normalize();

        // Get the angle between the head forward and the reference forward
        float angle = Vector3.SignedAngle(headForward, referenceForward, Vector3.up);

        // Create a rotation to align with the reference
        Quaternion rotationOffset = Quaternion.Euler(0, angle, 0);

        // Apply the new position and rotation to the tracking space
        Debug.Log($"Before recenter - TrackingSpace: position={trackingSpace.position}, rotation={trackingSpace.rotation.eulerAngles}");
        Debug.Log($"Before recenter - Head: position={headPosition}, forward={headForward}");

        // Apply the adjustments
        trackingSpace.position = positionOffset;
        trackingSpace.rotation = trackingSpace.rotation * rotationOffset;

        Debug.Log($"After recenter - TrackingSpace: position={trackingSpace.position}, rotation={trackingSpace.rotation.eulerAngles}");

        // Also try the standard OVR recentering for good measure
        if (OVRManager.display != null)
        {
            OVRManager.display.RecenterPose();
        }
    }
}