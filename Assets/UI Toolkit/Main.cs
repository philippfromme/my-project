using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Main : MonoBehaviour
{
    private UIDocument uiDocument;

    private Label speedLabel;
    private Label slipAngleLabel;

    public 

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        speedLabel = uiDocument.rootVisualElement.Q<Label>("Speed");
        slipAngleLabel = uiDocument.rootVisualElement.Q<Label>("SlipAngle");

        speedLabel.text = "Speed: 0 km/h";
        slipAngleLabel.text = "Slip Angle: 0°";
    }

    void Update()
    {
        
    }

    public void UpdateSpeed(int speed)
    {
        speedLabel.text = $"Speed: {speed} km/h";
    }
}
