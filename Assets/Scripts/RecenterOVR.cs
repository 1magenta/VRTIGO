using UnityEngine;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;

public class RecenterOVR : MonoBehaviour
{
    public void RecenterCamera()
    {
        OVRManager.display.RecenterPose();
    }
}
