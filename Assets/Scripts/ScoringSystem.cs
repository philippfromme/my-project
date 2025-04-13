using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoringSystem : MonoBehaviour
{
    public GameObject car;
    public UIDocument uiDocument;

    private Label scoreLabel;

    private int totalScore = 0;

    public int minSpeed = 10; // Minimum speed to start scoring (10 km/h)
    public int minSlipAngle = 10; // Minimum slip angle to start scoring (10 degrees)

    public float scoreInterval = 0.2f; // Time interval to add score (200ms)
    private float nextScoreTime; // The time the score will next be increased

    // Start is called before the first frame update
    void Start()
    {
        scoreLabel = uiDocument.rootVisualElement.Q<Label>("Score");

        scoreLabel.text = "Score: 0";

        nextScoreTime = Time.time;
    }

    void Update()
    {
        if (Time.time < nextScoreTime)
        {
            return;
        }

        float speed = car.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        float slipAngle = car.GetComponent<CarController>().slipAngle;

        if (speed < minSpeed || slipAngle < minSlipAngle)
        {
            // If speed is less than 5 km/h or slip angle is less than 5 degrees, do not score
            nextScoreTime = Time.time + scoreInterval;

            return;
        }

        int score = Mathf.FloorToInt(speed / 10 + slipAngle / 5);

        if (score > 0)
        {
            totalScore += score;

            scoreLabel.text = $"SCORE {totalScore}";
        }

        nextScoreTime = Time.time + scoreInterval;
    }
}
