using System.Collections;
using UnityEngine;

// one script for every kind of danger: spikes, the guard and the crusher
public class Hazard : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = 8f;    // impulse, N·s, flat and away from the hazard
    [SerializeField] float knockTime = 0.25f; // seconds the player can't steer

    [Header("Crusher only")]
    [SerializeField] bool timed;
    [SerializeField] float restTime = 2f;
    [SerializeField] float warnTime = 0.7f;   // longer than a reaction, so you can still get off
    [SerializeField] float strikeTime = 0.4f;
    [SerializeField] GameObject warning;
    [SerializeField] GameObject slab;

    Collider hurtBox;

    IEnumerator Start()
    {
        hurtBox = GetComponent<Collider>();
        if (!timed)
            yield break;

        while (true)
        {
            // rest: off, so the pad is safe
            hurtBox.enabled = false;
            slab.SetActive(false);
            warning.SetActive(false);
            yield return new WaitForSeconds(restTime);

            // warn first
            warning.SetActive(true);
            yield return new WaitForSeconds(warnTime);

            // strike: the slab shows with the collider, so the hit can be seen
            warning.SetActive(false);
            slab.SetActive(true);
            hurtBox.enabled = true;
            yield return new WaitForSeconds(strikeTime);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out Health health))
            return;

        // only a hit that lands throws you, Health's i-frames decide how often
        if (!health.TakeDamage(damage))
            return;

        Vector3 away = other.transform.position - transform.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f)
            away = -other.transform.forward;
        Vector3 push = away.normalized * knockback;

        if (other.TryGetComponent(out PlayerController p))
            p.Knockback(push, knockTime);
        else if (other.attachedRigidbody != null)
            other.attachedRigidbody.AddForce(push, ForceMode.Impulse);
    }
}
