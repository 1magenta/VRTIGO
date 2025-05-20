using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections;


using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.XR;


public class HandednessTouchTest : MonoBehaviour
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
    public GameObject fabLeft;                // Left hand prefab
    public GameObject fabRight;               // Right hand prefab




    public OVREyeGaze LeftEyeGaze;
    public OVREyeGaze RightEyeGaze;




    public StartSystem startMenu;             // Controls recording and playerName


    // OVR References
    public OVRHand leftHand;
    public OVRHand rightHand;
    private OVRSkeleton skeleton;
    private OVRHand activeHand;               // Active hand (left/right depending on detected hand)
    private GameObject activeFab;
    private Transform centerEyeAnchor;



    // State variables
    public string handedness = "";
    private bool handednessDetected = false;


    private bool initialized = false;         // Game logic initialized after handedness is known


    // Trial mechanics
    private int trial = 1;


    private string handednessFile;
    private string headPosFile;
    private string headRotFile;
    private string leftHandFile;
    private string rightHandFile;
    private string leftEyeFile;
    private string rightEyeFile;


    //private string visibleTrialsFile;
    //private string invisibleTrialsFile;
    private string trialLogFile;




    private float zDistance;


    private float reachDistance = 0.3f;  // For calibration
    private Vector3 shoulderOffset;


    private enum Phase { Calibration, Reach, Reset }
    private Phase phase = Phase.Calibration;


    private bool evaluating = false;


    private GameObject currentBall;
    private float trialError = 0f;


    public GameObject sphereSpawnPrefab;


    private float resetCooldownDuration = 2f; // duration in seconds
    private float resetCooldownEndTime = 0f;

    //private float globalDistance = 0.15f;

    private List<bool> randomizedVisibilityList = new List<bool>();

    private bool calibrationComplete = false;
    private bool waitingForCubeAfterCalibration = false;

    private bool flippedHands = false;
    private List<bool> flippedVisibilityList = new List<bool>();

    private Vector3 headPosone;
    private Vector3 forwardone;
    private Vector3 downone;



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

        // Randomize 5 visible and 5 invisible trials between trial 11–20
        List<bool> visList = new List<bool>();
        for (int i = 0; i < 5; i++) visList.Add(true);   // 5 visible
        for (int i = 0; i < 5; i++) visList.Add(false);  // 5 invisible
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


        // Split trial logs
        //visibleTrialsFile = Path.Combine(rootPath, "VisibleTrials.txt");
        //invisibleTrialsFile = Path.Combine(rootPath, "InvisibleTrials.txt");

        // Unified trial log file
        trialLogFile = Path.Combine(rootPath, "TrialData.txt");

        File.AppendAllText(trialLogFile, "Time,Trial,Phase,isVisible,Handedness,HandPos,TargetPos,Error,HeadPos,LeftEyeRot,RightEyeRot,ResetCubePos\n");





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
            }
            else
            {
                fabLeft.SetActive(false);
                fabRight.SetActive(true);
                activeHand = rightHand;
                activeFab = fabRight;
            }
            initialized = true;
            skeleton = activeHand.GetComponent<OVRSkeleton>();

            headPosone = centerEyeAnchor.position;
             forwardone = centerEyeAnchor.forward;
             downone = Vector3.down;




        }


        // Log head/eye/hand data every frame
        LogStandardData();


        if (trial == 1)
        {
            RunCalibrationTrial();
        }
        else if (trial >= 2 && trial <= 10)
        {
            RunPracticeTrials();
        }
        else if (trial >= 11 && trial <= 30)
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
                instructionText.text = "Touch and hold the ball with your palm";
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
            Vector3 handPos = activeFab.transform.position;
            Vector3 headPos = centerEyeAnchor.position;
            zDistance = handPos.z - headPos.z;


            if (zDistance >= 0.1f && !calibrationComplete)
            {
                reachDistance = zDistance;
                shoulderOffset = handPos - headPos;
                calibrationComplete = true;
                waitingForCubeAfterCalibration = true;

                instructionText.text = "Calibration complete! Now touch the green cube to start.";
                PositionResetCube();
                cube.SetActive(true); // Show the cube
            }

        }
    }


    private void RunPracticeTrials()
    {
        Vector3 handPos = GetFingertipOrHandPos();


        if (phase == Phase.Reach)
        {
            instructionText.text = "Touch and hold the ball with your palm";


            SetHandVisibility(trial < 6);


            //The Commented below here is how the ball is detected. the commented one checks if the hand goes within a certain radius of the ball and then waits .5 seconds before registering it. Wherever the hand is after that it calculated the error relative to the ball.

            //float distToBall = Vector3.Distance(handPos, currentBall.transform.position);


            //if (distToBall < globalDistance && !evaluating)
            //{
            //    evaluating = true;
            //    StartCoroutine(EvaluateReachAfterDelay(0.5f)); // Wait .5 second before evaluating
            //}

            //The code below is how the ball was detected before. It tracks the current balls z axis, and as soon as the hand passes that z axis, it triggers the detection.
            if (handPos.z  > currentBall.transform.position.z && !evaluating)
            {
                evaluating = true;
                StartCoroutine(EvaluateReachAfterDelay(0.01f)); // Wait .01 second before evaluating
            }

        }
        else if (phase == Phase.Reset)
        {
            if (Time.time >= resetCooldownEndTime && Vector3.Distance(handPos, cube.transform.position) < 0.15f)
            {
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
                    //instructionText.text = "All trials complete!";
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
            instructionText.text = "Touch and hold the ball with your palm";


            if (trial >= 11 && trial <= 20)
            {
                int visIndex = trial - 11;
                if (visIndex >= 0 && visIndex < randomizedVisibilityList.Count)
                {
                    SetHandVisibility(randomizedVisibilityList[visIndex]);
                }
            }
            else if (trial >= 21 && trial <= 30)
            {
                int visIndex = trial - 21;
                if (visIndex >= 0 && visIndex < flippedVisibilityList.Count)
                {
                    SetHandVisibility(flippedVisibilityList[visIndex]);
                }
            }





            //float distToBall = Vector3.Distance(handPos, currentBall.transform.position);


            //if (distToBall < globalDistance && !evaluating)
            //{
            //    evaluating = true;
            //    StartCoroutine(EvaluateAndLogReachAfterDelay(.5f)); // Evaluate after .5seconds
            //}

            //The code below is how the ball was detected before. It tracks the current balls z axis, and as soon as the hand passes that z axis, it triggers the detection.
            if (handPos.z > currentBall.transform.position.z && !evaluating)
            {
                evaluating = true;
                StartCoroutine(EvaluateAndLogReachAfterDelay(0.01f)); // Wait .01 second before evaluating
            }
        }
        else if (phase == Phase.Reset)
        {
            if (Time.time >= resetCooldownEndTime && Vector3.Distance(handPos, cube.transform.position) < 0.15f)
            {
                LogResetReturn();  // Only called during trials 11–20
                cube.SetActive(false);
                trial++;
                if (trial == 21 && !flippedHands)
                {
                    // Flip handedness
                    flippedHands = true;

                    if (handedness == "Left")
                    {
                        handedness = "Right";
                        activeHand = rightHand;
                        activeFab = fabRight;
                        fabLeft.SetActive(false);
                        fabRight.SetActive(true);
                    }
                    else
                    {
                        handedness = "Left";
                        activeHand = leftHand;
                        activeFab = fabLeft;
                        fabRight.SetActive(false);
                        fabLeft.SetActive(true);
                    }

                    skeleton = activeHand.GetComponent<OVRSkeleton>();
                    instructionText.text = "Now using the opposite hand!";
                    trialText.GetComponent<TextMeshProUGUI>().text = "Trial 21";

                    // Generate new visibility list
                    List<bool> visList = new List<bool>();
                    for (int i = 0; i < 5; i++) visList.Add(true);
                    for (int i = 0; i < 5; i++) visList.Add(false);
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

                if (trial <= 30)
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


        if (trial >= 11 && trial <= 30)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");


            Vector3 headPos = centerEyeAnchor.position;
            Vector3 leftEyeRot = ConvertToMinus180To180(LeftEyeGaze.transform.rotation.eulerAngles);
            Vector3 rightEyeRot = ConvertToMinus180To180(RightEyeGaze.transform.rotation.eulerAngles);


            bool isVisible = trial <= 20
                    ? randomizedVisibilityList[trial - 11]
                    : flippedVisibilityList[trial - 21];


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


            //string trialLogFile = isVisible ? visibleTrialsFile : invisibleTrialsFile;
            //File.AppendAllText(trialLogFile, log);
            Vector3 cubePos = cube.transform.position;

            //log += $"ResetCubePos({cubePos.x:F3},{cubePos.y:F3},{cubePos.z:F3})\n";
            File.AppendAllText(trialLogFile, log);

        }


        instructionText.text = "Return to cube.";
        phase = Phase.Reset;
        resetCooldownEndTime = Time.time + resetCooldownDuration;


    }

    private void LogResetReturn()
    {
        if (trial < 11 || trial > 30) return;  // Only log resets during recorded trials

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
        //if (skeleton != null && skeleton.Bones.Count > 0)
        //{
        //    foreach (var bone in skeleton.Bones)
        //    {
        //        if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
        //        {
        //            // Use local position of the index tip relative to the hand prefab
        //            return activeFab.transform.TransformPoint(bone.Transform.localPosition);
        //        }
        //    }
        //}

        // Fallback
        return activeFab.transform.position;
    }







    // private void ShowRandomBall()
    // {
    //     if (UnityEngine.Random.value < 0.5f)
    //     {
    //         currentBall = leftBall;
    //         leftBall.SetActive(true);
    //         rightBall.SetActive(false);
    //     }
    //     else
    //     {
    //         currentBall = rightBall;
    //         rightBall.SetActive(true);
    //         leftBall.SetActive(false);
    //     }
    // }


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
        Vector3 headPos = centerEyeAnchor.position;
        Vector3 forward = centerEyeAnchor.forward;


        // Base spawn distance in front of the player - calculated as the distance of the pinch - .1
        float distance = zDistance - 0.05f;


        // Random offset range (local x and y)
        float xOffset = UnityEngine.Random.Range(-0.2f, 0.2f); // left-right
        float yOffset = UnityEngine.Random.Range(-0.1f, 0.1f); // up-down


        // Calculate the random spawn position
        Vector3 spawnPos = headPos + (forward * distance) + (centerEyeAnchor.right * xOffset) + (centerEyeAnchor.up * yOffset);


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

        //if (visible)
        //{
        //    globalDistance = .15f;
        //}
        //else
        //{
        //    globalDistance = .25f;
        //}
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
        

        float cubeDistance = zDistance - 0.2f; // slightly closer on z
        float verticalOffset = 0.30f;           // slightly lower on y

        Vector3 cubePos = headPosone + (forwardone * cubeDistance) + (downone * verticalOffset);

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




