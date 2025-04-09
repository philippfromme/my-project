using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Main : MonoBehaviour
{
    private UIDocument uiDocument;

    private Label speedLabel;
    private Label slipAngleLabel;

    private VisualElement wheelSlipStatusFrontLeft;
    private VisualElement wheelSlipStatusFrontRight;
    private VisualElement wheelSlipStatusBackLeft;
    private VisualElement wheelSlipStatusBackRight;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        speedLabel = uiDocument.rootVisualElement.Q<Label>("Speed");
        slipAngleLabel = uiDocument.rootVisualElement.Q<Label>("SlipAngle");

        speedLabel.text = "Speed: 0 km/h";
        slipAngleLabel.text = "Slip Angle: 0°";

        wheelSlipStatusFrontLeft = uiDocument.rootVisualElement.Q<VisualElement>("FrontLeft");
        wheelSlipStatusFrontRight = uiDocument.rootVisualElement.Q<VisualElement>("FrontRight");
        wheelSlipStatusBackLeft = uiDocument.rootVisualElement.Q<VisualElement>("BackLeft");
        wheelSlipStatusBackRight = uiDocument.rootVisualElement.Q<VisualElement>("BackRight");
    }

    void Update()
    {
        
    }

    public void UpdateSpeed(int speed)
    {
        speedLabel.text = $"Speed: {speed} km/h";
    }

    public void UpdateSlipAngle(int slipAngle)
    {
        slipAngleLabel.text = $"Slip Angle: {slipAngle}°";
    }

    public void UpdateWheelSlipStatus(WheelSlipStatus wheelSlipStatus)
    {

        if (wheelSlipStatus.frontRightWheelSlipping) {
            wheelSlipStatusFrontRight.style.backgroundColor = Color.red;
        }
        else
        {
            wheelSlipStatusFrontRight.style.backgroundColor = Color.green;
        }

        if (wheelSlipStatus.frontLeftWheelSlipping) {
            wheelSlipStatusFrontLeft.style.backgroundColor = Color.red;
        }
        else
        {
            wheelSlipStatusFrontLeft.style.backgroundColor = Color.green;
        }

        if (wheelSlipStatus.backRightWheelSlipping) {
            wheelSlipStatusBackRight.style.backgroundColor = Color.red;
        }
        else
        {
            wheelSlipStatusBackRight.style.backgroundColor = Color.green;
        }

        if (wheelSlipStatus.backLeftWheelSlipping) {
            wheelSlipStatusBackLeft.style.backgroundColor = Color.red;
        }
        else
        {
            wheelSlipStatusBackLeft.style.backgroundColor = Color.green;
        }
    }
}
