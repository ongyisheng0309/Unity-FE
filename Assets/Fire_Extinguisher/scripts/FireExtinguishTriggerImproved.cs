using System.Collections;
using UnityEngine;

/// <summary>
/// Improved fire extinguishing system that avoids GameObject deactivation issues.
/// The trigger remains active while only the visual/audio effects are controlled.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FireExtinguishTriggerImproved : MonoBehaviour
{
    [Header("Fire Components")]
    public ParticleSystem fireParticles;
    public AudioSource fireAudio;
    public Light fireLight; // Optional fire light
    
    [Header("Extinguish Effects")]
    public ParticleSystem steamEffect; // Optional steam effect when extinguished
    public AudioSource extinguishSound; // Sound when fire is put out
    
    [Header("Settings")]
    public float extinguishTime = 2f; // Time it takes to fully extinguish
    public bool canReignite = true; // Whether fire can restart after being put out
    public float reigniteDelay = 3f; // Time before fire can restart
    
    [Header("Visual Settings")]
    public bool fadeOutSmooth = true; // Smooth fade vs instant off
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private bool isExtinguished = false;
    private bool isExtinguishing = false;
    private float extinguishProgress = 0f;
    private ParticleSystem.EmissionModule fireEmission;
    private ParticleSystem.MainModule fireMain;
    private float originalEmissionRate;
    private float originalStartSize;
    private float originalLightIntensity;
    
    // Smoke collision tracking
    private int smokeCollidersInContact = 0;
    private float lastExtinguishTime = 0f;
    
    // Coroutine references to prevent multiple coroutines
    private Coroutine reigniteCoroutine;
    private Coroutine steamCoroutine;
    
    void Start()
    {
        // Ensure collider is set as trigger
        GetComponent<Collider>().isTrigger = true;
        
        // Store original settings
        if (fireParticles != null)
        {
            fireEmission = fireParticles.emission;
            fireMain = fireParticles.main;
            originalEmissionRate = fireEmission.rateOverTime.constant;
            originalStartSize = fireMain.startSize.constant;
        }
        
        if (fireLight != null)
        {
            originalLightIntensity = fireLight.intensity;
        }
        
        // Ensure fire is initially active
        SetFireActive(true);
        
        if (steamEffect != null)
            steamEffect.Stop();
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to extinguisher smoke particles
        if (IsExtinguisherSmoke(other) && !isExtinguished)
        {
            smokeCollidersInContact++;
            
            // Start extinguishing only if not already extinguishing
            if (!isExtinguishing)
            {
                StartExtinguishing();
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // Continue extinguishing while smoke is present
        if (!isExtinguished && isExtinguishing && IsExtinguisherSmoke(other))
        {
            // Only continue if we have smoke colliders in contact
            if (smokeCollidersInContact > 0)
            {
                ContinueExtinguishing();
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Decrease smoke collider count when one leaves
        if (IsExtinguisherSmoke(other))
        {
            smokeCollidersInContact = Mathf.Max(0, smokeCollidersInContact - 1);
            
            // Stop extinguishing only if no more smoke colliders are in contact
            if (smokeCollidersInContact <= 0)
            {
                StopExtinguishing();
            }
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
        
        isExtinguishing = true;
        
        // Play steam effect
        if (steamEffect != null && !steamEffect.isPlaying)
        {
            steamEffect.transform.position = fireParticles != null ? fireParticles.transform.position : transform.position;
            steamEffect.Play();
        }
        
        // Play extinguish sound
        if (extinguishSound != null && !extinguishSound.isPlaying)
        {
            extinguishSound.Play();
        }
    }
    
    void ContinueExtinguishing()
    {
        if (isExtinguished || !isExtinguishing) return;
        
        // Only update progress once per frame to prevent multiple updates
        if (Time.time - lastExtinguishTime < Time.deltaTime) return;
        lastExtinguishTime = Time.time;
        
        // Increase extinguish progress based on time and smoke intensity
        float progressRate = Time.deltaTime / extinguishTime;
        
        // Scale progress by number of smoke colliders (more smoke = faster extinguishing)
        float smokeMultiplier = Mathf.Clamp(smokeCollidersInContact / 5f, 0.5f, 2f);
        extinguishProgress += progressRate * smokeMultiplier;
        extinguishProgress = Mathf.Clamp01(extinguishProgress);
        
        // Apply fade effect
        if (fadeOutSmooth)
        {
            float fadeValue = fadeCurve.Evaluate(1f - extinguishProgress);
            ApplyFadeEffect(fadeValue);
        }
        else
        {
            // Linear reduction
            ApplyFadeEffect(1f - extinguishProgress);
        }
        
        // Debug log to check progress
        Debug.Log($"Fire extinguish progress: {extinguishProgress:F2} | Smoke colliders: {smokeCollidersInContact}");
        
        // Fully extinguish when progress reaches 100%
        if (extinguishProgress >= 1f)
        {
            ExtinguishFire();
        }
    }
    
    void ApplyFadeEffect(float intensity)
    {
        // Update fire particles
        if (fireParticles != null)
        {
            var emission = fireEmission;
            emission.rateOverTime = originalEmissionRate * intensity;
            
            var main = fireMain;
            main.startSize = originalStartSize * intensity;
        }
        
        // Update fire light
        if (fireLight != null)
        {
            fireLight.intensity = originalLightIntensity * intensity;
        }
        
        // Adjust audio volume
        if (fireAudio != null)
        {
            fireAudio.volume = intensity;
        }
    }
    
    void StopExtinguishing()
    {
        isExtinguishing = false;
        
        Debug.Log("Stopped extinguishing - no more smoke in contact");
        
        // Stop steam effect after a delay
        if (steamEffect != null && steamEffect.isPlaying)
        {
            if (steamCoroutine != null) StopCoroutine(steamCoroutine);
            steamCoroutine = StartCoroutine(StopSteamAfterDelay(2f));
        }
        
        // Optionally allow fire to regenerate slowly when not being extinguished
        // (You can enable this if you want fire to recover when extinguishing stops)
        // StartCoroutine(RegenerateFireSlowly());
    }
    
    void ExtinguishFire()
    {
        isExtinguished = true;
        isExtinguishing = false;
        
        // Set fire to completely inactive
        SetFireActive(false);
        
        // Schedule reignition if enabled
        if (canReignite)
        {
            if (reigniteCoroutine != null) StopCoroutine(reigniteCoroutine);
            reigniteCoroutine = StartCoroutine(ReigniteAfterDelay());
        }
    }
    
    void SetFireActive(bool active)
    {
        if (fireParticles != null)
        {
            if (active)
            {
                if (!fireParticles.isPlaying) fireParticles.Play();
                ApplyFadeEffect(1f);
            }
            else
            {
                fireParticles.Stop();
            }
        }
        
        if (fireAudio != null)
        {
            if (active)
            {
                if (!fireAudio.isPlaying) fireAudio.Play();
                fireAudio.volume = 1f;
            }
            else
            {
                fireAudio.Stop();
            }
        }
        
        if (fireLight != null)
        {
            fireLight.intensity = active ? originalLightIntensity : 0f;
        }
    }
    
    IEnumerator StopSteamAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (steamEffect != null && steamEffect.isPlaying)
        {
            steamEffect.Stop();
        }
        steamCoroutine = null;
    }
    
    IEnumerator ReigniteAfterDelay()
    {
        yield return new WaitForSeconds(reigniteDelay);
        ReigniteFire();
        reigniteCoroutine = null;
    }
    
    public void ReigniteFire()
    {
        isExtinguished = false;
        isExtinguishing = false;
        extinguishProgress = 0f;
        
        // Restart fire effects
        SetFireActive(true);
    }
    
    // Public methods for external control
    public bool IsExtinguished() => isExtinguished;
    public float GetExtinguishProgress() => extinguishProgress;
    public void ForceExtinguish() => ExtinguishFire();
    public void ForceReignite() => ReigniteFire();
    
    void OnDestroy()
    {
        // Clean up coroutines
        if (reigniteCoroutine != null) StopCoroutine(reigniteCoroutine);
        if (steamCoroutine != null) StopCoroutine(steamCoroutine);
    }
}