using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds collision detection to extinguisher smoke particles.
/// Creates invisible sphere colliders that follow the smoke particles to interact with fire.
/// </summary>
public class ExtinguisherSmokeCollider : MonoBehaviour
{
    [Header("Smoke Particle System")]
    public ParticleSystem smokeParticles;
    
    [Header("Collision Settings")]
    public float colliderRadius = 0.5f; // Size of invisible colliders
    public int maxColliders = 20; // Maximum number of colliders to create
    public float colliderLifetime = 2f; // How long each collider lives
    
    private List<SmokeCollider> activeColliders = new List<SmokeCollider>();
    private Queue<SmokeCollider> colliderPool = new Queue<SmokeCollider>();
    
    void Start()
    {
        if (smokeParticles == null)
            smokeParticles = GetComponent<ParticleSystem>();
        
        // Pre-create collider pool
        CreateColliderPool();
    }
    
    void Update()
    {
        // Always clean up expired colliders, regardless of spray state
        CleanupExpiredColliders();
        
        // Only update colliders if smoke particles are actively playing
        if (smokeParticles != null && smokeParticles.isPlaying)
        {
            UpdateSmokeColliders();
        }
    }
    
    void CreateColliderPool()
    {
        for (int i = 0; i < maxColliders; i++)
        {
            GameObject colliderObj = new GameObject($"SmokeCollider_{i}");
            colliderObj.transform.SetParent(transform);
            
            // Add sphere collider and set as trigger
            SphereCollider sphereCollider = colliderObj.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = colliderRadius;
            
            // Add rigidbody for collision detection
            Rigidbody rb = colliderObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            // Tag it so fire can recognize it
            colliderObj.tag = "ExtinguisherSmoke";
            
            // Create smoke collider component
            SmokeCollider smokeCol = new SmokeCollider
            {
                gameObject = colliderObj,
                collider = sphereCollider,
                rigidbody = rb,
                creationTime = 0f
            };
            
            colliderObj.SetActive(false);
            colliderPool.Enqueue(smokeCol);
        }
    }
    
    void UpdateSmokeColliders()
    {
        // Get current smoke particles
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[smokeParticles.main.maxParticles];
        int particleCount = smokeParticles.GetParticles(particles);
        
        // Create colliders for new particles (limit to available colliders)
        int collidersToCreate = Mathf.Min(particleCount, maxColliders - activeColliders.Count);
        
        for (int i = 0; i < collidersToCreate && colliderPool.Count > 0; i++)
        {
            if (i < particleCount)
            {
                SmokeCollider smokeCol = colliderPool.Dequeue();
                
                // Position collider at particle position
                Vector3 worldPos = smokeParticles.transform.TransformPoint(particles[i].position);
                smokeCol.gameObject.transform.position = worldPos;
                smokeCol.creationTime = Time.time;
                smokeCol.gameObject.SetActive(true);
                
                activeColliders.Add(smokeCol);
            }
        }
    }
    
    void CleanupExpiredColliders()
    {
        for (int i = activeColliders.Count - 1; i >= 0; i--)
        {
            SmokeCollider smokeCol = activeColliders[i];
            
            // Check if collider has expired
            if (Time.time - smokeCol.creationTime > colliderLifetime)
            {
                smokeCol.gameObject.SetActive(false);
                activeColliders.RemoveAt(i);
                colliderPool.Enqueue(smokeCol);
            }
        }
    }
    
    void OnDisable()
    {
        // Clean up all active colliders when disabled
        foreach (var collider in activeColliders)
        {
            if (collider.gameObject != null)
                collider.gameObject.SetActive(false);
        }
        activeColliders.Clear();
    }
    
    [System.Serializable]
    private class SmokeCollider
    {
        public GameObject gameObject;
        public SphereCollider collider;
        public Rigidbody rigidbody;
        public float creationTime;
    }
}