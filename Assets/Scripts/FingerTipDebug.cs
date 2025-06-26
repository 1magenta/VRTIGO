using UnityEngine;

public class FingerTipDebug : MonoBehaviour
{
    [Header("Direct Joint Reference")]
    public Transform indexTipJoint;  
    public GameObject debugSphere;

    [Header("Auto-Find Settings")]
    public bool autoFindJoint = true;
    public bool testLeftHand = true;  // Toggle between left and right hand for auto-find

    void Start()
    {
        // Auto-find the joint if not manually assigned
        if (autoFindJoint && indexTipJoint == null)
        {
            FindIndexTipJoint();
        }

        // Create debug sphere
        if (debugSphere == null)
        {
            debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.name = "DirectFingertipSphere";
            debugSphere.transform.localScale = Vector3.one * 0.015f; // 1.5cm sphere
            
            Renderer renderer = debugSphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0f);
            renderer.material = mat;

            // Remove collider
            DestroyImmediate(debugSphere.GetComponent<Collider>());

            Debug.Log("Created debug sphere for direct joint tracking");
        }

        if (indexTipJoint != null)
        {
            Debug.Log($"Using fingertip joint: {indexTipJoint.name}");
            Debug.Log($"Joint position: {indexTipJoint.position}");
        }
        else
        {
            Debug.LogWarning("No index tip joint assigned! Please drag XRHand_IndexTip to the indexTipJoint field.");
        }
    }

    void Update()
    {
        if (indexTipJoint != null && debugSphere != null)
        {
            debugSphere.transform.position = indexTipJoint.position;
            debugSphere.SetActive(true);

            // Optional: Log position every 60 frames for debugging
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Fingertip position: {indexTipJoint.position}");
            }
        }
        else if (debugSphere != null)
        {
            debugSphere.SetActive(false);
        }
    }

    private void FindIndexTipJoint()
    {
        // Try to find the joint automatically
        string jointName = testLeftHand ? "XRHand_IndexTip" : "XRHand_IndexTip";

        // Search for the joint in the scene
        Transform[] allTransforms = FindObjectsOfType<Transform>();

        foreach (Transform t in allTransforms)
        {
            if (t.name == jointName)
            {
                // Check if this is the correct hand by checking parent hierarchy
                string handSide = testLeftHand ? "Left" : "Right";
                if (t.GetComponentInParent<Transform>().name.Contains(handSide) ||
                    t.root.name.Contains(handSide))
                {
                    indexTipJoint = t;
                    Debug.Log($"Auto-found {handSide} hand index tip joint: {t.name}");
                    break;
                }
            }
        }

        if (indexTipJoint == null)
        {
            Debug.LogWarning($"Could not auto-find index tip joint for {(testLeftHand ? "left" : "right")} hand. Please assign manually.");
        }
    }

    // Get the current fingertip position
    public Vector3 GetFingertipPosition()
    {
        if (indexTipJoint != null)
        {
            return indexTipJoint.position;
        }
        else
        {
            Debug.LogWarning("No fingertip joint available!");
            return Vector3.zero;
        }
    }

    // Check if fingertip tracking is available
    public bool IsFingertipTrackingAvailable()
    {
        return indexTipJoint != null;
    }

    [ContextMenu("Test Fingertip Position")]
    public void TestFingertipPosition()
    {
        if (indexTipJoint != null)
        {
            Debug.Log($"Current fingertip position: {indexTipJoint.position}");

            if (debugSphere != null)
            {
                debugSphere.transform.position = indexTipJoint.position;
                debugSphere.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
        else
        {
            Debug.LogError("No fingertip joint assigned!");
        }
    }
}