using UnityEngine;

public enum PatrolMode { Loop, PingPong }

// Patrol the waypoints, chase inside chaseRange, give up outside giveUpRange, walk back
[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    enum State { Patrol, Chase, Return }

    [SerializeField] Transform[] points;
    [SerializeField] PatrolMode mode = PatrolMode.Loop;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 3.5f;   // slower than the player's 6 m/s, so you can get away
    [SerializeField] float chaseRange = 6f;
    [SerializeField] float giveUpRange = 9f;    // longer than chaseRange, so it can't flicker at one edge
    [SerializeField] float arriveDistance = 0.1f; // bigger than one step (3.5 m/s × 0.02 s = 7 cm)
    [SerializeField] float turnSpeed = 360f;

    Rigidbody rb;
    Transform player;
    State state = State.Patrol;
    int current;
    int step = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        // found here, so one prefab works in every level
        player = FindAnyObjectByType<PlayerController>().transform;
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                MoveTo(points[current].position, patrolSpeed);
                if (Arrived(points[current].position))
                    current = NextIndex();
                if (InRange(chaseRange))
                    state = State.Chase;
                break;

            case State.Chase:
                Vector3 target = player.position;
                target.y = rb.position.y;
                MoveTo(target, chaseSpeed);
                if (!InRange(giveUpRange))
                {
                    current = Nearest();
                    state = State.Return;
                }
                break;

            case State.Return:
                MoveTo(points[current].position, patrolSpeed);
                if (InRange(chaseRange))
                    state = State.Chase;
                else if (Arrived(points[current].position))
                    state = State.Patrol;
                break;
        }
    }

    void MoveTo(Vector3 target, float speed)
    {
        Vector3 next = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        Vector3 dir = target - rb.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.fixedDeltaTime));
        rb.MovePosition(next);
    }

    // arrival by distance, not ==
    bool Arrived(Vector3 target) => Vector3.Distance(rb.position, target) < arriveDistance;

    // seen inside one range, lost outside a longer one
    bool InRange(float range) => Vector3.Distance(rb.position, player.position) < range;

    int NextIndex()
    {
        if (mode == PatrolMode.Loop)
            return (current + 1) % points.Length;

        if (current + step >= points.Length || current + step < 0)
            step = -step;
        return current + step;
    }

    int Nearest()
    {
        int best = 0;
        for (int i = 1; i < points.Length; i++)
            if (Vector3.Distance(rb.position, points[i].position) < Vector3.Distance(rb.position, points[best].position))
                best = i;
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, giveUpRange);
    }
}
