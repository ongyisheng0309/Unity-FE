using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamZone : MonoBehaviour
{
    private FireExtinguisherController extinguisher;

    void Start()
    {
        // Find the extinguisher controller
        extinguisher = GetComponentInParent<FireExtinguisherController>();
        
        // Add a sphere collider
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.radius = 1.5f; // Make it big enough to reach the fire
        col.isTrigger = true;
        
        // Tag it as foam
        gameObject.tag = "Untagged";
        
        Debug.Log("FoamZone created on hose tip");
    }

    void Update()
    {
        // Only active when actually spraying
        bool shouldBeActive = extinguisher != null && extinguisher.isTriggerPressed;
        GetComponent<Collider>().enabled = shouldBeActive;
        
        if (shouldBeActive)
        {
            Debug.Log("FoamZone is active and spraying");
        }
    }
}