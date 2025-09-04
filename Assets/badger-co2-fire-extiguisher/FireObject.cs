using UnityEngine;

public class FireObject : MonoBehaviour
{
    void Start()
    {
        // Make sure fire has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Create a sphere collider specifically
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 2f; // Make it big for testing
            sphereCol.isTrigger = true;
            Debug.Log($"Added SphereCollider to {gameObject.name}");
        }
        else
        {
            col.isTrigger = true;
            Debug.Log($"Using existing collider on {gameObject.name}");
        }
        
        Debug.Log($"Fire ready to be extinguished: {gameObject.name}");
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log($"Something touching fire: {other.name} (Tag: '{other.tag}')");
        
        // If foam touches fire, make fire disappear
        if (other.CompareTag("ExtinguisherFoam"))
        {
            Debug.Log($"FIRE EXTINGUISHED! {gameObject.name} hit by {other.name}");
            
            // Tell test manager fire is out
            DynamicTestManager testManager = FindObjectOfType<DynamicTestManager>();
            if (testManager != null)
            {
                testManager.OnFireExtinguished(this);
            }
            
            // Make fire disappear
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Not foam - expected 'ExtinguisherFoam' but got '{other.tag}'");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered fire area: {other.name} (Tag: '{other.tag}')");
        
        if (other.CompareTag("ExtinguisherFoam"))
        {
            Debug.Log($"Foam entered fire area: {other.name}");
        }
    }

    // // Handle particle collisions too
    // void OnParticleCollision(GameObject other)
    // {
    //     Debug.Log($"PARTICLE HIT FIRE: {other.name} (Tag: '{other.tag}')");

    //     // Check the tag of the GameObject that owns the ParticleSystem
    //     if (other.CompareTag("ExtinguisherFoam"))
    //     {
    //         Debug.Log($"FOAM PARTICLE HIT! Extinguishing fire...");
            
    //         DynamicTestManager testManager = FindObjectOfType<DynamicTestManager>();
    //         if (testManager != null)
    //         {
    //             testManager.OnFireExtinguished(this);
    //         }
            
    //         gameObject.SetActive(false);
    //     }
    //     else
    //     {
    //         Debug.Log($"Not foam - expected 'ExtinguisherFoam' but got '{other.tag}'");
    //     }
    // }

    // Method for test manager compatibility
    public bool IsExtinguished()
    {
        return !gameObject.activeInHierarchy;
    }
}