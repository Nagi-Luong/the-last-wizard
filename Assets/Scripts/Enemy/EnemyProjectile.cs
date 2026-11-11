using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float speed = 5f;
    public float lifetime = 5f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(Vector2 direction, float dmg, float spd = 5f)
    {
        damage = dmg;
        speed = spd;
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStatsScript stats = collision.GetComponent<PlayerStatsScript>();
            if (stats != null)
            {
                stats.TakeDamage((int)damage);
            }
            Destroy(gameObject);
        }
        else if (!collision.CompareTag("Enemy") && !collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
