using UnityEngine;

// one wide trigger under the level: a fall costs a life, not a heart
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health))
            health.Kill();
    }
}
