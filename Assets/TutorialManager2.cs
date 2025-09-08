using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add TextMeshPro namespace
using System.Collections;

public class TutorialManager2 : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI tutorialText; // Changed from Text to TextMeshProUGUI
    public GameObject tutorialPanel; // The panel containing the text
    
    [Header("Tutorial Settings")]
    public bool startOnAwake = true;
    
    // Tutorial messages and their display durations
    private TutorialStep[] tutorialSteps = new TutorialStep[]
    {
        new TutorialStep("Good Morning, Tester !", 5f),
        new TutorialStep("We Are Group FourZeroFour 404 !", 5f),
        new TutorialStep("This Project is build by Ong Yi Sheng, Desmond Ho Jia Shen, Tharini A/P Vijesh Kumar and Nga Ke Jie", 5f),
        new TutorialStep("Welcome To Our XR Assignment !", 5f),
        new TutorialStep("Now You Are in The Chemical Lab", 5f),
        new TutorialStep("Now Grab the Lighter at your right side with Grab or Button G", 5f),
        new TutorialStep("Now Collides the Lighter head with the Red Barrel infront of you to activate fire", 30f),
        new TutorialStep("Now Grab the Fire Extinguisher on your left with Grab or button G", 30f),
        new TutorialStep("Press Button P or Plug the Ring from the Fire Extinguisher", 30f),
        new TutorialStep("Now Face the Fire Extinguisher to the Fire Object!", 30f),
        new TutorialStep("Congratulations you finish the tutorial ! Enjoy the others experience by yourself!", -1f) // -1 means permanent
    };
    
    private int currentStepIndex = 0;
    private bool tutorialCompleted = false;
    
    void Start()
    {
        if (startOnAwake)
        {
            StartTutorial();
        }
    }
    
    public void StartTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
            
        StartCoroutine(RunTutorial());
    }
    
    IEnumerator RunTutorial()
    {

        yield return new WaitForSeconds(90f); // 90 seconds = 1 min 30 sec

        foreach (TutorialStep step in tutorialSteps)
        {
            // Display the message
            DisplayMessage(step.message);
            
            // If duration is -1, keep it permanent (final message)
            if (step.duration < 0)
            {
                tutorialCompleted = true;
                yield break; // Stop the tutorial here
            }
            
            // Wait for the specified duration
            yield return new WaitForSeconds(step.duration);
            
            // Hide message (except for the last one)
            HideMessage();
            
            // Small gap between messages
            yield return new WaitForSeconds(0.5f);
            
            currentStepIndex++;
        }
    }
    
    void DisplayMessage(string message)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
        }
    }
    
    void HideMessage()
    {
        if (tutorialText != null && !tutorialCompleted)
        {
            tutorialText.text = "";
            // Optionally hide the panel
            // if (tutorialPanel != null)
            //     tutorialPanel.SetActive(false);
        }
    }
    
    // Public methods for manual control
    public void NextStep()
    {
        if (currentStepIndex < tutorialSteps.Length - 1)
        {
            StopAllCoroutines();
            currentStepIndex++;
            StartCoroutine(ShowCurrentStep());
        }
    }
    
    public void SkipTutorial()
    {
        StopAllCoroutines();
        tutorialCompleted = true;
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
    
    IEnumerator ShowCurrentStep()
    {
        TutorialStep step = tutorialSteps[currentStepIndex];
        DisplayMessage(step.message);
        
        if (step.duration > 0)
        {
            yield return new WaitForSeconds(step.duration);
            HideMessage();
        }
    }
    
    // Method to manually trigger specific tutorial steps (useful for debugging)
    public void ShowStep(int stepIndex)
    {
        if (stepIndex >= 0 && stepIndex < tutorialSteps.Length)
        {
            currentStepIndex = stepIndex;
            DisplayMessage(tutorialSteps[stepIndex].message);
        }
    }
}

[System.Serializable]
public class TutorialStep
{
    public string message;
    public float duration; // -1 for permanent display
    
    public TutorialStep(string msg, float dur)
    {
        message = msg;
        duration = dur;
    }
}