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

    public Transform centerOfMassTransform; // Center of mass transform

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

    public float frontWheelRPM = 0f;
    public float rearWheelRPM = 0f;
    public float maxWheelRPM = 3000f; // Maximum wheel RPM

    public float maxSpeedKmh = 50f; // Maximum speed in km/h
    public float dragMultiplier = 0.5f; // Adjust drag near max speed

    public float drag;

    public float slipThreshold = 0.1f; // Adjust this value to control the slip threshold for particle effects

    public float antiRollFactor = 1f; // Adjust this value to control the anti-roll bar effect

    public AnimationCurve steeringCurve;

    [Header("Anti-Roll Bar")]
    public float antiRollForce = 5000f; // Adjust this value

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

        if (centerOfMassTransform != null) {
            rb.centerOfMass = centerOfMassTransform.localPosition; // Set center of mass to the transform's position
        } else {
            rb.ResetCenterOfMass(); // Reset to default center of mass
            Debug.LogWarning("Center of Mass Transform not assigned. Using default center of mass.");
        }

        drag = rb.drag; // Set initial drag

        InitWheelParticles();
    }

    void InitWheelParticles() {
        wheelParticles.frontRightWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.frontRightWheelCollider.transform.position, Quaternion.identity, wheelColliders.frontRightWheelCollider.transform)
            .GetComponent<ParticleSystem>();
        wheelParticles.frontRightWheelParticles.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Rotate the particle system to face the wheel

        wheelParticles.frontLeftWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.frontLeftWheelCollider.transform.position, Quaternion.identity, wheelColliders.frontLeftWheelCollider.transform)
            .GetComponent<ParticleSystem>();
        wheelParticles.frontLeftWheelParticles.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Rotate the particle system to face the wheel

        wheelParticles.backRightWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.backRightWheelCollider.transform.position, Quaternion.identity, wheelColliders.backRightWheelCollider.transform )
            .GetComponent<ParticleSystem>();
        wheelParticles.backRightWheelParticles.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Rotate the particle system to face the wheel

        wheelParticles.backLeftWheelParticles =
            Instantiate(wheelParticlePrefab, wheelColliders.backLeftWheelCollider.transform.position, Quaternion.identity, wheelColliders.backLeftWheelCollider.transform)
            .GetComponent<ParticleSystem>();
        wheelParticles.backLeftWheelParticles.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Rotate the particle system to face the wheel
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
            if (!wheelParticles.frontRightWheelParticles.isPlaying) {
                wheelParticles.frontRightWheelParticles.Play();
            }

            wheelSlipStatus.frontRightWheelSlipping = true;
        } else {
            if (wheelParticles.frontRightWheelParticles.isPlaying) {
                wheelParticles.frontRightWheelParticles.Stop();
            }

            wheelSlipStatus.frontRightWheelSlipping = false;
        }

        if (Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipThreshold) {
            if (!wheelParticles.frontLeftWheelParticles.isPlaying) {
                wheelParticles.frontLeftWheelParticles.Play();
            }

            wheelSlipStatus.frontLeftWheelSlipping = true;
        } else {
            if (wheelParticles.frontLeftWheelParticles.isPlaying) {
                wheelParticles.frontLeftWheelParticles.Stop();
            }

            wheelSlipStatus.frontLeftWheelSlipping = false;
        }

        if (Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipThreshold) {
            if (!wheelParticles.backRightWheelParticles.isPlaying) {
                wheelParticles.backRightWheelParticles.Play();
            }

            wheelSlipStatus.backRightWheelSlipping = true;
        } else {
            if (wheelParticles.backRightWheelParticles.isPlaying) {
                wheelParticles.backRightWheelParticles.Stop();
            }

            wheelSlipStatus.backRightWheelSlipping = false;
        }

        if (Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipThreshold) {
            if (!wheelParticles.backLeftWheelParticles.isPlaying) {
                wheelParticles.backLeftWheelParticles.Play();
            }

            wheelSlipStatus.backLeftWheelSlipping = true;
        } else {
            if (wheelParticles.backLeftWheelParticles.isPlaying) {
                wheelParticles.backLeftWheelParticles.Stop();
            }

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

        // Debug.Log($"GAS: {gasInput}, BRAKE: {brakeInput}, STEERING: {steeringInput}, SLIP ANGLE: {slipAngleInt}°");

        frontWheelRPM = (wheelColliders.frontLeftWheelCollider.rpm + wheelColliders.frontRightWheelCollider.rpm) / 2f;        
        rearWheelRPM = (wheelColliders.backLeftWheelCollider.rpm + wheelColliders.backRightWheelCollider.rpm) / 2f;

        bool rearWheelsSlipping = wheelSlipStatus.backLeftWheelSlipping || wheelSlipStatus.backRightWheelSlipping;

        Debug.Log($"Front Wheel RPM: {frontWheelRPM}, Rear Wheel RPM: {rearWheelRPM}, Rear Wheels Slipping: {rearWheelsSlipping}");
    }

    void FixedUpdate() {
        if (rb.velocity.magnitude * 3.6f > maxSpeedKmh) { // Apply drag when over max speed
            rb.drag = dragMultiplier;
        } else {
            rb.drag = drag; // Reset drag when under limit
        }

        // Debug.Log($"Speed: {speedKmh} km/h, Drag: {rb.drag}");

        ApplyAntiRoll();
    }

    void ApplyAntiRoll()
    {
        WheelHit hit;

        bool frontLeftGrounded = wheelColliders.frontLeftWheelCollider.GetGroundHit(out hit);
        bool frontRightGrounded = wheelColliders.frontRightWheelCollider.GetGroundHit(out hit);
        bool backLeftGrounded = wheelColliders.backLeftWheelCollider.GetGroundHit(out hit);
        bool backRightGrounded = wheelColliders.backRightWheelCollider.GetGroundHit(out hit);

        if (!rb)
        {
            return; // Ensure rb is not null
        }

        // if any of the wheels are not grounded, move the center of mass to the ground
        if (!frontLeftGrounded || !frontRightGrounded || !backLeftGrounded || !backRightGrounded)
        {
            Debug.Log("ANTI-ROLL BAR ACTIVE");

            rb.centerOfMass = centerOfMassTransform.localPosition + new Vector3(0, -antiRollFactor, 0); // Move center of mass downwards
        } else
        {
            rb.centerOfMass = centerOfMassTransform.localPosition; // Reset to default center of mass
        }
    }

    void ApplyMotorTorque() {
        float appliedMotorTorque = gasInput * motorTorque;

        wheelColliders.backLeftWheelCollider.motorTorque = appliedMotorTorque;
        wheelColliders.backRightWheelCollider.motorTorque = appliedMotorTorque;
    }

    void ApplyBrake() {
        wheelColliders.backLeftWheelCollider.brakeTorque = brakeInput * brakeTorque * (1f - brakeDistribution);
        wheelColliders.backRightWheelCollider.brakeTorque = brakeInput * brakeTorque * (1f - brakeDistribution);

        wheelColliders.frontLeftWheelCollider.brakeTorque = brakeInput * brakeTorque * brakeDistribution;
        wheelColliders.frontRightWheelCollider.brakeTorque = brakeInput * brakeTorque * brakeDistribution;
    }

    void ApplySteering() {
        float steerAngle = steeringInput * steeringCurve.Evaluate(speed);

        bool isMovingForward = Vector3.Dot(rb.velocity, transform.forward) > 0;

        // Assist steering based on speed and slip angle
        if (isMovingForward && speedKmh > 10f) {
            Debug.Log("Steering Assist Active");

            steerAngle += Vector3.SignedAngle(transform.forward, rb.velocity, transform.up) * steeringAssistFactor; // Adjust steering angle based on velocity direction
            steerAngle = Mathf.Clamp(steerAngle, -maxSteeringAngle, maxSteeringAngle); // Limit steering angle
        } else {
            Debug.Log("Steering Assist Inactive");
        }

        wheelColliders.frontLeftWheelCollider.steerAngle = steerAngle;
        wheelColliders.frontRightWheelCollider.steerAngle = steerAngle;
    }

    void CheckInput() {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");

        bool isMovingForward = Vector3.Dot(rb.velocity, transform.forward) > 0;

        // Handbrake logic
        if (Input.GetKey(KeyCode.Space))
        {
            brakeInput = 1f;

            gasInput = 0f;
        }
        else if (gasInput < 0 && isMovingForward) // Reverse input while moving forward
        {
            brakeInput = Mathf.Abs(gasInput); // Apply braking proportional to reverse input
        }
        else if (gasInput > 0 && !isMovingForward) // Forward input while moving backward
        {
            brakeInput = Mathf.Abs(gasInput); // Apply braking proportional to forward input
        }
        else
        {
            brakeInput = 0f; // No braking needed if inputs match movement direction
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

