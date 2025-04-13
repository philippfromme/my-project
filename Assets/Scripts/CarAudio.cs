using UnityEngine;

public class CarAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource baseEngineSound;  // Primary looping sound
    public AudioSource windSound;        // Optional: High-frequency wind sound
    public AudioSource motorLoadSound;   // Optional: Sound for acceleration/deceleration

    [Header("Sound Parameters")]
    public float minPitch = 0.5f;
    public float maxPitch = 2.0f;
    public float maxVolume = 0.8f;
    public float windVolumeMultiplier = 0.3f;
    public float loadVolumeMultiplier = 0.5f;

    [Header("References")]
    public CarController carController; // Reference to your CarController script
    private Rigidbody rb;

    void Start()
    {
        if (carController == null)
        {
            Debug.LogError("CarController not assigned!");
            return;
        }

        rb = carController.GetComponent<Rigidbody>();

        // Initialize looping sounds
        if (baseEngineSound != null)
        {
            baseEngineSound.loop = true;
            baseEngineSound.Play();
        }

        if (windSound != null)
        {
            windSound.loop = true;
            windSound.Play();
        }
    }

    void Update()
    {
        UpdateEngineSound();
        UpdateWindSound();
        UpdateMotorLoadSound();
    }

    void UpdateEngineSound()
    {
        if (baseEngineSound == null || rb == null) return;

        // Calculate normalized speed (0 to 1)
        float normalizedSpeed = Mathf.Clamp01(carController.rearWheelRPM / carController.maxWheelRPM);

        // Adjust pitch based on speed
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        baseEngineSound.pitch = targetPitch;

        // Adjust volume based on speed
        float targetVolume = normalizedSpeed * maxVolume;
        baseEngineSound.volume = targetVolume;
    }

    void UpdateWindSound()
    {
        if (windSound == null || rb == null) return;

        float normalizedSpeed = Mathf.Clamp01(rb.velocity.magnitude / carController.maxSpeedKmh);

        // Adjust volume of wind sound
        windSound.volume = normalizedSpeed * windVolumeMultiplier;
    }

    void UpdateMotorLoadSound()
    {
        if (motorLoadSound == null) return;

        // Get input (gasInput will vary from -1 (reverse) to 1 (forward))
        float load = Mathf.Abs(carController.gasInput);

        // Adjust volume based on load (acceleration/deceleration)
        motorLoadSound.volume = load * loadVolumeMultiplier;
    }
}
