using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Main : MonoBehaviour
{
    private UIDocument uiDocument;

    private Label speedLabel;
    private Label slipAngleLabel;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        speedLabel = uiDocument.rootVisualElement.Q<Label>("Speed");
        slipAngleLabel = uiDocument.rootVisualElement.Q<Label>("SlipAngle");

        speedLabel.text = "SPEED 0 km/h";
        slipAngleLabel.text = "SLIP ANGLE 0°";
    }

    void Update()
    {
        
    }

    public void UpdateSpeed(int speed)
    {
        speedLabel.text = $"SPEED {speed} km/h";
    }

    public void UpdateSlipAngle(int slipAngle)
    {
        slipAngleLabel.text = $"SLIP ANGLE {slipAngle}°";
    }
}
