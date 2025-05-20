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
