using UnityEngine;

/// <summary>
/// Simple debug script to monitor smoke colliders without visual rendering issues.
/// Attach to your extinguisher to see what's happening with smoke colliders.
/// </summary>
public class SimpleSmokeDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugLogs = true;
    public float logInterval = 1f; // How often to log debug info
    
    private float lastLogTime = 0f;
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
        if (showDebugLogs && Time.time - lastLogTime >= logInterval)
        {
            lastLogTime = Time.time;
            LogDebugInfo();
        }
    }
    
    void LogDebugInfo()
    {
        // Log spray effect status
        if (sprayEffect != null)
        {
            Debug.Log($"[SmokeDebugger] Spray playing: {sprayEffect.isPlaying}, Particles: {sprayEffect.particleCount}");
        }
        
        // Count active smoke colliders
        var smokeObjects = GameObject.FindGameObjectsWithTag("ExtinguisherSmoke");
        int activeColliders = 0;
        int totalColliders = smokeObjects.Length;
        
        foreach (var obj in smokeObjects)
        {
            if (obj != null && obj.activeInHierarchy) 
            {
                activeColliders++;
            }
        }
        
        Debug.Log($"[SmokeDebugger] Smoke colliders - Active: {activeColliders}, Total: {totalColliders}");
        
        // Log smoke collider component status
        if (smokeCollider != null)
        {
            Debug.Log($"[SmokeDebugger] Smoke collider component enabled: {smokeCollider.enabled}");
        }
    }
    
    // Public method to manually trigger debug info
    public void LogNow()
    {
        LogDebugInfo();
    }
    
    // Method to check if smoke colliders are working
    public bool AreSmokeCollidersWorking()
    {
        if (sprayEffect == null || smokeCollider == null) return false;
        
        if (!sprayEffect.isPlaying) return true; // Not spraying is fine
        
        var smokeObjects = GameObject.FindGameObjectsWithTag("ExtinguisherSmoke");
        foreach (var obj in smokeObjects)
        {
            if (obj != null && obj.activeInHierarchy) 
            {
                return true; // At least one collider is active
            }
        }
        
        return false; // Spraying but no active colliders = problem
    }
}