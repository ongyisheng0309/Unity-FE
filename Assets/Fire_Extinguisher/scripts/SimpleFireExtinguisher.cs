using System.Collections;
using UnityEngine;

/// <summary>
/// Simple fire extinguisher that works with timing properly.
/// Use this if you're having issues with the complex version.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SimpleFireExtinguisher : MonoBehaviour
{
    [Header("Fire Components")]
    public ParticleSystem fireParticles;
    public AudioSource fireAudio;
    public Light fireLight;
    
    [Header("Settings")]
    public float extinguishTime = 3f; // Time it takes to fully extinguish
    public bool canReignite = true; // Whether fire can restart automatically
    public float reigniteDelay = 3f; // Time before fire restarts
    public bool showDebugLogs = true;
    
    private bool isBeingExtinguished = false;
    private bool isExtinguished = false;
    private float extinguishProgress = 0f;
    private float originalEmissionRate;
    private float originalLightIntensity;
    private Coroutine extinguishCoroutine;
    
    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        
        // Store original values
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            originalEmissionRate = emission.rateOverTime.constant;
        }
        
        if (fireLight != null)
        {
            originalLightIntensity = fireLight.intensity;
        }
        
        if (showDebugLogs) Debug.Log($"Fire initialized with {extinguishTime}s extinguish time");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (IsExtinguisherSmoke(other))
        {
            if (showDebugLogs) Debug.Log($"[{gameObject.name}] Smoke detected: {other.name}");
            
            if (!isExtinguished && !isBeingExtinguished)
            {
                if (showDebugLogs) Debug.Log($"[{gameObject.name}] Starting extinguish process");
                StartExtinguishing();
            }
            else
            {
                if (showDebugLogs) Debug.Log($"[{gameObject.name}] Fire already extinguished or being extinguished");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (IsExtinguisherSmoke(other) && isBeingExtinguished)
        {
            if (showDebugLogs) Debug.Log("Smoke left - stopping extinguish process");
            StopExtinguishing();
        }
    }
    
    bool IsExtinguisherSmoke(Collider other)
    {
        return other.CompareTag("ExtinguisherSmoke") || 
               other.name.Contains("Spray") || 
               other.name.Contains("Smoke") ||
               other.name.Contains("SmokeCollider");
    }
    
    void StartExtinguishing()
    {
        if (isExtinguished) return;
        
        isBeingExtinguished = true;
        
        if (extinguishCoroutine != null)
        {
            StopCoroutine(extinguishCoroutine);
        }
        
        extinguishCoroutine = StartCoroutine(ExtinguishOverTime());
    }
    
    void StopExtinguishing()
    {
        isBeingExtinguished = false;
        
        if (extinguishCoroutine != null)
        {
            StopCoroutine(extinguishCoroutine);
            extinguishCoroutine = null;
        }
        
        if (showDebugLogs) Debug.Log($"Extinguishing stopped at {extinguishProgress:F2} progress");
    }
    
    IEnumerator ExtinguishOverTime()
    {
        if (showDebugLogs) Debug.Log("Starting extinguish coroutine");
        
        float elapsed = 0f;
        
        while (elapsed < extinguishTime && isBeingExtinguished && !isExtinguished)
        {
            elapsed += Time.deltaTime;
            extinguishProgress = elapsed / extinguishTime;
            
            // Update fire intensity
            float intensity = 1f - extinguishProgress;
            UpdateFireIntensity(intensity);
            
            if (showDebugLogs && Time.frameCount % 30 == 0) // Log every ~30 frames
            {
                Debug.Log($"Extinguish progress: {extinguishProgress:F2} ({elapsed:F1}s / {extinguishTime:F1}s)");
            }
            
            yield return null;
        }
        
        // If we completed the full time, extinguish the fire
        if (elapsed >= extinguishTime && isBeingExtinguished)
        {
            ExtinguishCompletely();
        }
        
        extinguishCoroutine = null;
    }
    
    void UpdateFireIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        
        // Update particle emission
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = originalEmissionRate * intensity;
        }
        
        // Update light intensity
        if (fireLight != null)
        {
            fireLight.intensity = originalLightIntensity * intensity;
        }
        
        // Update audio volume
        if (fireAudio != null)
        {
            fireAudio.volume = intensity;
        }
    }
    
    void ExtinguishCompletely()
    {
        if (showDebugLogs) Debug.Log("Fire completely extinguished!");
        
        isExtinguished = true;
        isBeingExtinguished = false;
        extinguishProgress = 1f;
        
        // Stop all fire effects
        if (fireParticles != null) fireParticles.Stop();
        if (fireAudio != null) fireAudio.Stop();
        if (fireLight != null) fireLight.intensity = 0f;
        
        // Restart fire after delay if enabled
        if (canReignite)
        {
            if (showDebugLogs) Debug.Log($"Fire will reignite in {reigniteDelay} seconds");
            StartCoroutine(RestartFireAfterDelay(reigniteDelay));
        }
        else
        {
            if (showDebugLogs) Debug.Log("Fire permanently extinguished (reignite disabled)");
        }
    }
    
    IEnumerator RestartFireAfterDelay(float delay)
    {
        if (showDebugLogs) Debug.Log($"Waiting {delay} seconds to reignite...");
        
        yield return new WaitForSeconds(delay);
        
        if (showDebugLogs) Debug.Log("Reigniting fire now!");
        RestartFire();
    }
    
    public void RestartFire()
    {
        if (showDebugLogs) Debug.Log("Restarting fire");
        
        isExtinguished = false;
        isBeingExtinguished = false;
        extinguishProgress = 0f;
        
        // Restart fire effects
        UpdateFireIntensity(1f);
        if (fireParticles != null) fireParticles.Play();
        if (fireAudio != null) fireAudio.Play();
    }
    
    // Public methods
    public bool IsExtinguished() => isExtinguished;
    public float GetProgress() => extinguishProgress;
}