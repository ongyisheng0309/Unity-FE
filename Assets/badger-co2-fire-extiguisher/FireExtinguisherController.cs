using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireExtinguisherController : MonoBehaviour
{
    [Header("Fire Extinguisher Components")]
    public GameObject extinguisherLock;        // Extinguisher_Lock_LP
    public GameObject extinguisherTrigger;     // Extinguisher_Trigger_LP
    public GameObject triggerAxis;             // Extinguisher_TriggerAxis_LP
    public GameObject hose;                    // Extinguisher_Hose_LP
    public ParticleSystem foamParticles;       // Particle system at hose tip

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

    [Header("Audio (Optional)")]
    public AudioSource spraySound;             // Sound effect for spraying
    public AudioSource pinRemoveSound;         // Sound effect for pin removal

    private Vector3 lockOriginalPosition;
    private Quaternion triggerOriginalRotation;
    private bool isRemoving = false;

    void Start()
    {
        // Store original positions/rotations
        if (extinguisherLock != null)
            lockOriginalPosition = extinguisherLock.transform.localPosition;

        if (extinguisherTrigger != null)
            triggerOriginalRotation = extinguisherTrigger.transform.localRotation;

        // Initialize spray duration
        remainingSpray = sprayDuration;

        // Ensure particle system is off at start
        if (foamParticles != null)
        {
            foamParticles.Stop();
            var emission = foamParticles.emission;
            emission.enabled = false;
        }

        // Set initial state
        canUseExtinguisher = false;
    }

    void Update()
    {
        // Handle pin removal (Right mouse button or 'P' key)
        if (!isPinRemoved && !isRemoving && (Input.GetKeyDown(KeyCode.P) || Input.GetMouseButtonDown(1)))
        {
            StartCoroutine(RemoveSafetyPin());
        }

        // Handle trigger squeeze (Left mouse button or Space key)
        if (isPinRemoved && remainingSpray > 0)
        {
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                PressTrigger();
            }
            else
            {
                ReleaseTrigger();
            }
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

        Debug.Log("Safety pin removed! Fire extinguisher is ready to use.");
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

    // Public method to check if extinguisher can be used
    public bool CanUse()
    {
        return isPinRemoved && remainingSpray > 0;
    }

    // Public method to get remaining spray percentage
    public float GetRemainingSprayPercentage()
    {
        return (remainingSpray / sprayDuration) * 100f;
    }

    // Method to refill extinguisher (for testing or gameplay)
    public void Refill()
    {
        remainingSpray = sprayDuration;
        Debug.Log("Fire extinguisher refilled!");
    }

    // Method to reset extinguisher to initial state
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

        // Reset states
        isPinRemoved = false;
        isTriggerPressed = false;
        canUseExtinguisher = false;
        remainingSpray = sprayDuration;

        Debug.Log("Fire extinguisher reset to initial state.");
    }
}