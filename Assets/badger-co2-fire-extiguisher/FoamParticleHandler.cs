using UnityEngine;

public class FoamParticleHandler : MonoBehaviour
{
    void OnParticleCollision(GameObject other)
    {
        // Find the FireObject component on the collided object
        FireObject fire = other.GetComponent<FireObject>();
        if (fire != null)
        {
            Debug.Log($"Foam particle hit fire: {other.name}");
            
            // Notify the test manager and extinguish the fire
            DynamicTestManager testManager = FindObjectOfType<DynamicTestManager>();
            if (testManager != null)
            {
                testManager.OnFireExtinguished(fire);
            }
            
            other.SetActive(false);
        }
    }
}