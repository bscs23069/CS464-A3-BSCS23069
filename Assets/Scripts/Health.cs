using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHearts = 3;
    [SerializeField] float iFrames = 1f;   // seconds nothing can hurt you after a hit
    [SerializeField] Renderer body;        // blinks while invulnerable

    public int Current { get; private set; }
    public int Max => maxHearts;

    float invulnerableUntil;

    void Awake()
    {
        Current = maxHearts;
    }

    void Update()
    {
        if (body != null)
            body.enabled = Time.time >= invulnerableUntil || Mathf.Repeat(Time.time, 0.2f) < 0.1f;
    }

    // a hazard can ask every physics step, so say no while invulnerable or already down
    public bool TakeDamage(int amount)
    {
        if (Current <= 0 || Time.time < invulnerableUntil)
            return false;

        invulnerableUntil = Time.time + iFrames;
        Current -= amount;
        GameManager.Instance.UpdateHearts();

        // <= 0, never == 0: the crusher's 2 can take 1 heart to -1
        if (Current <= 0)
            GameManager.Instance.PlayerDied();

        return true;
    }

    public void Kill()
    {
        if (Current <= 0)
            return;

        Current = 0;
        GameManager.Instance.UpdateHearts();
        GameManager.Instance.PlayerDied();
    }

    public void Restore(float protection)
    {
        Current = maxHearts;
        invulnerableUntil = Time.time + protection;
        GameManager.Instance.UpdateHearts();
    }
}
