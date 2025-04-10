using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;

    public WheelColliders wheelColliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;
    public WheelSlipStatus wheelSlipStatus;

    public GameObject wheelParticlePrefab;

    public float gasInput;
    public float brakeInput;
    public float steeringInput;

    public float motorTorque = 1500f;
    public float brakeTorque = 3000f;

    public float maxSteeringAngle = 45f; // Maximum steering angle in degrees

    [Range(0, 1)]
    public float brakeDistribution = 0.7f; // 70% front, 30% rear

    [Range(0, 1)]
    public float steeringAssistFactor = 0.75f; // Steering assist factor

    public float slipAngle;

    public float speed;
    public float speedKmh; // Speed in km/h

    public float maxSpeed = 200f; // Maximum speed in km/h
    public float dragMultiplier = 0.5f; // Adjust drag near max speed

    public float drag;

    public float slipThreshold = 0.1f; // Adjust this value to control the slip threshold for particle effects

    public AnimationCurve steeringCurve;

    // Events
    [System.Serializable]
    public class SpeedEvent : UnityEvent<int> { }
    public SpeedEvent OnSpeedUpdated;

    [System.Serializable]
    public class SlipAngleEvent : UnityEvent<int> { }
    public SlipAngleEvent OnSlipAngleUpdated;

    [System.Serializable]
    public class WheelSlipStatusEvent : UnityEvent<WheelSlipStatus> { }
    public WheelSlipStatusEvent OnWheelSlipStatusUpdated;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // Adjust center of mass for better handling
        drag = rb.drag; // Set initial drag

        InitWheelParticles();
    }

    void InitWheelParticles() {
        wheelParticles.frontRightWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.frontRightWheelCollider.transform.position, Quaternion.identity, wheelColliders.frontRightWheelCollider.transform)
            .GetComponent<ParticleSystem>();

        wheelParticles.frontLeftWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.frontLeftWheelCollider.transform.position, Quaternion.identity, wheelColliders.frontLeftWheelCollider.transform)
            .GetComponent<ParticleSystem>();

        wheelParticles.backRightWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.backRightWheelCollider.transform.position, Quaternion.identity, wheelColliders.backRightWheelCollider.transform )
            .GetComponent<ParticleSystem>();

        wheelParticles.backLeftWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.backLeftWheelCollider.transform.position, Quaternion.identity, wheelColliders.backLeftWheelCollider.transform)
            .GetComponent<ParticleSystem>();
    }

    void CheckWheelSlip() {
        WheelHit[] wheelHits = new WheelHit[4];

        wheelColliders.frontRightWheelCollider.GetGroundHit(out wheelHits[0]);
        wheelColliders.frontLeftWheelCollider.GetGroundHit(out wheelHits[1]);
        wheelColliders.backRightWheelCollider.GetGroundHit(out wheelHits[2]);
        wheelColliders.backLeftWheelCollider.GetGroundHit(out wheelHits[3]);

        Debug.DrawRay(wheelHits[0].point, wheelHits[0].sidewaysSlip * Vector3.right, Color.red);
        Debug.DrawRay(wheelHits[0].point, wheelHits[0].forwardSlip * Vector3.forward, Color.blue);
        Debug.DrawRay(wheelHits[1].point, wheelHits[1].sidewaysSlip * Vector3.right, Color.red);
        Debug.DrawRay(wheelHits[1].point, wheelHits[1].forwardSlip * Vector3.forward, Color.blue);
        Debug.DrawRay(wheelHits[2].point, wheelHits[2].sidewaysSlip * Vector3.right, Color.red);
        Debug.DrawRay(wheelHits[2].point, wheelHits[2].forwardSlip * Vector3.forward, Color.blue);
        Debug.DrawRay(wheelHits[3].point, wheelHits[3].sidewaysSlip * Vector3.right, Color.red);
        Debug.DrawRay(wheelHits[3].point, wheelHits[3].forwardSlip * Vector3.forward, Color.blue);

        if (Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipThreshold) {
            wheelParticles.frontRightWheelParticles.Play();
            wheelSlipStatus.frontRightWheelSlipping = true;
        } else {
            wheelParticles.frontRightWheelParticles.Stop();
            wheelSlipStatus.frontRightWheelSlipping = false;
        }

        if (Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipThreshold) {
            wheelParticles.frontLeftWheelParticles.Play();
            wheelSlipStatus.frontLeftWheelSlipping = true;
        } else {
            wheelParticles.frontLeftWheelParticles.Stop();
            wheelSlipStatus.frontLeftWheelSlipping = false;
        }

        if (Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipThreshold) {
            wheelParticles.backRightWheelParticles.Play();
            wheelSlipStatus.backRightWheelSlipping = true;
        } else {
            wheelParticles.backRightWheelParticles.Stop();
            wheelSlipStatus.backRightWheelSlipping = false;
        }

        if (Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipThreshold) {
            wheelParticles.backLeftWheelParticles.Play();
            wheelSlipStatus.backLeftWheelSlipping = true;
        } else {
            wheelParticles.backLeftWheelParticles.Stop();
            wheelSlipStatus.backLeftWheelSlipping = false;
        }
    }

    void Update()
    {
        speedKmh = rb.velocity.magnitude * 3.6f; // Speed in km/h
        speed = rb.velocity.magnitude; // Speed in m/s

        CheckInput();
        ApplyMotorTorque();
        ApplySteering();
        ApplyBrake();
        CheckWheelSlip();
        UpdateWheels();

        int speedKmhInt = Mathf.FloorToInt(speedKmh);

        OnSpeedUpdated?.Invoke(speedKmhInt);

        int slipAngleInt = Mathf.FloorToInt(slipAngle);

        if (speed < 1f) {
            slipAngleInt = 0; // Reset slip angle when speed is low
        }

        OnSlipAngleUpdated?.Invoke(slipAngleInt); // Update slip angle event

        OnWheelSlipStatusUpdated?.Invoke(wheelSlipStatus); // Update wheel slip status event
    }

    void FixedUpdate() {
        if (rb.velocity.magnitude > maxSpeed * 0.9f) { // Apply drag when near max speed
            rb.drag = dragMultiplier;
        } else {
            rb.drag = drag; // Reset drag when under limit
        }
    }

    void ApplyMotorTorque() {
        float motorTorque = gasInput * this.motorTorque;

        wheelColliders.backLeftWheelCollider.motorTorque = motorTorque;
        wheelColliders.backRightWheelCollider.motorTorque = motorTorque;
    }

    void ApplyBrake() {
        wheelColliders.backLeftWheelCollider.brakeTorque = brakeInput * brakeTorque * (1f - brakeDistribution);
        wheelColliders.backRightWheelCollider.brakeTorque = brakeInput * brakeTorque * (1f - brakeDistribution);

        wheelColliders.frontLeftWheelCollider.brakeTorque = brakeInput * brakeTorque * brakeDistribution;
        wheelColliders.frontRightWheelCollider.brakeTorque = brakeInput * brakeTorque * brakeDistribution;
    }

    void ApplySteering() {
        float steeringAngle = steeringInput * steeringCurve.Evaluate(speed);

        if (speed > 1f) {
            steeringAngle += Vector3.SignedAngle(transform.forward, rb.velocity, transform.up) * steeringAssistFactor; // Adjust steering angle based on velocity direction
            steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle); // Limit steering angle
        }

        wheelColliders.frontLeftWheelCollider.steerAngle = steeringAngle;
        wheelColliders.frontRightWheelCollider.steerAngle = steeringAngle;
    }

    void CheckInput() {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        slipAngle = Vector3.Angle(transform.forward, rb.velocity - transform.forward);

        if (slipAngle < 120f && gasInput < 0) {
            brakeInput = Mathf.Abs(gasInput);
            gasInput = 0;
        } else {
            brakeInput = 0;
        }

        // apply handbrake
        if (Input.GetKey(KeyCode.Space)) {
            brakeInput = 1f;

            gasInput = 0f;
        } else {
            brakeInput = 0f;
        }
    }

    void UpdateWheels() {
        UpdateWheel(wheelColliders.frontRightWheelCollider, wheelMeshes.frontRightWheelMesh);
        UpdateWheel(wheelColliders.frontLeftWheelCollider, wheelMeshes.frontLeftWheelMesh);
        UpdateWheel(wheelColliders.backRightWheelCollider, wheelMeshes.backRightWheelMesh);
        UpdateWheel(wheelColliders.backLeftWheelCollider, wheelMeshes.backLeftWheelMesh);
    }

    void UpdateWheel(WheelCollider wheelCollider, MeshRenderer wheelMesh) {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = rotation;
    }
}

[System.Serializable]
public class WheelColliders {
    public WheelCollider frontRightWheelCollider;
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider backRightWheelCollider;
    public WheelCollider backLeftWheelCollider;
}

[System.Serializable]
public class WheelMeshes {
    public MeshRenderer frontRightWheelMesh;
    public MeshRenderer frontLeftWheelMesh;
    public MeshRenderer backRightWheelMesh;
    public MeshRenderer backLeftWheelMesh;
}

[System.Serializable]
public class WheelParticles {
    public ParticleSystem frontRightWheelParticles;
    public ParticleSystem frontLeftWheelParticles;
    public ParticleSystem backRightWheelParticles;
    public ParticleSystem backLeftWheelParticles;
}

[System.Serializable]
public class WheelSlipStatus {
    public bool frontRightWheelSlipping;
    public bool frontLeftWheelSlipping;
    public bool backRightWheelSlipping;
    public bool backLeftWheelSlipping;

    public override string ToString()
    {
        return $"Wheel Slip Status:\n" +
               $"- Front Right: {frontRightWheelSlipping}\n" +
               $"- Front Left: {frontLeftWheelSlipping}\n" +
               $"- Back Right: {backRightWheelSlipping}\n" +
               $"- Back Left: {backLeftWheelSlipping}";
    }
}

