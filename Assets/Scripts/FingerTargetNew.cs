using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class FingerTargetNew : MonoBehaviour
{
    [Header("Scene References")]
    public TextMeshProUGUI instructionText;   // UI text instructions
    public GameObject leftBall;               // Left trigger ball
    public GameObject rightBall;              // Right trigger ball
    public GameObject cube;                   // Green cube (reset)
    public GameObject cubeblue;               // Blue cube (start game)
    public GameObject explosion;              // Explosion prefab when ball is touched
    public GameObject displayText;            // Parent container for text
    public GameObject trialText;              // Text showing current trial #
    public GameObject visibilityText;         // Text showing hand visibility
    public StartSystem startMenu;             // Controls recording and playerName
    public GameObject sphereSpawnPrefab;

    public GameObject fabLeft;                // Left hand prefab
    public GameObject fabRight;               // Right hand prefab

    public OVREyeGaze LeftEyeGaze;
    public OVREyeGaze RightEyeGaze;

    

    // OVR References
    public OVRHand leftHand;
    public OVRHand rightHand;
    private OVRSkeleton skeleton;
    private OVRHand activeHand;               // Active hand (left/right depending on detected hand)
    private GameObject activeFab;
    private Transform centerEyeAnchor;
    private Transform activeIndexTip;         // Current active fingertip joint

    [Header("Fingertip Tracking")]
    public Transform leftIndexTipJoint;
    public Transform rightIndexTipJoint;
    public bool showFingertipDebug = false;   // Toggle to show fingertip debug sphere
    private GameObject fingertipDebugSphere;  // Debug sphere (auto-creation)

    [Header("Shoulder Offset")]
    public bool showShoulderDebug = false;    // Toggle to show shoulder position debug sphere
    private GameObject shoulderDebugSphere;   // Debug sphere for shoulder position (auto-creation)
    public float shoulderDropFromHead = 0.25f;
    public float shoulderLateralOffset = 0.18f;    // How far to the side is shoulder

    [Header("Hit Detection")]
    public float hitDetectionRadius = 0.03f; // Distance required to "hit" the target

    [Header("Target Distance Settings")]
    [Range(0.9f, 1.0f)]
    public float targetDistanceMultiplier = 1f; // Multiplier for target distance (full reach by default)
    [Range(0f, 0.1f)]
    public float targetDistanceOffset = 0f;        // Additional offset to subtract (no offset by default)


    [Header("Trajectory Logging")]
    public float trajectoryLogRate = 30f;        // Hz - how often to log trajectory
    private bool enableTrajectoryLogging = true;  // Toggle trajectory logging

    // State variables
    public string handedness = "";
    private bool handednessDetected = false;
    private bool initialized = false;         // Game logic initialized after handedness is known

    // Trial mechanics: To adjust the arm reach quantity (25 reaches per arm)
    private int trial = 1;
    private const int FIRST_RECORDED_TRIAL = 11;      // First trial after practice
    private const int REACHES_PER_ARM = 25;           // Number of reaches for each arm
    private const int SWITCH_HANDS_TRIAL = FIRST_RECORDED_TRIAL + REACHES_PER_ARM; // Trial 36
    private const int TOTAL_TRIALS = SWITCH_HANDS_TRIAL + REACHES_PER_ARM - 1;      // Trial 60

    private string handednessFile;
    private string headPosFile;
    private string headRotFile;
    private string leftHandFile;
    private string rightHandFile;
    private string leftEyeFile;
    private string rightEyeFile;
    private string trialLogFile;
    private string fingertipTrajectoryFile;  // New trajectory log file

    // Calibration variables
    private float calibratedReachDistance = 0.3f;  // Full 3D reach distance
    private Vector3 calibratedShoulderOffset;      // Offset from head to shoulder during calibration
    private Vector3 shoulderPosition;               // Estimated shoulder position

    private enum Phase { Calibration, Reach, Reset }
    private Phase phase = Phase.Calibration;

    private bool evaluating = false;
    private GameObject currentBall;
    private float trialError = 0f;

    private float resetCooldownDuration = 2f; // duration in seconds
    private float resetCooldownEndTime = 0f;

    private List<bool> randomizedVisibilityList = new List<bool>();

    private bool calibrationComplete = false;
    private bool waitingForCubeAfterCalibration = false;

    private bool flippedHands = false;
    private List<bool> flippedVisibilityList = new List<bool>();

    private Vector3 headPosone;
    private Vector3 forwardone;
    private Vector3 downone;

    // Trajectory logging variables
    private float lastTrajectoryLogTime = 0f;
    private bool isLoggingTrajectory = false;
    private Vector3 lastFingertipPosition = Vector3.zero;
    private string currentMovementPhase = "Idle";

    void Start()
    {
        // Get camera rig and hand anchors
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        if (rig != null)
        {
            centerEyeAnchor = rig.centerEyeAnchor;
            leftHand = rig.leftHandAnchor.GetComponentInChildren<OVRHand>();
            rightHand = rig.rightHandAnchor.GetComponentInChildren<OVRHand>();
        }

        // Create file paths for logging
        string rootPath = Path.Combine(Application.persistentDataPath, "VisInvisStability");

        if (!string.IsNullOrEmpty(StartSystem.playerName))
        {
            rootPath = Path.Combine(rootPath, StartSystem.playerName);
        }

        rootPath = Path.Combine(rootPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        Directory.CreateDirectory(rootPath);

        // Randomize visibility trials
        // Generate visible/invisible trials based on reaches per arm
        int reachesPerArm = SWITCH_HANDS_TRIAL - FIRST_RECORDED_TRIAL;
        int visibleTrials = Mathf.CeilToInt(reachesPerArm / 2.0f);  // Half visible (rounded up)
        int invisibleTrials = reachesPerArm - visibleTrials;        // Half invisible (rounded down)

        List<bool> visList = new List<bool>();
        for (int i = 0; i < visibleTrials; i++) visList.Add(true);
        for (int i = 0; i < invisibleTrials; i++) visList.Add(false);

        System.Random rng = new System.Random();
        int n = visList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            bool value = visList[k];
            visList[k] = visList[n];
            visList[n] = value;
        }
        randomizedVisibilityList = visList;

        // Core data logs
        handednessFile = Path.Combine(rootPath, "Handedness.txt");
        headPosFile = Path.Combine(rootPath, "HeadPosition.txt");
        headRotFile = Path.Combine(rootPath, "HeadRotation.txt");
        leftHandFile = Path.Combine(rootPath, "LeftHandPosition.txt");
        rightHandFile = Path.Combine(rootPath, "RightHandPosition.txt");
        leftEyeFile = Path.Combine(rootPath, "LeftEyeRotation.txt");
        rightEyeFile = Path.Combine(rootPath, "RightEyeRotation.txt");

        // Unified trial log file
        trialLogFile = Path.Combine(rootPath, "TrialData.txt");
        File.AppendAllText(trialLogFile, "Time,Trial,Phase,isVisible,Handedness,HandPos,TargetPos,Error,HeadPos,LeftEyeRot,RightEyeRot,ResetCubePos\n");

        // Fingertip trajectory log file
        fingertipTrajectoryFile = Path.Combine(rootPath, "FingertipTrajectory.txt");
        string trajectoryHeader = "Time,Trial,MovementPhase,isVisible,Handedness,FingertipPos,TargetPos,DistanceToTarget," +
                                "Velocity,HeadPos,LeftEyeRot,RightEyeRot,MovementDirection,TimeFromTrialStart\n";
        File.AppendAllText(fingertipTrajectoryFile, trajectoryHeader);

        // Create fingertip debug sphere if enabled
        if (showFingertipDebug && fingertipDebugSphere == null)
        {
            fingertipDebugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fingertipDebugSphere.name = "FingertipDebugSphere";
            fingertipDebugSphere.transform.localScale = Vector3.one * 0.01f; // 1cm sphere

            // Make it bright and easy to see
            Renderer renderer = fingertipDebugSphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0f);
            renderer.material = mat;

            // Remove collider to avoid interference
            DestroyImmediate(fingertipDebugSphere.GetComponent<Collider>());

            Debug.Log("Created fingertip debug sphere");
        }

        // Create shoulder debug sphere if enabled
        if (showShoulderDebug && shoulderDebugSphere == null)
        {
            shoulderDebugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulderDebugSphere.name = "ShoulderDebugSphere";
            shoulderDebugSphere.transform.localScale = Vector3.one * 0.03f; // 3cm sphere (larger for visibility)

            // Make it blue to distinguish from fingertip
            Renderer renderer = shoulderDebugSphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.blue;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0f);
            renderer.material = mat;

            // Remove collider to avoid interference
            DestroyImmediate(shoulderDebugSphere.GetComponent<Collider>());

            Debug.Log("Created shoulder debug sphere");
        }
    }

    void Update()
    {
        if (!handednessDetected)
        {
            float leftDist = Vector3.Distance(fabLeft.transform.position, leftBall.transform.position);
            float rightDist = Vector3.Distance(fabRight.transform.position, rightBall.transform.position);

            if (leftDist < .2f) // tweak threshold as needed
            {
                handedness = "Left";
                handednessDetected = true;
                instructionText.text = "Left hand detected. Waiting to Start...";
                File.WriteAllText(handednessFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{handedness}");
                Debug.Log("Detected Left Hand by proximity.");
            }
            else if (rightDist < .2f)
            {
                handedness = "Right";
                handednessDetected = true;
                instructionText.text = "Right hand detected. Waiting to Start...";
                File.WriteAllText(handednessFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{handedness}");
                Debug.Log("Detected Right Hand by proximity.");
            }
        }

        // Wait for handedness detection and recording trigger
        if (!handednessDetected || !startMenu.recording) return;

        // Initialize once after handedness is detected
        if (!initialized)
        {
            leftBall.SetActive(false);
            rightBall.SetActive(false);
            if (handedness == "Left")
            {
                fabRight.SetActive(false);
                fabLeft.SetActive(true);
                activeHand = leftHand;
                activeFab = fabLeft;
                activeIndexTip = leftIndexTipJoint; // Set active fingertip joint
            }
            else
            {
                fabLeft.SetActive(false);
                fabRight.SetActive(true);
                activeHand = rightHand;
                activeFab = fabRight;
                activeIndexTip = rightIndexTipJoint; // Set active fingertip joint
            }
            initialized = true;
            skeleton = activeHand.GetComponent<OVRSkeleton>();

            headPosone = centerEyeAnchor.position;
            forwardone = centerEyeAnchor.forward;
            downone = Vector3.down;
        }

        // Log head/eye/hand data every frame
        LogStandardData();

        // Update fingertip debug visualization
        UpdateFingertipDebugSphere();

        // Update shoulder debug visualization
        UpdateShoulderDebugSphere();

        // Log fingertip trajectory during active trials
        if (enableTrajectoryLogging && trial >= 2) // Start logging from practice trials
        {
            LogFingertipTrajectory();
        }

        if (trial == 1)
        {
            RunCalibrationTrial();
        }
        else if (trial >= 2 && trial <= 10)
        {
            RunPracticeTrials();
        }
        else if (trial >= FIRST_RECORDED_TRIAL && trial <= TOTAL_TRIALS)
        {
            RunRecordedTrials();
        }

        if (waitingForCubeAfterCalibration)
        {
            Vector3 handPos = GetFingertipOrHandPos();
            if (Vector3.Distance(handPos, cube.transform.position) < 0.15f)
            {
                cube.SetActive(false);
                trial = 2;
                trialText.GetComponent<TextMeshProUGUI>().text = "Trial 2";
                phase = Phase.Reach;
                waitingForCubeAfterCalibration = false;
                instructionText.text = "Touch and hold the ball with your fingertip";
                ShowRandomBall();
            }
            return; // Stop running trials while waiting
        }
    }

    private void RunCalibrationTrial()
    {
        instructionText.text = "Reach out and pinch your index and thumb fingers";
        if (activeHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            Vector3 handPos = GetFingertipOrHandPos();
            Vector3 headPos = centerEyeAnchor.position;

            // Calculate estimated shoulder position
            shoulderPosition = GetEstimatedShoulderPosition(headPos);

            // Calculate 3D reach distance from shoulder to fingertip
            float reachDistance3D = Vector3.Distance(handPos, shoulderPosition);

            if (reachDistance3D >= 0.1f && !calibrationComplete)
            {
                calibratedReachDistance = reachDistance3D;
                calibratedShoulderOffset = shoulderPosition - headPos;
                calibrationComplete = true;
                waitingForCubeAfterCalibration = true;

                instructionText.text = $"Calibration complete! Reach distance: {calibratedReachDistance:F2}m\nNow touch the green cube with your fingertip to start.";
                Debug.Log($"Calibrated reach distance: {calibratedReachDistance:F2}m from estimated shoulder position");
                Debug.Log($"Shoulder position: {shoulderPosition}, Fingertip position: {handPos}");

                PositionResetCube();
                cube.SetActive(true); // Show the cube
            }
        }
    }

    private Vector3 GetEstimatedShoulderPosition(Vector3 headPosition)
    {
        // Estimate shoulder position based on head position and handedness
        Vector3 shoulderPos = headPosition;

        shoulderPos.y -= shoulderDropFromHead;

        // Offset laterally based on handedness
        Vector3 rightVector = centerEyeAnchor.right;
        if (handedness == "Right")
        {
            shoulderPos += rightVector * shoulderLateralOffset;
        }
        else
        {
            shoulderPos -= rightVector * shoulderLateralOffset;
        }

        return shoulderPos;
    }

    private void RunPracticeTrials()
    {
        Vector3 handPos = GetFingertipOrHandPos();

        if (phase == Phase.Reach)
        {
            instructionText.text = "Touch and hold the ball with your fingertip";
            currentMovementPhase = "Reaching";
            isLoggingTrajectory = true;

            SetHandVisibility(trial < 6);

            // Use 3D distance-based hit detection instead of Z-axis crossing
            float distanceToTarget = Vector3.Distance(handPos, currentBall.transform.position);
            if (distanceToTarget <= hitDetectionRadius && !evaluating)
            {
                currentMovementPhase = "TargetHit";
                evaluating = true;
                StartCoroutine(EvaluateReachAfterDelay(0.01f));
            }
        }
        else if (phase == Phase.Reset)
        {
            currentMovementPhase = "Returning";

            if (Time.time >= resetCooldownEndTime && Vector3.Distance(handPos, cube.transform.position) < 0.15f)
            {
                currentMovementPhase = "CubeReached";
                isLoggingTrajectory = false; // Stop logging when cube is reached
                cube.SetActive(false);
                trial++;
                evaluating = false;

                if (trial <= 10)
                {
                    instructionText.text = $"Trial {trial}";
                    trialText.GetComponent<TextMeshProUGUI>().text = $"Trial {trial}";
                    phase = Phase.Reach;
                    ShowRandomBall();
                }
                else
                {
                    SetHandVisibility(true);
                }
            }
        }
    }

    private System.Collections.IEnumerator EvaluateReachAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 finalHandPos = GetFingertipOrHandPos();

        Instantiate(explosion, currentBall.transform.position, Quaternion.identity);
        currentBall.SetActive(false);
        PositionResetCube();
        cube.SetActive(true);
        trialError = Vector3.Distance(finalHandPos, currentBall.transform.position);

        instructionText.text = "Return to cube.";
        phase = Phase.Reset;
        resetCooldownEndTime = Time.time + resetCooldownDuration;
    }

    private void RunRecordedTrials()
    {
        Vector3 handPos = GetFingertipOrHandPos();

        if (phase == Phase.Reach)
        {
            instructionText.text = "Touch and hold the ball with your fingertip";
            currentMovementPhase = "Reaching";
            isLoggingTrajectory = true;

            // Adjusted indices to account for 25 trials per hand
            if (trial >= FIRST_RECORDED_TRIAL && trial < SWITCH_HANDS_TRIAL)
            {
                int visIndex = trial - FIRST_RECORDED_TRIAL;
                if (visIndex >= 0 && visIndex < randomizedVisibilityList.Count)
                {
                    SetHandVisibility(randomizedVisibilityList[visIndex]);
                }
            }
            else if (trial >= SWITCH_HANDS_TRIAL && trial <= TOTAL_TRIALS)
            {
                int visIndex = trial - SWITCH_HANDS_TRIAL;
                if (visIndex >= 0 && visIndex < flippedVisibilityList.Count)
                {
                    SetHandVisibility(flippedVisibilityList[visIndex]);
                }
            }

            // Use 3D distance-based hit detection instead of Z-axis crossing
            float distanceToTarget = Vector3.Distance(handPos, currentBall.transform.position);
            if (distanceToTarget <= hitDetectionRadius && !evaluating)
            {
                currentMovementPhase = "TargetHit";
                evaluating = true;
                StartCoroutine(EvaluateAndLogReachAfterDelay(0.01f)); 
            }
        }
        else if (phase == Phase.Reset)
        {
            currentMovementPhase = "Returning";

            if (Time.time >= resetCooldownEndTime && Vector3.Distance(handPos, cube.transform.position) < 0.15f)
            {
                LogResetReturn();  // Only called during trials 11–20
                currentMovementPhase = "CubeReached";
                isLoggingTrajectory = false; // Stop logging when cube is reached
                cube.SetActive(false);
                trial++;

                if (trial == SWITCH_HANDS_TRIAL && !flippedHands)
                {
                    // Flip handedness
                    flippedHands = true;

                    if (handedness == "Left")
                    {
                        handedness = "Right";
                        activeHand = rightHand;
                        activeFab = fabRight;
                        activeIndexTip = rightIndexTipJoint; // Update fingertip joint reference
                        fabLeft.SetActive(false);
                        fabRight.SetActive(true);
                    }
                    else
                    {
                        handedness = "Left";
                        activeHand = leftHand;
                        activeFab = fabLeft;
                        activeIndexTip = leftIndexTipJoint; // Update fingertip joint reference
                        fabRight.SetActive(false);
                        fabLeft.SetActive(true);
                    }

                    skeleton = activeHand.GetComponent<OVRSkeleton>();
                    instructionText.text = "Now using the opposite hand!";
                    trialText.GetComponent<TextMeshProUGUI>().text = $"Trial {SWITCH_HANDS_TRIAL}";

                    // Generate new visibility list for second hand
                    int reachesPerArm = TOTAL_TRIALS - SWITCH_HANDS_TRIAL + 1;
                    int visibleTrials = Mathf.CeilToInt(reachesPerArm / 2.0f);  // Half visible (rounded up)
                    int invisibleTrials = reachesPerArm - visibleTrials;        // Half invisible (rounded down)

                    List<bool> visList = new List<bool>();
                    for (int i = 0; i < visibleTrials; i++) visList.Add(true);
                    for (int i = 0; i < invisibleTrials; i++) visList.Add(false);

                    System.Random rng = new System.Random();
                    int n = visList.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = rng.Next(n + 1);
                        bool value = visList[k];
                        visList[k] = visList[n];
                        visList[n] = value;
                    }
                    flippedVisibilityList = visList;

                    phase = Phase.Reach;
                    ShowRandomBall();
                    evaluating = false;
                    return;
                }

                evaluating = false;

                if (trial <= TOTAL_TRIALS)
                {
                    instructionText.text = $"Trial {trial}";
                    trialText.GetComponent<TextMeshProUGUI>().text = $"Trial {trial}";
                    phase = Phase.Reach;
                    ShowRandomBall();
                }
                else
                {
                    instructionText.text = "All trials complete!";
                    SetHandVisibility(true);
                }
            }
        }
    }

    private IEnumerator EvaluateAndLogReachAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 finalHandPos = GetFingertipOrHandPos();
        Vector3 targetPos = currentBall.transform.position;

        Instantiate(explosion, targetPos, Quaternion.identity);
        currentBall.SetActive(false);
        PositionResetCube();
        cube.SetActive(true);
        trialError = Vector3.Distance(finalHandPos, targetPos);

        if (trial >= FIRST_RECORDED_TRIAL && trial <= TOTAL_TRIALS)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            Vector3 headPos = centerEyeAnchor.position;
            Vector3 leftEyeRot = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
            Vector3 rightEyeRot = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);

            //Adjusted indices for visibility list
            bool isVisible = trial < SWITCH_HANDS_TRIAL
                ? randomizedVisibilityList[trial - FIRST_RECORDED_TRIAL]
                : flippedVisibilityList[trial - SWITCH_HANDS_TRIAL];

            string phaseName = isVisible ? "Visible" : "Invisible";

            string log = $"{time}," +
                        $"Trial {trial}," +
                        $"Phase:{phaseName}," +
                        $"isVisible:{isVisible}," +
                        $"Handedness:{handedness}," +
                        $"HandPos({finalHandPos.x:F3},{finalHandPos.y:F3},{finalHandPos.z:F3})," +
                        $"TargetPos({targetPos.x:F3},{targetPos.y:F3},{targetPos.z:F3})," +
                        $"Error({trialError:F3})," +
                        $"HeadPos({headPos.x:F3},{headPos.y:F3},{headPos.z:F3})," +
                        $"LeftEyeRot({leftEyeRot.x:F1},{leftEyeRot.y:F1},{leftEyeRot.z:F1})," +
                        $"RightEyeRot({rightEyeRot.x:F1},{rightEyeRot.y:F1},{rightEyeRot.z:F1})\n";

            File.AppendAllText(trialLogFile, log);
        }

        instructionText.text = "Return to cube.";
        phase = Phase.Reset;
        resetCooldownEndTime = Time.time + resetCooldownDuration;
    }

    private void LogResetReturn()
    {
        if (trial < FIRST_RECORDED_TRIAL || trial > TOTAL_TRIALS) return;

        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        Vector3 handPos = GetFingertipOrHandPos();
        Vector3 cubePos = cube.transform.position;
        float resetError = Vector3.Distance(handPos, cubePos);

        Vector3 headPos = centerEyeAnchor.position;
        Vector3 leftEyeRot = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
        Vector3 rightEyeRot = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);

        string log = $"{time}," +
                     $"Trial Reset Cube," +
                     $"Phase:Reset," +
                     $"isVisible:N/A," +
                     $"Handedness:{handedness}," +
                     $"HandPos({handPos.x:F3},{handPos.y:F3},{handPos.z:F3})," +
                     $"TargetPos({cubePos.x:F3},{cubePos.y:F3},{cubePos.z:F3})," +
                     $"Error({resetError:F3})," +
                     $"HeadPos({headPos.x:F3},{headPos.y:F3},{headPos.z:F3})," +
                     $"LeftEyeRot({leftEyeRot.x:F1},{leftEyeRot.y:F1},{leftEyeRot.z:F1})," +
                     $"RightEyeRot({rightEyeRot.x:F1},{rightEyeRot.y:F1},{rightEyeRot.z:F1})\n";

        File.AppendAllText(trialLogFile, log);
    }

    private Vector3 GetFingertipOrHandPos()
    {
        if (activeIndexTip != null)
        {
            return activeIndexTip.position;
        }
        else
        {
            // Fallback to hand position with forward offset estimation
            Vector3 handPos = activeFab.transform.position;
            Vector3 handForward = activeFab.transform.forward;
            return handPos + handForward * 0.08f; // ~8cm forward from hand center
        }
    }

    private void UpdateFingertipDebugSphere()
    {
        if (showFingertipDebug && fingertipDebugSphere != null)
        {
            Vector3 fingertipPos = GetFingertipOrHandPos();
            fingertipDebugSphere.transform.position = fingertipPos;
            fingertipDebugSphere.SetActive(true);

            // Color coding for tracking quality
            Renderer renderer = fingertipDebugSphere.GetComponent<Renderer>();
            if (activeIndexTip != null)
            {
                renderer.material.color = Color.green; // Direct fingertip tracking
            }
            else
            {
                renderer.material.color = Color.red; // Fallback mode
            }
        }
        else if (fingertipDebugSphere != null)
        {
            fingertipDebugSphere.SetActive(false);
        }
    }

    private void UpdateShoulderDebugSphere()
    {
        if (showShoulderDebug && shoulderDebugSphere != null && initialized)
        {
            // Update shoulder position based on current head position
            Vector3 currentShoulderPos = GetEstimatedShoulderPosition(centerEyeAnchor.position);
            shoulderDebugSphere.transform.position = currentShoulderPos;
            shoulderDebugSphere.SetActive(true);

            // Change color based on phase
            Renderer renderer = shoulderDebugSphere.GetComponent<Renderer>();
            if (phase == Phase.Calibration)
            {
                renderer.material.color = Color.yellow; 
            }
            else
            {
                renderer.material.color = Color.blue; // Blue during normal operation
            }

            if (phase == Phase.Calibration && activeHand != null && activeHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                Vector3 fingertipPos = GetFingertipOrHandPos();
                Debug.DrawLine(currentShoulderPos, fingertipPos, Color.yellow, 0.1f);
            }
        }
        else if (shoulderDebugSphere != null)
        {
            shoulderDebugSphere.SetActive(false);
        }
    }

    private void LogFingertipTrajectory()
    {
        if (!isLoggingTrajectory || currentBall == null)
            return;

        // Check if enough time has passed since last log (based on log rate)
        float timeSinceLastLog = Time.time - lastTrajectoryLogTime;
        float logInterval = 1f / trajectoryLogRate; // Convert Hz to seconds

        if (timeSinceLastLog >= logInterval)
        {
            Vector3 currentFingertipPos = GetFingertipOrHandPos();
            Vector3 targetPos = currentBall.transform.position;

            // Calculate movement metrics
            float distanceToTarget = Vector3.Distance(currentFingertipPos, targetPos);
            Vector3 velocity = (currentFingertipPos - lastFingertipPosition) / timeSinceLastLog;
            float velocityMagnitude = velocity.magnitude;
            Vector3 movementDirection = velocity.normalized;

            // Get current visibility state
            bool isVisible = false;
            if (trial >= FIRST_RECORDED_TRIAL && trial <= TOTAL_TRIALS)
            {
                if (trial < SWITCH_HANDS_TRIAL)
                {
                    int visIndex = trial - FIRST_RECORDED_TRIAL;
                    if (visIndex >= 0 && visIndex < randomizedVisibilityList.Count)
                        isVisible = randomizedVisibilityList[visIndex];
                }
                else
                {
                    int visIndex = trial - SWITCH_HANDS_TRIAL;
                    if (visIndex >= 0 && visIndex < flippedVisibilityList.Count)
                        isVisible = flippedVisibilityList[visIndex];
                }
            }
            else if (trial >= 2 && trial <= 10)
            {
                isVisible = trial < 6; // Practice trials visibility logic
            }

            // Get head and eye data
            Vector3 headPos = centerEyeAnchor.position;
            Vector3 leftEyeRot = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
            Vector3 rightEyeRot = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);

            // Calculate time from trial start (approximate)
            float timeFromTrialStart = Time.time % 60f; // Simple approximation

            // Create detailed trajectory log entry
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string trajectoryLog = $"{time}," +
                                 $"Trial {trial}," +
                                 $"{currentMovementPhase}," +
                                 $"{isVisible}," +
                                 $"{handedness}," +
                                 $"FingertipPos({currentFingertipPos.x:F4},{currentFingertipPos.y:F4},{currentFingertipPos.z:F4})," +
                                 $"TargetPos({targetPos.x:F4},{targetPos.y:F4},{targetPos.z:F4})," +
                                 $"DistanceToTarget({distanceToTarget:F4})," +
                                 $"Velocity({velocityMagnitude:F4})," +
                                 $"HeadPos({headPos.x:F4},{headPos.y:F4},{headPos.z:F4})," +
                                 $"LeftEyeRot({leftEyeRot.x:F2},{leftEyeRot.y:F2},{leftEyeRot.z:F2})," +
                                 $"RightEyeRot({rightEyeRot.x:F2},{rightEyeRot.y:F2},{rightEyeRot.z:F2})," +
                                 $"MovementDirection({movementDirection.x:F3},{movementDirection.y:F3},{movementDirection.z:F3})," +
                                 $"TimeFromTrialStart({timeFromTrialStart:F3})\n";

            try
            {
                File.AppendAllText(fingertipTrajectoryFile, trajectoryLog);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write trajectory data: {ex.Message}");
            }

            // Update tracking variables
            lastTrajectoryLogTime = Time.time;
            lastFingertipPosition = currentFingertipPos;
        }
    }

    private void ShowRandomBall()
    {
        if (currentBall != null)
        {
            Destroy(currentBall); // Clean up the previous one
        }

        currentBall = SpawnRandomSphere();
    }

    private GameObject SpawnRandomSphere()
    {
        // Update shoulder position for current head position
        Vector3 currentHeadPos = centerEyeAnchor.position;
        shoulderPosition = GetEstimatedShoulderPosition(currentHeadPos);

        // Get forward direction from shoulder to where user is looking
        Vector3 gazePoint = currentHeadPos + centerEyeAnchor.forward * calibratedReachDistance;
        Vector3 reachDirection = (gazePoint - shoulderPosition).normalized;

        // Apply the multiplier and offset to the calibrated distance
        float targetDistance = (calibratedReachDistance * targetDistanceMultiplier) - targetDistanceOffset;

        // Base spawn position from shoulder along reach direction
        Vector3 baseSpawnPos = shoulderPosition + (reachDirection * targetDistance);

        // Add random offsets perpendicular to the reach direction
        Vector3 right = Vector3.Cross(reachDirection, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, reachDirection).normalized;

        float xOffset = UnityEngine.Random.Range(-0.2f, 0.2f); // left-right
        float yOffset = UnityEngine.Random.Range(-0.1f, 0.1f); // up-down

        Vector3 spawnPos = baseSpawnPos + (right * xOffset) + (up * yOffset);

        // Log spawn details for debugging
        Debug.Log($"Spawning target at distance {targetDistance:F2}m from shoulder (calibrated: {calibratedReachDistance:F2}m, multiplier: {targetDistanceMultiplier:F2}, offset: {targetDistanceOffset:F2})");
        Debug.Log($"Shoulder position: {shoulderPosition}, Target position: {spawnPos}");

        // Draw debug line from shoulder to target for a moment
        if (showShoulderDebug)
        {
            Debug.DrawLine(shoulderPosition, spawnPos, Color.magenta, 2f);
        }

        // Spawn the prefab
        GameObject newSphere = Instantiate(sphereSpawnPrefab, spawnPos, Quaternion.identity);

        return newSphere;
    }

    private void SetHandVisibility(bool visible)
    {
        if (handedness == "Left")
            fabLeft.SetActive(visible);
        else
            fabRight.SetActive(visible);

        visibilityText.GetComponent<TextMeshProUGUI>().text = visible ? "Visible" : "Invisible";
    }

    // Logs headset, hands, eyes to respective files
    private void LogStandardData()
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        Vector3 headPos = centerEyeAnchor.position;
        Quaternion headRot = centerEyeAnchor.rotation;
        Vector3 leftEyeRot = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
        Vector3 rightEyeRot = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);

        File.AppendAllText(headPosFile, $"{time},{headPos.x},{headPos.y},{headPos.z}\n");
        File.AppendAllText(headRotFile, $"{time},{headRot.eulerAngles.x},{headRot.eulerAngles.y},{headRot.eulerAngles.z}\n");
        File.AppendAllText(leftEyeFile, $"{time},{leftEyeRot.x},{leftEyeRot.y},{leftEyeRot.z}\n");
        File.AppendAllText(rightEyeFile, $"{time},{rightEyeRot.x},{rightEyeRot.y},{rightEyeRot.z}\n");

        if (leftHand != null)
        {
            Vector3 lp = leftHand.transform.position;
            File.AppendAllText(leftHandFile, $"{time},{lp.x},{lp.y},{lp.z}\n");
        }

        if (rightHand != null)
        {
            Vector3 rp = rightHand.transform.position;
            File.AppendAllText(rightHandFile, $"{time},{rp.x},{rp.y},{rp.z}\n");
        }
    }

    private void PositionResetCube()
    {
        // Position cube based on estimated shoulder position and calibrated reach
        shoulderPosition = GetEstimatedShoulderPosition(headPosone);
        Vector3 cubeDirection = (headPosone + forwardone * calibratedReachDistance - shoulderPosition).normalized;

        float cubeDistance = calibratedReachDistance * 0.7f; // Place cube at 70% of full reach
        Vector3 cubePos = shoulderPosition + (cubeDirection * cubeDistance);

        // Add slight downward offset
        cubePos.y -= 0.1f;

        cube.transform.position = cubePos;
    }

    // Converts euler angle to signed [-180, 180]
    Vector3 ConvertToMinus180To180(Vector3 eulerAngles)
    {
        return new Vector3(
            ConvertAngleToMinus180To180(eulerAngles.x),
            ConvertAngleToMinus180To180(eulerAngles.y),
            ConvertAngleToMinus180To180(eulerAngles.z)
        );
    }

    float ConvertAngleToMinus180To180(float angle)
    {
        return (angle > 180) ? angle - 360 : angle;
    }
}