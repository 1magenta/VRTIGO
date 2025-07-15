using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections;

public class FingerTap : MonoBehaviour
{
    public static FingerTap Instance;

    public string FingerTapCounter, path;
    public string headposfile, headrotfile;

    public int counter;
    public TextMeshProUGUI Counting;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI Notice;


    [Header("Logging Settings")]
    public float logRate = 50f;

    private OVRHand rightHand;
    private OVRHand leftHand;

    private bool[] fingerPinchStates = new bool[5]; // Tracks if fingers are currently pinching
    private float[] pinchCooldownTimers = new float[5]; // Cooldown timers for each finger
    private float pinchCooldown = 0.005f; // Cooldown time in seconds

    private float now;
    private string phase; // Default phase
    //private float phaseTime = 10f; // Duration for Phase 1 (testing) and Phase 2 (15 seconds)
    private float phaseEndTime;
    private Transform centerEyeAnchor;
    public StartSystem startMenu;

    private Vector3 headsetPosition;
    private Quaternion headsetRotation;

    void OnEnable()
    {
        Time.fixedDeltaTime = 1f / logRate;
        // Locate and initialize OVRHand
        OVRCameraRig ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        if (ovrCameraRig != null)
        {
            centerEyeAnchor = ovrCameraRig.centerEyeAnchor;
            Debug.Log("OVRCameraRig found in the scene!");
        }
        else
        {
            Debug.LogError("OVRCameraRig not found in the scene!");
        }

        rightHand = ovrCameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>();
        leftHand = ovrCameraRig.leftHandAnchor.GetComponentInChildren<OVRHand>();

        if (rightHand == null || leftHand == null)
        {
            Debug.LogError("Hand components not found in the scene!");
            Notice.text = "Hand components not found in the scene!";
        }

        if (startMenu.recording)
        {
            // Initialize file path
            path = Path.Combine(Application.persistentDataPath, "FingerTap");
            path = Path.Combine(path, StartSystem.playerName);
            path = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

            try
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Directory created: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create directory: {ex.Message}");
            }

            // Initialize FingerTapCounter file path
            FingerTapCounter = Path.Combine(path, "FingerTapCounter.txt");
            headposfile = Path.Combine(path, "HeadPosition.txt");
            headrotfile = Path.Combine(path, "HeadRotation.txt");
            Debug.Log($"FingerTapCounter File: {FingerTapCounter}");
            Debug.Log($"HeadPosFile: {headposfile}");
            Debug.Log($"HeadRotFile: {headrotfile}");
        }
        counter = 0;

        SetPhase(startMenu.phase);
        Debug.Log(startMenu.phase);
    }

    void Update()
    {
       

        if (leftHand == null || rightHand == null)
            return;

        now = Time.time;

        // Update Phase-specific logic
        if (phase == "Tutorial" || phase == "Round1" || phase == "Round2")
        {
            if (now < phaseEndTime)
            {
                // Handle finger taps for left hand in Phase 1 and 2
                if (phase == "Tutorial")
                {
                    HandleFingerTaps(leftHand, "Left");
                }
                else if (phase == "Round1")
                {
                    // Left hand logic (phase 2)
                    HandleFingerTaps(leftHand, "Left");
                }
                else if (phase == "Round2")
                {
                    // Right hand logic (phase 3)
                    HandleFingerTaps(rightHand, "Right");
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.frameCount % 60 == 0)
        {
            float actualFPS = 1f / Time.deltaTime;
            Debug.Log($"FingerTap - Actual FPS: {actualFPS:F1}, Target log rate: {logRate}Hz");
        }

        headsetPosition = Vector3.zero;
        headsetRotation = Quaternion.identity;

        if (centerEyeAnchor != null)
        {
            headsetPosition = centerEyeAnchor.position;
            headsetRotation = centerEyeAnchor.rotation;
        }

        //Log head data
        if (startMenu.recording && (phase == "Round1" || phase == "Round2"))
        {
            LogHeadData();
        }
    }

    private void LogHeadData()
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

            // log headposition and roation
            File.AppendAllText(headposfile, timestamp + ", " + headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
            File.AppendAllText(headrotfile, timestamp + ", " + headsetRotation.eulerAngles.x + ", " + headsetRotation.eulerAngles.y + ", " + headsetRotation.eulerAngles.z + "\n");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write data: {ex.Message}");
        }
    }

    void SetPhase(string newPhase)
    {
        phase = newPhase;
        counter = 0;
        Counting.text = counter.ToString();

        if (phase == "Tutorial")
        {
            // Set a 10-second timer for phase 1 (testing)
            StartCoroutine(StartCountdown(10f));
            TitleText.text = "Left Hand";
        }
        else if (phase == "Round1")
        {
            // Set a 15-second timer for phase 2 (recording)
            StartCoroutine(StartCountdown(15f));
            TitleText.text = "Left Hand";
        }
        else if (phase == "Round2")
        {
            // Set a 15-second timer for phase 3 (recording for right hand)
            StartCoroutine(StartCountdown(15f));
            TitleText.text = "Right Hand";
        }
    }

    IEnumerator StartCountdown(float countdownTime)
    {
        phaseEndTime = Time.time + countdownTime;

        // Countdown logic: update TimerText every second
        while (Time.time < phaseEndTime)
        {
            float remainingTime = phaseEndTime - Time.time;
            TimerText.text = $"Time Remaining: {Mathf.Max(0f, Mathf.Ceil(remainingTime))}";
            yield return null;  // Wait until the next frame to continue updating
        }

        TimerText.text = "Time's up!";
        // Phase completed, reset counter for the next phase
        counter = 0;
        Counting.text = counter.ToString();
    }

    void HandleFingerTaps(OVRHand hand, string handType)
    {
        for (int i = 1; i < 5; i++) // Start from index 1 to exclude the thumb
        {
            OVRHand.HandFinger finger = (OVRHand.HandFinger)i;

            if (hand.GetFingerIsPinching(finger))
            {
                if (!fingerPinchStates[i] && now >= pinchCooldownTimers[i])
                {
                    counter++;
                    Counting.text = counter.ToString();

                    // Write data to the file only during phase 2 and 3
                    if (phase == "Round1" || phase == "Round2")
                    {
                        try
                        {
                            string fingerName = (phase == "phase 2") ? Enum.GetName(typeof(OVRHand.HandFinger), finger) : $"{handType}-{Enum.GetName(typeof(OVRHand.HandFinger), finger)}";
                            string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            //string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

                            File.AppendAllText(FingerTapCounter, $"{time},{fingerName},{counter}{Environment.NewLine}");
                            //File.AppendAllText(headposfile, timestamp + ", " + headsetPosition.x + ", " + headsetPosition.y + ", " + headsetPosition.z + "\n");
                            //File.AppendAllText(headrotfile, timestamp + ", " + headsetRotation.eulerAngles.x + ", " + headsetRotation.eulerAngles.y + ", " + headsetRotation.eulerAngles.z + "\n");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to write counter data: {ex.Message}");
                        }
                    }

                    // Set cooldown timer for this finger
                    pinchCooldownTimers[i] = now + pinchCooldown;
                }

                fingerPinchStates[i] = true;
            }
            else
            {
                fingerPinchStates[i] = false;
            }
        }
    }

}
