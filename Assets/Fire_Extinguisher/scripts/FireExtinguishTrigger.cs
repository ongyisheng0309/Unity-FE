using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Combined fire extinguisher controller and fire extinguishing system.
/// Handles both extinguisher mechanics and fire detection/extinguishing.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnhancedFireExtinguisherController : MonoBehaviour
{
    [Header("Fire Extinguisher Components")]
    public GameObject extinguisherLock;        // Extinguisher_Lock_LP
    public GameObject extinguisherTrigger;     // Extinguisher_Trigger_LP
    public GameObject triggerAxis;             // Extinguisher_TriggerAxis_LP
    public GameObject hose;                    // Extinguisher_Hose_LP
    public ParticleSystem foamParticles;       // Particle system at hose tip

    [Header("Fire Detection Components")]
    public ParticleSystem fireParticles;       // Fire to extinguish
    public AudioSource fireAudio;              // Fire sound
    public GameObject fireGameObject;          // The entire fire GameObject to disable
    
    [Header("Extinguish Effects")]
    public ParticleSystem steamEffect;         // Steam effect when extinguished
    public AudioSource extinguishSound;        // Sound when fire is put out

    [Header("Animation Settings")]
    public float triggerRotationAngle = 25f;   // How much trigger rotates when squeezed
    public float triggerSpeed = 5f;            // Speed of trigger animation
    public float lockRemoveDistance = 0.1f;    // How far the lock moves when removed
    public float lockRemoveSpeed = 3f;         // Speed of lock removal

    [Header("Safety Settings")]
    public bool isPinRemoved = false;          // Safety pin status
    public bool isTriggerPressed = false;      // Trigger squeeze status
    public bool canUseExtinguisher = false;    // Overall usage permission

    [Header("Particle Settings")]
    public float sprayForce = 20f;             // Force of the spray
    public float sprayDuration = 10f;          // How long it can spray (seconds)
    private float remainingSpray;              // Remaining spray time

    [Header("Fire Extinguishing Settings")]
    public float extinguishTime = 2f;          // Time it takes to fully extinguish
    public bool canReignite = false;           // Whether fire can restart after being put out
    public float reigniteDelay = 10f;          // Time before fire can restart

    [Header("Audio (Optional)")]
    public AudioSource spraySound;             // Sound effect for spraying
    public AudioSource pinRemoveSound;         // Sound effect for pin removal

    // Private variables for extinguisher mechanics
    private Vector3 lockOriginalPosition;
    private Quaternion triggerOriginalRotation;
    private bool isRemoving = false;
    private XRGrabInteractable grabInteractable;

    // Private variables for fire extinguishing
    private bool isExtinguished = false;
    private bool isExtinguishing = false;
    private float extinguishProgress = 0f;
    private ParticleSystem.EmissionModule fireEmission;
    private float originalEmissionRate;
    private Collider triggerCollider;
    private Vector3 originalFireScale;         // Added to store original fire scale

    void Start()
    {
        // Get XR Grab Interactable component
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        // Get and initially disable the trigger collider
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        triggerCollider.enabled = false; // Disabled until pin is removed

        // Store original positions/rotations
        if (extinguisherLock != null)
            lockOriginalPosition = extinguisherLock.transform.localPosition;

        if (extinguisherTrigger != null)
            triggerOriginalRotation = extinguisherTrigger.transform.localRotation;

        // Initialize spray duration
        remainingSpray = sprayDuration;

        // Store original fire emission rate and scale
        if (fireParticles != null)
        {
            fireEmission = fireParticles.emission;
            originalEmissionRate = fireEmission.rateOverTime.constant;
        }

        if (fireGameObject != null)
        {
            originalFireScale = fireGameObject.transform.localScale; // Initialize original fire scale
            fireGameObject.SetActive(true);
        }

        // Ensure particle system is off at start
        if (foamParticles != null)
        {
            foamParticles.Stop();
            var emission = foamParticles.emission;
            emission.enabled = false;
        }

        // Ensure fire is initially active
        if (fireGameObject != null)
            fireGameObject.SetActive(true);
        
        if (steamEffect != null)
            steamEffect.Stop();

        // Set initial state
        canUseExtinguisher = false;

        // Add XR interaction events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void Update()
    {
        // Handle pin removal - Keyboard input for testing
        if (!isPinRemoved && !isRemoving && Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(RemoveSafetyPin());
        }

        // Handle trigger - Keyboard input for testing
        if (isPinRemoved && remainingSpray > 0)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                PressTrigger();
            }
            else
            {
                ReleaseTrigger();
            }
        }

        // Reset/Refill input - R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
            Refill();
            Debug.Log("Fire extinguisher reset and refilled via R key!");
        }

        // Update remaining spray indicator
        if (isTriggerPressed && remainingSpray > 0)
        {
            remainingSpray -= Time.deltaTime;
            if (remainingSpray <= 0)
            {
                remainingSpray = 0;
                ReleaseTrigger();
                Debug.Log("Fire extinguisher is empty!");
            }
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Fire extinguisher grabbed!");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        ReleaseTrigger(); // Make sure spray stops when released
        Debug.Log("Fire extinguisher released!");
    }

    IEnumerator RemoveSafetyPin()
    {
        isRemoving = true;
        Debug.Log("Removing safety pin...");

        // Play sound effect if available
        if (pinRemoveSound != null)
            pinRemoveSound.Play();

        // Animate pin removal
        float elapsedTime = 0;
        Vector3 targetPosition = lockOriginalPosition + Vector3.up * lockRemoveDistance;

        while (elapsedTime < 1f)
        {
            if (extinguisherLock != null)
            {
                extinguisherLock.transform.localPosition = Vector3.Lerp(
                    lockOriginalPosition,
                    targetPosition,
                    elapsedTime
                );
            }

            elapsedTime += Time.deltaTime * lockRemoveSpeed;
            yield return null;
        }

        // Disable the pin object
        if (extinguisherLock != null)
            extinguisherLock.SetActive(false);

        isPinRemoved = true;
        canUseExtinguisher = true;
        isRemoving = false;

        // Enable the trigger collider only after pin is removed
        if (triggerCollider != null)
            triggerCollider.enabled = true;

        Debug.Log("Safety pin removed! Fire extinguisher is ready to use and can now detect fires.");
    }

    void PressTrigger()
    {
        if (!isTriggerPressed)
        {
            isTriggerPressed = true;
            Debug.Log("Trigger pressed - Spraying foam!");

            // Start particle system
            if (foamParticles != null)
            {
                var emission = foamParticles.emission;
                emission.enabled = true;
                foamParticles.Play();
            }

            // Play spray sound
            if (spraySound != null && !spraySound.isPlaying)
                spraySound.Play();
        }

        // Animate trigger rotation
        if (extinguisherTrigger != null)
        {
            Quaternion targetRotation = triggerOriginalRotation * Quaternion.Euler(-triggerRotationAngle, 0, 0);
            extinguisherTrigger.transform.localRotation = Quaternion.Lerp(
                extinguisherTrigger.transform.localRotation,
                targetRotation,
                Time.deltaTime * triggerSpeed
            );
        }
    }

    void ReleaseTrigger()
    {
        if (isTriggerPressed)
        {
            isTriggerPressed = false;
            Debug.Log("Trigger released - Stopping spray.");

            // Stop particle system
            if (foamParticles != null)
            {
                var emission = foamParticles.emission;
                emission.enabled = false;
                foamParticles.Stop();
            }

            // Stop spray sound
            if (spraySound != null)
                spraySound.Stop();
        }

        // Return trigger to original position
        if (extinguisherTrigger != null)
        {
            extinguisherTrigger.transform.localRotation = Quaternion.Lerp(
                extinguisherTrigger.transform.localRotation,
                triggerOriginalRotation,
                Time.deltaTime * triggerSpeed
            );
        }
    }

    // Fire extinguishing trigger events
    void OnTriggerEnter(Collider other)
    {
        // Only detect fires if pin is removed and extinguisher is being used
        if (!isPinRemoved || !isTriggerPressed) return;

        // Check if the collider belongs to extinguisher foam particles
        if (other.CompareTag("ExtinguisherSmoke") || other.name.Contains("Spray") || other.name.Contains("Smoke"))
        {
            StartExtinguishing();
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Only continue extinguishing if pin is removed and trigger is pressed
        if (!isPinRemoved || !isTriggerPressed) return;

        // Continue extinguishing while smoke is present
        if (!isExtinguished && (other.CompareTag("ExtinguisherSmoke") || other.name.Contains("Spray") || other.name.Contains("Smoke")))
        {
            ContinueExtinguishing();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Stop extinguishing when smoke leaves
        if (other.CompareTag("ExtinguisherSmoke") || other.name.Contains("Spray") || other.name.Contains("Smoke"))
        {
            StopExtinguishing();
        }
    }

    void StartExtinguishing()
    {
        if (isExtinguished) return;
        
        isExtinguishing = true;
        Debug.Log("Started extinguishing fire!");
        
        // Play steam effect
        if (steamEffect != null && !steamEffect.isPlaying)
        {
            steamEffect.transform.position = fireParticles.transform.position;
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
        
        // Increase extinguish progress
        extinguishProgress += Time.deltaTime / extinguishTime;
        extinguishProgress = Mathf.Clamp01(extinguishProgress);
        
        // Reduce fire intensity
        if (fireParticles != null)
        {
            var emission = fireEmission;
            emission.rateOverTime = originalEmissionRate * (1f - extinguishProgress);
        }
        
        // Fully extinguish when progress reaches 100%
        if (extinguishProgress >= 1f)
        {
            ExtinguishFire();
        }
    }
    
    void StopExtinguishing()
    {
        isExtinguishing = false;
        
        // Stop steam effect
        if (steamEffect != null && steamEffect.isPlaying)
        {
            steamEffect.Stop();
        }
    }
    
    void ExtinguishFire()
    {
        isExtinguished = true;
        isExtinguishing = false;
        
        Debug.Log("Fire completely extinguished!");
        
        // Stop all fire effects
        if (fireParticles != null)
            fireParticles.Stop();
        
        if (fireAudio != null)
            fireAudio.Stop();
        
        // Stop steam effect immediately if we're about to deactivate
        if (steamEffect != null && steamEffect.isPlaying)
        {
            steamEffect.Stop();
        }
        
        // Schedule reignition BEFORE deactivating if enabled
        if (canReignite && gameObject.activeInHierarchy)
        {
            StartCoroutine(ReigniteAfterDelay());
        }
        
        // Deactivate fire GameObject last
        if (fireGameObject != null)
            fireGameObject.SetActive(false);
    }
    
    IEnumerator ReigniteAfterDelay()
    {
        yield return new WaitForSeconds(reigniteDelay);
        ReigniteFire();
    }
    
    public void ReigniteFire()
    {
        isExtinguished = false;
        isExtinguishing = false;
        extinguishProgress = 0f;
        
        Debug.Log("Fire reignited!");
        
        // Restart fire effects
        if (fireGameObject != null)
        {
            fireGameObject.SetActive(true);
            fireGameObject.transform.localScale = originalFireScale; // Restore original scale
        }
        
        if (fireParticles != null)
        {
            var emission = fireEmission;
            emission.rateOverTime = originalEmissionRate;
            fireParticles.Play();
        }
        
        if (fireAudio != null)
        {
            fireAudio.volume = 1f; // Restore original volume
            fireAudio.Play();
        }
    }

    // Public methods for status checking
    public bool CanUse()
    {
        return isPinRemoved && remainingSpray > 0;
    }

    public float GetRemainingSprayPercentage()
    {
        return (remainingSpray / sprayDuration) * 100f;
    }

    public bool IsExtinguished()
    {
        return isExtinguished;
    }

    // Utility methods
    public void Refill()
    {
        remainingSpray = sprayDuration;
        Debug.Log("Fire extinguisher refilled!");
    }

    public void Reset()
    {
        // Reset pin
        if (extinguisherLock != null)
        {
            extinguisherLock.SetActive(true);
            extinguisherLock.transform.localPosition = lockOriginalPosition;
        }

        // Reset trigger
        if (extinguisherTrigger != null)
            extinguisherTrigger.transform.localRotation = triggerOriginalRotation;

        // Reset particles
        if (foamParticles != null)
        {
            foamParticles.Stop();
            var emission = foamParticles.emission;
            emission.enabled = false;
        }

        // Reset trigger collider
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        // Reset states
        isPinRemoved = false;
        isTriggerPressed = false;
        canUseExtinguisher = false;
        remainingSpray = sprayDuration;
        
        // Reset fire extinguishing states
        isExtinguished = false;
        isExtinguishing = false;
        extinguishProgress = 0f;

        // Reset fire scale to original
        if (fireGameObject != null && originalFireScale != Vector3.zero)
        {
            fireGameObject.transform.localScale = originalFireScale;
        }

        Debug.Log("Fire extinguisher reset to initial state.");
    }
}