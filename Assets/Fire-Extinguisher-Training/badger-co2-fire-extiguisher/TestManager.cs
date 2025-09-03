using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TestManager : MonoBehaviour
{
    [Header("Test Criteria")]
    public FireExtinguisherController extinguisherController;
    public FireObject[] fires;                // All fires in the scene

    [Header("UI Elements")]
    public Text statusText;
    public Text instructionsText;
    public GameObject passPanel;
    public GameObject failPanel;

    [Header("Test Parameters")]
    public float testTimeLimit = 30f;         // Time limit to extinguish all fires

    private float elapsedTime = 0f;
    private bool testStarted = false;
    private bool testCompleted = false;
    private int firesExtinguished = 0;

    // Test criteria tracking
    private bool pinRemovedProperly = false;
    private bool triggerUsedCorrectly = false;
    private bool allFiresExtinguished = false;

    void Start()
    {
        // Initialize UI
        if (instructionsText != null)
        {
            instructionsText.text = "FIRE EXTINGUISHER TEST\n" +
                                  "1. Remove safety pin (Press P or Right Mouse)\n" +
                                  "2. Aim at fire base\n" +
                                  "3. Squeeze trigger (Hold Space or Left Mouse)\n" +
                                  "4. Sweep side to side";
        }

        if (passPanel != null) passPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        StartCoroutine(StartTest());
    }

    IEnumerator StartTest()
    {
        yield return new WaitForSeconds(2f);
        testStarted = true;
        Debug.Log("Fire Extinguisher Test Started!");
    }

    void Update()
    {
        if (!testStarted || testCompleted) return;

        // Track time
        elapsedTime += Time.deltaTime;

        // Update status
        if (statusText != null)
        {
            statusText.text = $"Time: {elapsedTime:F1}s / {testTimeLimit}s\n" +
                            $"Fires Remaining: {fires.Length - firesExtinguished}\n" +
                            $"Foam Remaining: {extinguisherController.GetRemainingSprayPercentage():F0}%\n" +
                            $"Pin Removed: {(extinguisherController.isPinRemoved ? "YES" : "NO")}";
        }

        // Check test criteria
        CheckTestCriteria();

        // Check time limit
        if (elapsedTime >= testTimeLimit && !allFiresExtinguished)
        {
            FailTest("Time limit exceeded!");
        }
    }

    void CheckTestCriteria()
    {
        // Check if pin was removed
        if (extinguisherController.isPinRemoved && !pinRemovedProperly)
        {
            pinRemovedProperly = true;
            Debug.Log("✓ Safety pin removed correctly");
        }

        // Check if trigger is being used after pin removal
        if (extinguisherController.isPinRemoved && extinguisherController.isTriggerPressed)
        {
            triggerUsedCorrectly = true;
            Debug.Log("✓ Trigger operated correctly");
        }

        // Check if someone tries to use trigger before removing pin
        if (!extinguisherController.isPinRemoved && Input.GetMouseButton(0))
        {
            Debug.LogWarning("⚠ Cannot use extinguisher - safety pin still in place!");
        }
    }

    public void OnTestPassed()
    {
        firesExtinguished++;

        if (firesExtinguished >= fires.Length)
        {
            allFiresExtinguished = true;
            PassTest();
        }
    }

    void PassTest()
    {
        if (testCompleted) return;

        testCompleted = true;
        Debug.Log("===== TEST PASSED =====");
        Debug.Log($"Time taken: {elapsedTime:F1} seconds");
        Debug.Log($"Foam used: {(100f - extinguisherController.GetRemainingSprayPercentage()):F0}%");

        // Validate all criteria
        bool allCriteriaMet = pinRemovedProperly && triggerUsedCorrectly && allFiresExtinguished;

        if (allCriteriaMet)
        {
            if (passPanel != null)
            {
                passPanel.SetActive(true);
                Text passText = passPanel.GetComponentInChildren<Text>();
                if (passText != null)
                {
                    passText.text = "TEST PASSED!\n\n" +
                                  "✓ Safety pin removed\n" +
                                  "✓ Trigger operated correctly\n" +
                                  "✓ All fires extinguished\n" +
                                  $"✓ Time: {elapsedTime:F1}s\n" +
                                  $"✓ Efficiency: {(100f - extinguisherController.GetRemainingSprayPercentage()):F0}% foam used";
                }
            }
        }
        else
        {
            FailTest("Not all criteria met");
        }
    }

    void FailTest(string reason)
    {
        if (testCompleted) return;

        testCompleted = true;
        Debug.Log($"===== TEST FAILED: {reason} =====");

        if (failPanel != null)
        {
            failPanel.SetActive(true);
            Text failText = failPanel.GetComponentInChildren<Text>();
            if (failText != null)
            {
                failText.text = $"TEST FAILED\n\nReason: {reason}\n\n" +
                              $"Pin Removed: {(pinRemovedProperly ? "✓" : "✗")}\n" +
                              $"Trigger Used: {(triggerUsedCorrectly ? "✓" : "✗")}\n" +
                              $"Fires Extinguished: {firesExtinguished}/{fires.Length}";
            }
        }
    }

    // Reset test
    public void RestartTest()
    {
        // Reset extinguisher
        extinguisherController.Reset();

        // Reset test variables
        elapsedTime = 0f;
        testStarted = false;
        testCompleted = false;
        firesExtinguished = 0;
        pinRemovedProperly = false;
        triggerUsedCorrectly = false;
        allFiresExtinguished = false;

        // Restart scene or reset fires
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}