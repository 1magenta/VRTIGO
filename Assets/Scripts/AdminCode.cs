using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class AdminCode : MonoBehaviour
{
    private string currentScene;
    //private int counter = 0;

    //public TextMeshProUGUI HeadPosX;


    // Start is called before the first frame update
    void Start()
    {
        

       
    }

    // Update is called once per frame
    void Update()
    {
        currentScene = SceneManager.GetActiveScene().name;
        
        if (OVRInput.GetUp(OVRInput.Button.Three))
        {
           
            if (currentScene == "StartMenu")
            {
                
                SceneManager.LoadScene("BucketTestV2");
            }

            if (currentScene == "BucketTestV2")
            {
                 GameObject tutorial = GameObject.Find("Tutorial");
                // GameObject sourceCode = GameObject.Find("ControlPanel");
                // BucketTest bucketTest; // Reference to the BucketTest script
                // bucketTest = sourceCode.GetComponent<BucketTest>();
                
                if (tutorial.activeSelf)
                {
                     tutorial.SetActive(false);
                //     bucketTest.enabled = true;
                //     //counter++;
                    
                } 
                // else if (counter <= 2)
                // {
                //     bucketTest.enabled = false;
                //     bucketTest.enabled = true;
                //     counter++;
                // }
                else{
                    SceneManager.LoadScene("TestofNystagmus");
                }
            }

            if (currentScene == "TestofNystagmus")
            {
                SceneManager.LoadScene("FingerTapping");
            }
            if (currentScene == "FingerTapping")
            {
                SceneManager.LoadScene("TestofSkew");
            }
            // if (currentScene == "TestofSkew")
            // {
            //     SceneManager.LoadScene("FingerTarget");
            // }
            if (currentScene == "TestofSkew")
            {
                SceneManager.LoadScene("HeadStability");
            }
            if (currentScene == "HeadStability")
            {
                SceneManager.LoadScene("StartMenu");
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            //OVRManager.display.RecenterPose();
            

        }

    }
}


