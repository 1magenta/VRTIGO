/*using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class StartSystem : MonoBehaviour
{
    public bool running;
    public bool recording;
    public Image uiImageRunning;
    public Image uiImageRecording;
    public static string playerName;
    public TMP_InputField playerNameInput;  // Change to TMP_InputField
    public string phase;

    private GameObject SceneCode;


    void Start()
    {
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(SavePlayerName);
            playerName = string.IsNullOrEmpty(playerName) ? "player" : playerName;
            playerNameInput.text = playerName; // Default to "player" if null
            Debug.Log(playerName);
        }
        phase = PlayerPrefs.GetString("Phase", "Tutorial");

        SceneCode = GameObject.Find("SceneCode");

    }

    public void StartProcedure()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "StartMenu")
        {
            SceneManager.LoadScene("BucketTestV2");
            phase = "Tutorial";
            PlayerPrefs.SetString("Phase", phase);
            PlayerPrefs.Save();
        }
        else if (currentScene == "BucketTestV2")
        {
            if (SceneCode != null)
            {
                Debug.Log("Check1");
                if (phase == "Final")
                {
                    phase = "Tutorial";
                    PlayerPrefs.SetString("Phase", phase);
                    PlayerPrefs.Save();
                    recording = false;
                    running = false;
                    SceneManager.LoadScene("TestofNystagmus");
                }
                else if (phase == "Round3" || phase == "Round2" || phase == "Round1")
                {
                    StartCoroutine(DelaySceneCodeChange());
                }
                else if(phase == "Tutorial")
                {
                    Debug.Log("Check2");
                    SceneCode.SetActive(true);
                    running = true;
                    Debug.Log("HeadPositionData found and activated.");
                    phase = "Round1";
                }
            }
            else
            {
                Debug.LogWarning("HeadPositionData not found in the scene.");
            }
        }
        else if (currentScene == "TestofNystagmus")
        {
            if (SceneCode != null)
            {
                Debug.Log("Check1");
                if (phase == "Round2")
                {
                    phase = "Tutorial";
                    PlayerPrefs.SetString("Phase", phase);
                    PlayerPrefs.Save();
                    recording = false;
                    running = false;
                    SceneManager.LoadScene("FingerTapping");
                }
                else if (phase == "Round1")
                {
                    StartCoroutine(DelaySceneCodeChange());
                
                }
                else if(phase == "Tutorial")
                {
                    SceneCode.SetActive(false);
                    Debug.Log("Check2");
                    running = true;
                    SceneCode.SetActive(true);
                    
                    Debug.Log("HeadPositionData found and activated.");
                    phase = "Round1";
                }
            }
            else
            {
                Debug.LogWarning("HeadPositionData not found in the scene.");
            }
        }
        else if (currentScene == "FingerTapping")
        {
            if (SceneCode != null)
            {
                Debug.Log("Check1");
                if (phase == "Round3")
                {
                    phase = "Tutorial";
                    PlayerPrefs.SetString("Phase", phase);
                    PlayerPrefs.Save();
                    recording = false;
                    running = false;
                    SceneManager.LoadScene("TestofSkew");
                }
                else if (phase == "Round2" || phase == "Round1")
                {
                    StartCoroutine(DelaySceneCodeChange());
                }
                else if(phase == "Tutorial")
                {
                    SceneCode.SetActive(false);
                    Debug.Log("Check2");
                    running = true;
                    SceneCode.SetActive(true);
                    Debug.Log("HeadPositionData found and activated.");
                    phase = "Round1";
                }
            }
            else
            {
                Debug.LogWarning("HeadPositionData not found in the scene.");
            }
        }
        else if (currentScene == "TestofSkew")
        {
            if (SceneCode != null)
            {
                Debug.Log("Check1");
                if (phase == "Round1")
                {
                    phase = "Tutorial";
                    PlayerPrefs.SetString("Phase", phase);
                    PlayerPrefs.Save();
                    recording = false;
                    running = false;
                    SceneManager.LoadScene("FingerTarget");
                }
                else if(phase == "Tutorial")
                {
                    SceneCode.SetActive(false);
                    Debug.Log("Check2");
                    running = true;
                    recording = true;
                    SceneCode.SetActive(true);
                    Debug.Log("HeadPositionData found and activated.");
                    phase = "Round1";
                }
            }
            else
            {
                Debug.LogWarning("HeadPositionData not found in the scene.");
            }
        }
        else if (currentScene == "FingerTarget")
        {
            if (SceneCode != null)
            {
                Debug.Log("Check1");
                if (phase == "Round1")
                {
                    phase = "Tutorial";
                    PlayerPrefs.SetString("Phase", phase);
                    PlayerPrefs.Save();
                    recording = false;
                    running = false;
                    SceneManager.LoadScene("HeadStability");
                }
                else if (phase == "Tutorial")
                {
                
                    running = true;
                    recording = true;
                    SceneCode.SetActive(true);
                    phase = "Round1";
                }
            }
            else
            {
                Debug.LogWarning("HeadPositionData not found in the scene.");
            }
        }
        else if (currentScene == "HeadStability")
        {
            if (SceneCode != null)
            {
                Debug.Log("Check1");
                if (phase == "Round1")
                {
                    phase = "Tutorial";
                    PlayerPrefs.SetString("Phase", phase);
                    PlayerPrefs.Save();
                    recording = false;
                    running = false;
                    SceneManager.LoadScene("StartMenu");
                }
                else if(phase == "Tutorial")
                {
                    SceneCode.SetActive(false);
                    Debug.Log("Check2");
                    running = true;
                    recording = true;
                    SceneCode.SetActive(true);
                    Debug.Log("HeadPositionData found and activated.");
                    phase = "Round1";
                }
            }
            else
            {
                Debug.LogWarning("HeadPositionData not found in the scene.");
            }
        }
        else
        {
            Debug.Log("Skip button pressed, but no matching scene to skip.");
        }
    }

    private IEnumerator DelaySceneCodeChange()
    {
        SceneCode.SetActive(false);  // Turn off SceneCode
        yield return new WaitForSeconds(1);  // Wait for 1 second
        recording = true;
        SceneCode.SetActive(true);  // Turn on SceneCode
        
        phase = phase == "Round1" ? "Round2" : phase == "Round2" ? "Round3" : "Final";
    }
    void Update()
    {
        if (uiImageRunning != null)
        {
            uiImageRunning.color = running ? Color.green : Color.red;
        }

        if (uiImageRecording != null)
        {
            uiImageRecording.color = recording ? Color.green : Color.red;
        }
    }

    public void SavePlayerName(string name)
    {
        playerName = name;
        Debug.Log("Player Name Saved: " + playerName);
    }
}
*/


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartSystem : MonoBehaviour
{
    public bool running;
    public bool recording;
    public Image uiImageRunning;
    public Image uiImageRecording;
    public static string playerName;
    public TMP_InputField playerNameInput;
    public string phase;

    private GameObject SceneCode;

    // Scene sequence - easy to modify
    private readonly string[] sceneSequence = {
        "StartMenu",
        "HeadStability",
        "TestofSkew",
        "TestofNystagmus",
        "FingerTapping",
        "BucketTestV2",
        "FingerTarget"
    };

    void Start()
    {
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(SavePlayerName);
            playerName = string.IsNullOrEmpty(playerName) ? "player" : playerName;
            playerNameInput.text = playerName;
            Debug.Log(playerName);
        }
        phase = PlayerPrefs.GetString("Phase", "Tutorial");
        SceneCode = GameObject.Find("SceneCode");
    }

    // Helper method to get next scene in sequence
    private string GetNextScene(string currentScene)
    {
        for (int i = 0; i < sceneSequence.Length; i++)
        {
            if (sceneSequence[i] == currentScene)
            {
                if (i + 1 < sceneSequence.Length)
                {
                    return sceneSequence[i + 1];
                }
                else
                {
                    return "StartMenu"; // Loop back to start
                }
            }
        }
        return null; // Scene not found in sequence
    }

    // Helper method to load next scene and reset phase
    private void LoadNextSceneInSequence()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = GetNextScene(currentScene);

        if (nextScene != null)
        {
            phase = "Tutorial";
            PlayerPrefs.SetString("Phase", phase);
            PlayerPrefs.Save();
            recording = false;
            running = false;
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning($"Current scene '{currentScene}' not found in sequence.");
        }
    }

    public void StartProcedure()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "StartMenu")
        {
            HandleStartMenu();
        }
        else if (currentScene == "HeadStability")
        {
            HandleHeadStability();
        }
        else if (currentScene == "TestofSkew")
        {
            HandleTestofSkew();
        }
        else if (currentScene == "TestofNystagmus")
        {
            HandleTestofNystagmus();
        }
        else if (currentScene == "FingerTapping")
        {
            HandleFingerTapping();
        }
        else if (currentScene == "BucketTestV2")
        {
            HandleBucketTestV2();
        }
        else if (currentScene == "FingerTarget")
        {
            HandleFingerTarget();
        }
        else
        {
            Debug.Log("Skip button pressed, but no matching scene to skip.");
        }
    }

    private void HandleStartMenu()
    {
        LoadNextSceneInSequence();
    }

    private void HandleHeadStability()
    {
        if (SceneCode != null)
        {
            Debug.Log("Check1");
            if (phase == "Round1")
            {
                LoadNextSceneInSequence();
            }
            else if (phase == "Tutorial")
            {
                SceneCode.SetActive(false);
                Debug.Log("Check2");
                running = true;
                recording = true;
                SceneCode.SetActive(true);
                Debug.Log("HeadPositionData found and activated.");
                phase = "Round1";
            }
        }
        else
        {
            Debug.LogWarning("HeadPositionData not found in the scene.");
        }
    }

    private void HandleTestofSkew()
    {
        if (SceneCode != null)
        {
            Debug.Log("Check1");
            if (phase == "Round1")
            {
                LoadNextSceneInSequence();
            }
            else if (phase == "Tutorial")
            {
                SceneCode.SetActive(false);
                Debug.Log("Check2");
                running = true;
                recording = true;
                SceneCode.SetActive(true);
                Debug.Log("HeadPositionData found and activated.");
                phase = "Round1";
            }
        }
        else
        {
            Debug.LogWarning("HeadPositionData not found in the scene.");
        }
    }

    private void HandleTestofNystagmus()
    {
        if (SceneCode != null)
        {
            Debug.Log("Check1");
            if (phase == "Round2")
            {
                LoadNextSceneInSequence();
            }
            else if (phase == "Round1")
            {
                StartCoroutine(DelaySceneCodeChange());
            }
            else if (phase == "Tutorial")
            {
                SceneCode.SetActive(false);
                Debug.Log("Check2");
                running = true;
                SceneCode.SetActive(true);
                Debug.Log("HeadPositionData found and activated.");
                phase = "Round1";
            }
        }
        else
        {
            Debug.LogWarning("HeadPositionData not found in the scene.");
        }
    }

    private void HandleFingerTapping()
    {
        if (SceneCode != null)
        {
            Debug.Log("Check1");
            if (phase == "Round3")
            {
                LoadNextSceneInSequence();
            }
            else if (phase == "Round2" || phase == "Round1")
            {
                StartCoroutine(DelaySceneCodeChange());
            }
            else if (phase == "Tutorial")
            {
                SceneCode.SetActive(false);
                Debug.Log("Check2");
                running = true;
                SceneCode.SetActive(true);
                Debug.Log("HeadPositionData found and activated.");
                phase = "Round1";
            }
        }
        else
        {
            Debug.LogWarning("HeadPositionData not found in the scene.");
        }
    }

    private void HandleBucketTestV2()
    {
        if (SceneCode != null)
        {
            Debug.Log("Check1");
            if (phase == "Final")
            {
                LoadNextSceneInSequence();
            }
            else if (phase == "Round3" || phase == "Round2" || phase == "Round1")
            {
                StartCoroutine(DelaySceneCodeChange());
            }
            else if (phase == "Tutorial")
            {
                Debug.Log("Check2");
                SceneCode.SetActive(true);
                running = true;
                Debug.Log("HeadPositionData found and activated.");
                phase = "Round1";
            }
        }
        else
        {
            Debug.LogWarning("HeadPositionData not found in the scene.");
        }
    }

    private void HandleFingerTarget()
    {
        if (SceneCode != null)
        {
            Debug.Log("Check1");
            if (phase == "Round1")
            {
                LoadNextSceneInSequence();
            }
            else if (phase == "Tutorial")
            {
                running = true;
                recording = true;
                SceneCode.SetActive(true);
                phase = "Round1";
            }
        }
        else
        {
            Debug.LogWarning("HeadPositionData not found in the scene.");
        }
    }

    private IEnumerator DelaySceneCodeChange()
    {
        SceneCode.SetActive(false);
        yield return new WaitForSeconds(1);
        recording = true;
        SceneCode.SetActive(true);

        phase = phase == "Round1" ? "Round2" : phase == "Round2" ? "Round3" : "Final";
    }

    void Update()
    {
        if (uiImageRunning != null)
        {
            uiImageRunning.color = running ? Color.green : Color.red;
        }

        if (uiImageRecording != null)
        {
            uiImageRecording.color = recording ? Color.green : Color.red;
        }
    }

    public void SavePlayerName(string name)
    {
        playerName = name;
        Debug.Log("Player Name Saved: " + playerName);
    }
}