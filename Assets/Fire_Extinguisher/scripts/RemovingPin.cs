using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RemovingPin : MonoBehaviour
{
    public InputActionReference LeftHand; // Assign XR RightHand/Activate
    public InputActionReference RightHand; // Assign XR RightHand/Activate
    public float moveSpeed = 1f;       // units per second
    public float maxDistance = 2f;     // maximum allowed movement along X
    public PASStep PAS;

    private float movedDistance = 0f;
    private bool isHolding = false;
    private XRBaseInteractable interactable;
    private Vector3 startLocalPos;
    

    private void Awake()
    {
        // get the interactable attached to this pin
        interactable = GetComponent<XRBaseInteractable>();
        PAS.currentStep = PASEnum.PullPin;
        startLocalPos = transform.localPosition; // localPosition ignores parent's scale
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
        if (isHolding && movedDistance < maxDistance)
        {
            float step = moveSpeed * Time.deltaTime;

            // Move along local X axis
            transform.localPosition += Vector3.right * step;

            // Compute local-space distance
            float movedDistance = Vector3.Distance(startLocalPos, transform.localPosition);

            if (movedDistance >= 1.5f)
            {
                PAS.currentStep = PASEnum.AimNozzle;
                Destroy(gameObject);
            }

            if (movedDistance >= maxDistance)
                transform.localPosition = startLocalPos + Vector3.right * maxDistance;
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
