using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The target to follow (e.g., the car)
    private Rigidbody targetRigidbody; // The Rigidbody of the target

    public Vector3 offset1;
    public Vector3 offset2;
    public Vector3 offset3;
    public float followSpeed = 10f; // Speed of the camera following the target

    private int currentOffsetIndex = 0;

    void Start()
    {
        targetRigidbody = target.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check for input to change the camera offset
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentOffsetIndex = (currentOffsetIndex + 1) % 3; // Cycle through offsets
        }
    }

    void LateUpdate()
    {
        Vector3 targetForward = (targetRigidbody.velocity + target.transform.forward).normalized;

        Vector3 offset = Vector3.zero;

        switch (currentOffsetIndex)
        {
            case 0:
                offset = offset1;
                break;
            case 1:
                offset = offset2;
                break;
            case 2:
                offset = offset3;
                break;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            target.position + new Vector3(offset.x, offset.y, 0f) + targetForward * offset.z,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target);
    }
}
