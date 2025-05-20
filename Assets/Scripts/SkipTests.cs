using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipTests : MonoBehaviour
{
    public void SkipToNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "StartMenu")
        {
            SceneManager.LoadScene("BucketTestV2");
        }
        else if (currentScene == "BucketTestV2")
        {
            SceneManager.LoadScene("TestofNystagmus");
        }
        else if (currentScene == "TestofNystagmus")
        {
            SceneManager.LoadScene("FingerTapping");
        }
        else if (currentScene == "FingerTapping")
        {
            SceneManager.LoadScene("TestofSkew");
        }
        else if (currentScene == "TestofSkew")
        {
            SceneManager.LoadScene("FingerTarget");
        }
        else if (currentScene == "FingerTarget")
        {
            SceneManager.LoadScene("HeadStability");
        }
        else if (currentScene == "HeadStability")
        {
            SceneManager.LoadScene("StartMenu");
        }
        else
        {
            Debug.Log("Skip button pressed, but no matching scene to skip.");
        }
    }
}
