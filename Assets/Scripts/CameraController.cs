using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The target to follow (e.g., the car)
    private Rigidbody targetRigidbody; // The Rigidbody of the target

    public Vector3 offset; // Offset from the target position
    public float followSpeed = 10f; // Speed of the camera following the target

    void Start()
    {
        targetRigidbody = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        Vector3 targetForward = (targetRigidbody.velocity + target.transform.forward).normalized;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position + new Vector3(0f, offset.y, 0f) + targetForward * offset.z,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target);
    }
}
