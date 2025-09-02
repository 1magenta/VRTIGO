// 1. Create GlobalTestManager.cs - Manages data folders and test sequence
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using TMPro;

public class GlobalTestManager : MonoBehaviour
{
    [Header("Test Sequence")]
    public string[] testScenes = {
        "HeadStability",
        "TestofNystagmus",
        "TestofSkew"
    };

    [Header("Participant Info")]
    public static string participantID = "P1";
    public static string sessionFolder = "";

    // Singleton pattern
    public static GlobalTestManager Instance;
    private int currentTestIndex = 0;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSession()
    {
        // Create main session folder
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        sessionFolder = Path.Combine(Application.persistentDataPath, $"{participantID}_{timestamp}");

        try
        {
            Directory.CreateDirectory(sessionFolder);
            Debug.Log($"Session folder created: {sessionFolder}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create session folder: {ex.Message}");
        }
    }

    public static string GetTestDataPath(string testName)
    {
        if (string.IsNullOrEmpty(sessionFolder)) return "";

        string testPath = Path.Combine(sessionFolder, testName);
        Directory.CreateDirectory(testPath);
        return testPath;
    }

    public void StartTests()
    {
        currentTestIndex = 0;
        LoadNextTest();
    }

    public void CompleteCurrentTest()
    {
        currentTestIndex++;
        if (currentTestIndex < testScenes.Length)
        {
            LoadNextTest();
        }
        else
        {
            CompleteAllTests();
        }
    }

    void LoadNextTest()
    {
        if (currentTestIndex < testScenes.Length)
        {
            Debug.Log($"Loading test: {testScenes[currentTestIndex]}");
            SceneManager.LoadScene(testScenes[currentTestIndex]);
        }
    }

    void CompleteAllTests()
    {
        Debug.Log("All tests completed!");
        SceneManager.LoadScene("StartScene"); // Return to start
    }

    public string GetCurrentTestName()
    {
        if (currentTestIndex < testScenes.Length)
            return testScenes[currentTestIndex];
        return "Unknown";
    }

    public int GetCurrentTestIndex()
    {
        return currentTestIndex;
    }

    public int GetTotalTests()
    {
        return testScenes.Length;
    }
}

