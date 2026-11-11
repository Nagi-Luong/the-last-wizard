using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Camera cam;
    [SerializeField] private float fireForce = 5f;

    [SerializeField] private float attackSpeed = 5f;
    [SerializeField] private float nextAttack = 0f;

    private Vector3 mousePos;
    private Vector2 lookDir;
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {

        PlayerInventory inv = GetComponentInParent<PlayerInventory>();
        float currentAttackSpeed = attackSpeed;
        if (inv != null) currentAttackSpeed *= inv.AttackSpeedMultiplier;

        if (Time.time >= nextAttack)
        {
            if (Input.GetButton("Fire1"))
            {
                mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                lookDir = (mousePos - firePoint.position).normalized;
                Shoot(inv);
                nextAttack = Time.time + 1f / currentAttackSpeed;
            }
        }
    }

    private void Shoot(PlayerInventory inv)
    {
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        firePoint.eulerAngles = new Vector3(0, 0, angle);

        //spawn bullet
        GameObject bullet = Instantiate(firePrefab, firePoint.position, firePoint.rotation);
        FireAttackScript fireScript = bullet.GetComponent<FireAttackScript>();
        if (fireScript != null && inv != null)
        {
            fireScript.damage *= inv.DamageMultiplier;
            // Kiểm tra tỉ lệ chí mạng
            if (Random.value < inv.CritChance)
            {
                fireScript.damage *= 2f;
                fireScript.isCrit = true;
            }
            fireScript.slowAmount = inv.SlowEffect;
            fireScript.aoeBlastPercent = inv.AoeBlastPercent;
        }

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.up * fireForce, ForceMode2D.Impulse);
        Destroy(bullet, 5f);
    }
}
