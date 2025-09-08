using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Instruction : MonoBehaviour
{
    public PASStep PAS;

    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PASEnum currentStep = PAS.currentStep;
        switch(currentStep){
            case PASEnum.PullPin:
                text.text = "Step 1: Pull the Pin";
                break;
            case PASEnum.AimNozzle:
                text.text = "Step 2: Lift the nozzle to aim";
                break;
            case PASEnum.PressHandle:
                text.text = "Step 3: Press the trigger";
                break;
            case PASEnum.Spray:
                text.text = "Step 4: Now, pickup the extinguisher and spray left and right";
                break;
            default:
                text.text = "";
                break;
        }
    }
}
