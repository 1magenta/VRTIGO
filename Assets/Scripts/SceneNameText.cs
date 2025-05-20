using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneNameText : MonoBehaviour
{
    public TextMeshProUGUI sceneText;

    void Start()
    {
        if (sceneText == null)
        {
            Debug.LogError("TextMeshProUGUI reference is missing!");
            return;
        }
        
        // Update the text with the current scene name
        sceneText.text = SceneManager.GetActiveScene().name;
    }
}
