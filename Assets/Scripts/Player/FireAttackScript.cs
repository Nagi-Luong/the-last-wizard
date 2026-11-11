using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAttackScript : MonoBehaviour
{
    public float damage = 20f;
    public bool isCrit = false;
    public float slowAmount = 0f;
    public float aoeBlastPercent = 0f;

    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject particleEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyScript enemy = collision.GetComponent<EnemyScript>();
        PlayerControllerScript player = collision.GetComponent<PlayerControllerScript>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // Hiệu ứng làm chậm (Cold Snap)
            if (slowAmount > 0f)
            {
                enemy.ApplySlow(slowAmount, 2f);
            }

            // Sát thương nổ lan (Abyss Nova)
            if (aoeBlastPercent > 0f)
            {
                Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 2f);
                float aoeDamage = damage * aoeBlastPercent;
                foreach (var col in nearbyEnemies)
                {
                    EnemyScript nearby = col.GetComponent<EnemyScript>();
                    if (nearby != null && nearby != enemy)
                    {
                        nearby.TakeDamage(aoeDamage);
                    }
                }
            }
        }

        if (player == null)
        {
            if (impactEffect != null)
            {
                GameObject effect = Instantiate(impactEffect, transform.position, transform.rotation);
                Destroy(effect, 1f);
            }

            if (particleEffect != null)
            {
                GameObject particles = Instantiate(particleEffect, transform.position, transform.rotation);
                Destroy(particles, 3f);
            }

            Destroy(gameObject);
        }
    }
}
