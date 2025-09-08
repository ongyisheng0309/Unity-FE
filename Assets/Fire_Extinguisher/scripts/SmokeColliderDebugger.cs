using UnityEngine;

/// <summary>
/// Debug script to help identify smoke collider issues.
/// Attach to your extinguisher to see what's happening with smoke colliders.
/// </summary>
public class SmokeColliderDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugLogs = true;
    public bool visualizeColliders = false;
    public Color debugColor = Color.cyan;
    
    private ExtinguisherSmokeCollider smokeCollider;
    private ParticleSystem sprayEffect;
    
    void Start()
    {
        // Find components
        smokeCollider = GetComponentInChildren<ExtinguisherSmokeCollider>();
        sprayEffect = GetComponentInChildren<ParticleSystem>();
        
        if (showDebugLogs)
        {
            Debug.Log($"[SmokeDebugger] Found smoke collider: {smokeCollider != null}");
            Debug.Log($"[SmokeDebugger] Found spray effect: {sprayEffect != null}");
        }
    }
    
    void Update()
    {
        if (showDebugLogs && Time.frameCount % 60 == 0) // Log every ~1 second
        {
            if (sprayEffect != null)
            {
                Debug.Log($"[SmokeDebugger] Spray playing: {sprayEffect.isPlaying}, Particles: {sprayEffect.particleCount}");
            }
            
            // Count active smoke colliders
            var smokeObjects = GameObject.FindGameObjectsWithTag("ExtinguisherSmoke");
            int activeColliders = 0;
            foreach (var obj in smokeObjects)
            {
                if (obj.activeInHierarchy) activeColliders++;
            }
            
            Debug.Log($"[SmokeDebugger] Active smoke colliders: {activeColliders}");
        }
        
        // Visualize colliders (using Gizmos instead of Debug.DrawWireSphere)
        // Note: Visual debugging is handled in OnDrawGizmos method
    }
    
    void OnDrawGizmos()
    {
        if (visualizeColliders)
        {
            Gizmos.color = debugColor;
            var smokeObjects = GameObject.FindGameObjectsWithTag("ExtinguisherSmoke");
            foreach (var obj in smokeObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    var collider = obj.GetComponent<SphereCollider>();
                    if (collider != null)
                    {
                        Gizmos.DrawWireSphere(obj.transform.position, collider.radius);
                    }
                }
            }
        }
    }
}