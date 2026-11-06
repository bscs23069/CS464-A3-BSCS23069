using UnityEngine;

// one kick straight up, the same height whatever the mass or how you landed on it
public class BouncePad : MonoBehaviour
{
    [SerializeField] float bounceSpeed = 9f; // m/s up: v² / 2g = 81 / 19.6 = 4.1 m

    void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body == null)
            return;

        // VelocityChange ignores mass; cancel the fall first so every bounce is the same
        float change = bounceSpeed - body.linearVelocity.y;
        body.AddForce(Vector3.up * change, ForceMode.VelocityChange);
    }
}
