using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRFireExtinguisherController : MonoBehaviour
{
    [Header("Fire Extinguisher Components")]
    public GameObject extinguisherLock;        // Extinguisher_Lock_LP (the pin)
    public GameObject extinguisherTrigger;     // Extinguisher_Trigger_LP
    public GameObject extinguisherHandle;      // Extinguisher_Handle_LP
    public GameObject hoseEnd;                 // End of Extinguisher_Hose_LP
    public ParticleSystem foamParticles;       // Particle system at hose tip

    [Header("XR Interaction")]
    private XRGrabInteractable grabInteractable;
    private XRBaseInteractor currentInteractor;
    public bool requireTwoHands = false;       // Optional two-handed operation

    [Header("Pin/Lock Interaction")]
    public XRGrabInteractable pinGrabInteractable;  // Add XR Grab to the pin too!
    public float pinPullDistance = 0.1f;
    public AudioClip pinRemoveSound;

    [Header("Trigger Settings")]
    public float triggerRotationAngle = 25f;
    public float triggerReturnSpeed = 5f;
    private Quaternion triggerOriginalRotation;

    [Header("Safety & State")]
    public bool isPinRemoved = false;
    public bool isTriggerPressed = false;
    public float sprayDuration = 30f;
    private float remainingSpray;

    [Header("Haptic Feedback")]
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    [Header("Audio")]
    public AudioSource sprayAudioSource;
    public AudioClip spraySound;
    private AudioSource audioSource;

    void Start()
    {
        // Get XR components
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Store original rotations
        if (extinguisherTrigger != null)
            triggerOriginalRotation = extinguisherTrigger.transform.localRotation;

        // Initialize spray
        remainingSpray = sprayDuration;
        if (foamParticles != null)
        {
            foamParticles.Stop();
            var emission = foamParticles.emission;
            emission.enabled = false;
        }

        // Setup XR events
        SetupXREvents();

        // Setup pin as grabbable if exists
        SetupPinInteraction();
    }

    void SetupXREvents()
    {
        // Subscribe to grab events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        grabInteractable.activated.AddListener(OnTriggerActivated);
        grabInteractable.deactivated.AddListener(OnTriggerDeactivated);
    }

    void SetupPinInteraction()
    {
        if (extinguisherLock != null)
        {
            // Make pin grabbable separately
            pinGrabInteractable = extinguisherLock.GetComponent<XRGrabInteractable>();
            if (pinGrabInteractable == null)
            {
                pinGrabInteractable = extinguisherLock.AddComponent<XRGrabInteractable>();

                // Configure pin grab settings
                pinGrabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
                pinGrabInteractable.trackPosition = true;
                pinGrabInteractable.trackRotation = false;
                pinGrabInteractable.throwOnDetach = true;

                // Add small collider if needed
                if (extinguisherLock.GetComponent<Collider>() == null)
                {
                    CapsuleCollider col = extinguisherLock.AddComponent<CapsuleCollider>();
                    col.radius = 0.01f;
                    col.height = 0.05f;
                }

                // Add rigidbody if needed
                if (extinguisherLock.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = extinguisherLock.AddComponent<Rigidbody>();
                    rb.mass = 0.1f;
                    rb.useGravity = false;
                }
            }

            // Subscribe to pin grab events
            pinGrabInteractable.selectEntered.AddListener(OnPinGrabbed);
            pinGrabInteractable.selectExited.AddListener(OnPinReleased);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject as XRBaseInteractor;
        Debug.Log("Fire Extinguisher grabbed!");

        // Show instructions
        ShowInstructions();

        // Haptic feedback
        SendHapticFeedback(0.3f, 0.1f);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        currentInteractor = null;
        Debug.Log("Fire Extinguisher released!");

        // Stop spraying if active
        if (isTriggerPressed)
        {
            ReleaseTrigger();
        }
    }

    void OnPinGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Safety pin grabbed - pull to remove!");
        StartCoroutine(RemovePinSequence(args.interactorObject as XRBaseInteractor));
    }

    void OnPinReleased(SelectExitEventArgs args)
    {
        if (!isPinRemoved)
        {
            Debug.Log("Pin released but not removed");
        }
    }

    IEnumerator RemovePinSequence(XRBaseInteractor interactor)
    {
        Vector3 startPos = extinguisherLock.transform.position;

        // Wait for pull motion
        float pullTimer = 0f;
        while (pinGrabInteractable.isSelected && !isPinRemoved)
        {
            float distance = Vector3.Distance(extinguisherLock.transform.position, startPos);

            if (distance > pinPullDistance)
            {
                // Pin successfully removed
                isPinRemoved = true;
                Debug.Log("Safety pin REMOVED! Ready to use!");

                // Play sound
                if (pinRemoveSound != null && audioSource != null)
                    audioSource.PlayOneShot(pinRemoveSound);

                // Haptic feedback
                if (interactor != null)
                    SendHapticToInteractor(interactor, 0.7f, 0.2f);

                // Disable pin after short delay
                yield return new WaitForSeconds(0.5f);
                extinguisherLock.SetActive(false);

                break;
            }

            pullTimer += Time.deltaTime;
            yield return null;
        }
    }

    void OnTriggerActivated(ActivateEventArgs args)
    {
        if (isPinRemoved && remainingSpray > 0)
        {
            PressTrigger();
        }
        else if (!isPinRemoved)
        {
            Debug.LogWarning("Remove safety pin first!");
            SendHapticFeedback(0.1f, 0.05f); // Weak feedback - blocked
        }
    }

    void OnTriggerDeactivated(DeactivateEventArgs args)
    {
        ReleaseTrigger();
    }

    void PressTrigger()
    {
        if (!isTriggerPressed && isPinRemoved)
        {
            isTriggerPressed = true;
            Debug.Log("Spraying foam!");

            // Start particles
            if (foamParticles != null)
            {
                var emission = foamParticles.emission;
                emission.enabled = true;
                foamParticles.Play();
            }

            // Play spray sound
            if (sprayAudioSource != null && spraySound != null)
            {
                sprayAudioSource.clip = spraySound;
                sprayAudioSource.loop = true;
                sprayAudioSource.Play();
            }

            // Continuous haptic while spraying
            InvokeRepeating(nameof(SprayHaptic), 0f, 0.1f);
        }

        // Animate trigger
        if (extinguisherTrigger != null)
        {
            Quaternion targetRotation = triggerOriginalRotation * Quaternion.Euler(-triggerRotationAngle, 0, 0);
            extinguisherTrigger.transform.localRotation = Quaternion.Lerp(
                extinguisherTrigger.transform.localRotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }
    }

    void ReleaseTrigger()
    {
        if (isTriggerPressed)
        {
            isTriggerPressed = false;
            Debug.Log("Stopped spraying");

            // Stop particles
            if (foamParticles != null)
            {
                var emission = foamParticles.emission;
                emission.enabled = false;
                foamParticles.Stop();
            }

            // Stop sound
            if (sprayAudioSource != null)
                sprayAudioSource.Stop();

            // Stop haptics
            CancelInvoke(nameof(SprayHaptic));
        }

        // Return trigger to original position
        if (extinguisherTrigger != null)
        {
            extinguisherTrigger.transform.localRotation = Quaternion.Lerp(
                extinguisherTrigger.transform.localRotation,
                triggerOriginalRotation,
                Time.deltaTime * triggerReturnSpeed
            );
        }
    }

    void Update()
    {
        // Update spray duration
        if (isTriggerPressed && remainingSpray > 0)
        {
            remainingSpray -= Time.deltaTime;
            if (remainingSpray <= 0)
            {
                remainingSpray = 0;
                ReleaseTrigger();
                Debug.Log("Extinguisher empty!");
            }
        }

        // Alternative input methods (for testing without VR)
        if (Application.isEditor)
        {
            // Pin removal with P key
            if (Input.GetKeyDown(KeyCode.P) && !isPinRemoved)
            {
                RemovePinManually();
            }

            // Trigger with Space
            if (Input.GetKeyDown(KeyCode.Space) && isPinRemoved)
            {
                PressTrigger();
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ReleaseTrigger();
            }
        }
    }

    void RemovePinManually()
    {
        isPinRemoved = true;
        if (extinguisherLock != null)
            extinguisherLock.SetActive(false);
        Debug.Log("Pin removed manually (testing mode)");
    }

    void SprayHaptic()
    {
        SendHapticFeedback(hapticIntensity * 0.5f, hapticDuration);
    }

    void SendHapticFeedback(float intensity, float duration)
    {
        if (currentInteractor != null)
        {
            SendHapticToInteractor(currentInteractor, intensity, duration);
        }
    }

    void SendHapticToInteractor(XRBaseInteractor interactor, float intensity, float duration)
    {
        if (interactor != null && interactor.TryGetComponent<XRBaseController>(out var controller))
        {
            controller.SendHapticImpulse(intensity, duration);
        }
    }

    void ShowInstructions()
    {
        Debug.Log("=== FIRE EXTINGUISHER INSTRUCTIONS ===");
        Debug.Log("1. Grab and remove the safety pin (grab the pin and pull)");
        Debug.Log("2. Point hose at base of fire");
        Debug.Log("3. Press trigger button to spray");
        Debug.Log("4. Sweep side to side");
        Debug.Log($"Remaining spray: {remainingSpray:F1} seconds");
    }

    public void RefillExtinguisher()
    {
        remainingSpray = sprayDuration;
        Debug.Log("Extinguisher refilled!");
    }

    public float GetRemainingSprayPercentage()
    {
        return (remainingSpray / sprayDuration) * 100f;
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
            grabInteractable.activated.RemoveListener(OnTriggerActivated);
            grabInteractable.deactivated.RemoveListener(OnTriggerDeactivated);
        }

        if (pinGrabInteractable != null)
        {
            pinGrabInteractable.selectEntered.RemoveListener(OnPinGrabbed);
            pinGrabInteractable.selectExited.RemoveListener(OnPinReleased);
        }
    }
}