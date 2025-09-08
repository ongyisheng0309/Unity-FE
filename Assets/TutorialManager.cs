using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI tutorialText;
    
    // Tutorial messages and their display durations
    private string[] messages = new string[]
    {
        "Good Morning, Tester !",
        "We Are Group FourZeroFour 404 !",
        "This Project is build by Ong Yi Sheng, Desmond Ho Jia Shen, Tharini A/P Vijesh Kumar and Nga Ke Jie",
        "Welcome To Our XR Assignment !",
        "Now You Are in The Chemical Lab",
        "Now Grab the Lighter at your right side with Grab or Button G",
        "Now Collides the Lighter head with the Red Barrel infront of you to activate fire",
        "Now Grab the Fire Extinguisher on your left with Grab or button G",
        "Press Button P or Plug the Ring from the Fire Extinguisher",
        "Now Face the Fire Extinguisher to the Fire Object!",
        "Congratulations you finish the tutorial ! Enjoy the others experience by yourself!"
    };
    
    private float[] durations = new float[]
    {
        15f, 15f, 15f, 15f, 15f, 15f, 30f, 30f, 30f, 30f, -1f
    };
    
    void Start()
    {
        StartCoroutine(ShowMessages());
    }
    
    IEnumerator ShowMessages()
    {
        for (int i = 0; i < messages.Length; i++)
        {
            // Show message
            tutorialText.text = messages[i];
            
            // If it's the last message (duration -1), keep it forever
            if (durations[i] < 0)
            {
                yield break;
            }
            
            // Wait for the duration
            yield return new WaitForSeconds(durations[i]);
        }
    }
}