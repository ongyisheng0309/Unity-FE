using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateHose : MonoBehaviour
{
    public InputActionReference LeftHand; // Assign XR RightHand/Activate
    public InputActionReference RightHand; // Assign XR RightHand/Activate
    public float rotationSpeed = 90f;  // degrees per second
    public float maxRotation = 90f;    // maximum allowed rotation
    public PASStep PAS;
    public bool isNozzel = false;
    public ParticleSystem SprayEffect;
    public AudioSource ExtinguisherSound; 

    private float currentRotation = 0f;
    private bool isHolding = false;
    private XRBaseInteractable interactable;

    private void Awake()
    {
        // get the interactable attached to this pin
        interactable = GetComponent<XRBaseInteractable>();
    }
    
    private void OnEnable()
    {
        LeftHand.action.started += OnPress;
        LeftHand.action.canceled += OnRelease;
        RightHand.action.started += OnPress;
        RightHand.action.canceled += OnRelease;
    }

    private void OnDisable()
    {
        LeftHand.action.started -= OnPress;
        LeftHand.action.canceled -= OnRelease;
        RightHand.action.started -= OnPress;
        RightHand.action.canceled -= OnRelease;
    }

    void Update()
    {
        if(PAS.currentStep != PASEnum.AimNozzle && isNozzel) return; //Make them pull the pin first
        if(PAS.currentStep != PASEnum.PressHandle && !isNozzel) return; //Make them pull the pin first
        if (isHolding && currentRotation < maxRotation)
        {
            float step = rotationSpeed * Time.deltaTime;

            // clamp so we never exceed max
            if (currentRotation + step > maxRotation)
                step = maxRotation - currentRotation;

            transform.Rotate(step, 0, 0);
            currentRotation += step;

            if(PAS.currentStep == PASEnum.PressHandle & currentRotation > 0){
                SprayEffect.Play();
                if(ExtinguisherSound){
                    ExtinguisherSound.Play();
                }
            }

            if(currentRotation > (maxRotation * 0.8)){
                if(PAS.currentStep == PASEnum.AimNozzle){
                    PAS.currentStep = PASEnum.PressHandle;
                }else{
                    PAS.currentStep = PASEnum.Spray;
                }
            }
        }
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
         if (interactable.isHovered || interactable.isSelected)
            isHolding = true;
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        isHolding = false;
    }
}
