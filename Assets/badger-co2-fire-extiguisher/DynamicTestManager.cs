using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DynamicTestManager : MonoBehaviour
{
    [Header("Test Criteria")]
    public FireExtinguisherController extinguisherController;
    public List<FireObject> activeFires = new List<FireObject>();

    [Header("UI Elements")]
    public Text statusText;

    private float elapsedTime = 0f;
    private bool testStarted = false;
    private bool testCompleted = false;
    private int totalFiresCreated = 0;

    void Start()
    {
        Debug.Log("🚒 Simple Fire Test Started!");
        testStarted = true;
    }

    void Update()
    {
        if (!testStarted || testCompleted) return;

        elapsedTime += Time.deltaTime;

        // Remove destroyed fires
        activeFires.RemoveAll(fire => fire == null);

        // Count active fires
        int activeBurningFires = 0;
        foreach (FireObject fire in activeFires)
        {
            if (fire != null && !fire.IsExtinguished())
                activeBurningFires++;
        }

        // Update UI
        if (statusText != null)
        {
            statusText.text = $"Time: {elapsedTime:F1}s\n" +
                            $"Fires Created: {totalFiresCreated}\n" +
                            $"Fires Burning: {activeBurningFires}\n" +
                            $"Pin Removed: {(extinguisherController.isPinRemoved ? "✅" : "❌")}\n" +
                            $"Foam: {extinguisherController.GetRemainingSprayPercentage():F0}%";
        }

        // Check if all fires are out
        if (totalFiresCreated > 0 && activeBurningFires == 0)
        {
            PassTest();
        }
    }

    public void RegisterNewFire(FireObject fire)
    {
        if (fire != null && !activeFires.Contains(fire))
        {
            activeFires.Add(fire);
            totalFiresCreated++;
            Debug.Log($"🔥 New fire registered! Total: {totalFiresCreated}");
        }
    }

    public void OnFireExtinguished(FireObject fire)
    {
        Debug.Log("🚿 Fire reported as extinguished!");
    }

    void PassTest()
    {
        if (testCompleted) return;
        
        testCompleted = true;
        Debug.Log("🏆 TEST PASSED! All fires extinguished!");
        
        if (statusText != null)
        {
            statusText.text = "🏆 TEST PASSED!\n" +
                            $"Time: {elapsedTime:F1}s\n" +
                            "All fires extinguished!";
        }
    }
}