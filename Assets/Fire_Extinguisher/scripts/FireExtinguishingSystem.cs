using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the interaction between fire extinguisher smoke and fire particles.
/// Handles smooth extinguishing effects when smoke particles contact fire.
/// </summary>
public class FireExtinguishingSystem : MonoBehaviour
{
    [Header("Fire Settings")]
    public ParticleSystem fireParticles;
    public AudioSource fireAudio;
    public float extinguishRadius = 2f; // Radius around smoke particles that can extinguish fire
    public float extinguishRate = 2f; // How fast the fire gets extinguished
    
    [Header("Extinguisher Settings")]
    public ParticleSystem smokeParticles; // The extinguisher spray effect
    public float smokeEffectiveness = 1f; // Multiplier for extinguishing effectiveness
    
    [Header("Visual Effects")]
    public ParticleSystem steamEffect; // Steam effect when fire is extinguished
    public AudioSource extinguishSound; // Sound when fire is put out
    
    private float currentFireIntensity = 1f; // Current fire strength (1 = full, 0 = extinguished)
    private bool isExtinguishing = false;
    private ParticleSystem.EmissionModule fireEmission;
    private ParticleSystem.MainModule fireMain;
    private float originalFireRate;
    private float originalFireLifetime;
    
    void Start()
    {
        // Store original fire particle settings
        if (fireParticles != null)
        {
            fireEmission = fireParticles.emission;
            fireMain = fireParticles.main;
            originalFireRate = fireEmission.rateOverTime.constant;
            originalFireLifetime = fireMain.startLifetime.constant;
        }
        
        // Ensure steam effect is initially stopped
        if (steamEffect != null)
            steamEffect.Stop();
    }
    
    void Update()
    {
        // Check if smoke particles are active and near fire
        if (smokeParticles != null && smokeParticles.isPlaying && fireParticles != null && fireParticles.isPlaying)
        {
            CheckForExtinguishing();
        }
        
        // Regenerate fire slowly if not being extinguished
        if (!isExtinguishing && currentFireIntensity < 1f)
        {
            RegenerateFire();
        }
    }
    
    void CheckForExtinguishing()
    {
        // Get smoke particle positions
        ParticleSystem.Particle[] smokeParticleArray = new ParticleSystem.Particle[smokeParticles.main.maxParticles];
        int smokeCount = smokeParticles.GetParticles(smokeParticleArray);
        
        bool smokeTouchingFire = false;
        
        // Check if any smoke particles are within extinguish radius of fire
        for (int i = 0; i < smokeCount; i++)
        {
            Vector3 smokeWorldPos = smokeParticles.transform.TransformPoint(smokeParticleArray[i].position);
            float distanceToFire = Vector3.Distance(smokeWorldPos, fireParticles.transform.position);
            
            if (distanceToFire <= extinguishRadius)
            {
                smokeTouchingFire = true;
                break;
            }
        }
        
        if (smokeTouchingFire)
        {
            ExtinguishFire();
        }
        else
        {
            isExtinguishing = false;
        }
    }
    
    void ExtinguishFire()
    {
        if (currentFireIntensity <= 0f) return;
        
        isExtinguishing = true;
        
        // Reduce fire intensity over time
        currentFireIntensity -= extinguishRate * smokeEffectiveness * Time.deltaTime;
        currentFireIntensity = Mathf.Clamp01(currentFireIntensity);
        
        // Update fire particle emission rate and size
        var emission = fireEmission;
        emission.rateOverTime = originalFireRate * currentFireIntensity;
        
        var main = fireMain;
        main.startSize = originalFireLifetime * currentFireIntensity;
        
        // Play steam effect if fire is being extinguished
        if (steamEffect != null && !steamEffect.isPlaying && currentFireIntensity > 0.1f)
        {
            steamEffect.transform.position = fireParticles.transform.position;
            steamEffect.Play();
        }
        
        // Play extinguish sound
        if (extinguishSound != null && !extinguishSound.isPlaying)
        {
            extinguishSound.Play();
        }
        
        // Stop fire completely if intensity is very low
        if (currentFireIntensity <= 0.1f)
        {
            fireParticles.Stop();
            if (fireAudio != null) fireAudio.Stop();
            
            // Stop steam effect after a delay
            if (steamEffect != null)
            {
                StartCoroutine(StopSteamAfterDelay(2f));
            }
        }
    }
    
    void RegenerateFire()
    {
        // Slowly regenerate fire if it's not completely out
        if (currentFireIntensity > 0.1f)
        {
            currentFireIntensity += 0.1f * Time.deltaTime;
            currentFireIntensity = Mathf.Clamp01(currentFireIntensity);
            
            // Update particle system
            var emission = fireEmission;
            emission.rateOverTime = originalFireRate * currentFireIntensity;
            
            var main = fireMain;
            main.startSize = originalFireLifetime * currentFireIntensity;
            
            // Restart fire if it was stopped
            if (!fireParticles.isPlaying && currentFireIntensity > 0.2f)
            {
                fireParticles.Play();
                if (fireAudio != null) fireAudio.Play();
            }
        }
    }
    
    IEnumerator StopSteamAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (steamEffect != null && steamEffect.isPlaying)
        {
            steamEffect.Stop();
        }
    }
    
    // Public method to restart fire (can be called by other scripts)
    public void RestartFire()
    {
        currentFireIntensity = 1f;
        if (fireParticles != null)
        {
            fireParticles.Play();
        }
        if (fireAudio != null)
        {
            fireAudio.Play();
        }
    }
    
    // Public method to check if fire is extinguished
    public bool IsFireExtinguished()
    {
        return currentFireIntensity <= 0.1f;
    }
}