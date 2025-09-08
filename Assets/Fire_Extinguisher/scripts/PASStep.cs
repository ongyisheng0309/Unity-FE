using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PASStep : MonoBehaviour
{
    public PASEnum currentStep = PASEnum.None;
}

public enum PASEnum
{
    None,
    PullPin,
    AimNozzle,
    PressHandle,
    Spray
}