using UnityEngine;

public class LookAtObject : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5f;

    [Tooltip("Which axes are allowed to rotate (1 = enabled, 0 = locked)")]
    public Vector3 rotateFrom = new Vector3(1, 1, 1);

    void Update()
    {
        if (target == null) return;

        // Calculate the direction from this object to the target
        Vector3 direction = target.position - transform.position;

        // Get the desired rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Apply axis constraints
        Vector3 euler = targetRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        if (rotateFrom.x == 0) euler.x = currentEuler.x;
        if (rotateFrom.y == 0) euler.y = currentEuler.y;
        if (rotateFrom.z == 0) euler.z = currentEuler.z;

        // Smoothly rotate towards the target rotation
        Quaternion constrainedRotation = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            constrainedRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
