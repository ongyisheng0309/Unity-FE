using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ExtinguisherController : MonoBehaviour
{
    public PASStep PAS;
    public AudioSource ExtinguisherSound;
    public ParticleSystem SprayEffect;
    public Canvas instructionCanvas;
    private XRRayInteractor[] rayInteractors; // assign in Inspector
    public float canvasHeight = 0.5f;   // height above extinguisher
    public float canvasDistance = 5f; // offset forward
    public float baseScale = 0.5f;      // canvas scale relative to prefab
    public GameObject extinguisher; 

    [SerializeField] private XRGrabInteractable extinguisherGrab;
    private Transform playerCamera;
    
    [Header("Fire Extinguishing System")]
    public ExtinguisherSmokeCollider smokeCollider; // Reference to smoke collider system

    void Awake()
    {
        extinguisherGrab.enabled = false; // cannot grab until all steps done
        // Subscribe to events using new API
        extinguisherGrab.selectEntered.AddListener(OnGrab);
        extinguisherGrab.activated.AddListener(OnActivate);
        extinguisherGrab.deactivated.AddListener(OnDeactivate);

    }

    // Start is called before the first frame update
    void Start()
    {
        instructionCanvas.gameObject.SetActive(false); // hide initially
        playerCamera = Camera.main.transform;
        StartCoroutine(WaitForRayInteractors());
    }

    IEnumerator WaitForRayInteractors()
    {
        // Wait until at least one XRRayInteractor exists in the scene
        while (true)
        {
            rayInteractors = FindObjectsOfType<XRRayInteractor>();
            if (rayInteractors.Length > 0)
                break;
            yield return null; // wait a frame
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(PAS.currentStep == PASEnum.Spray){
            extinguisherGrab.enabled = true;
        }

        bool hitExtinguisher = false;

        foreach (var interactor in rayInteractors)
    {
        if (interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Transform t = hit.collider.transform;
            while (t != null)
            {
                if (t == this.transform) // check if the hit object is THIS prefab
                {
                    hitExtinguisher = true;
                    break;
                }
                t = t.parent;
            }
            if (hitExtinguisher)
                break;
        }
    }

    instructionCanvas.gameObject.SetActive(hitExtinguisher);

    if (hitExtinguisher)
    {
        instructionCanvas.gameObject.SetActive(true);

    // Forward direction from camera
    Vector3 forward = playerCamera.forward;

    // Slightly above the extinguisher position (instead of above camera)
    Vector3 targetPos = extinguisher.transform.position + Vector3.up * canvasHeight + forward * canvasDistance;

    instructionCanvas.transform.position = targetPos;

    // Face the player
    Vector3 directionToPlayer = playerCamera.position - instructionCanvas.transform.position;
    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

    // Fix inverted text
    lookRotation *= Quaternion.Euler(0f, 180f, 0f);

    instructionCanvas.transform.rotation = lookRotation;

    // Scale
    instructionCanvas.transform.localScale = Vector3.one * baseScale;
    }
    }

    void OnActivate(ActivateEventArgs args)
    {
        SprayEffect.Play();
        ExtinguisherSound.Play();
        
        // Enable smoke collision system when spraying
        if (smokeCollider != null)
        {
            smokeCollider.enabled = true;
        }
    }

    void OnDeactivate(DeactivateEventArgs args){        
        SprayEffect.Stop();
        ExtinguisherSound.Stop();
        
        // Keep smoke collision system enabled so other fires can still be extinguished
        // The system will automatically clean up its colliders when particles stop
        // if (smokeCollider != null)
        // {
        //     smokeCollider.enabled = false;
        // }
    }

    void OnGrab(SelectEnterEventArgs args){
        SprayEffect.Stop(); // If we grab it, only trigger sound and effect when activate
        ExtinguisherSound.Stop(); //Stop the effect also
    }
}
