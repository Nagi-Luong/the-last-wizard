using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator animator;

    
    private float horizontal;
    private float vertical;
    private bool isWalking;
    private const float moveLimiter = 0.7f;
    [SerializeField] private float moveSpeed = 2.5f;

    private PlayerStatsScript playerStatsScript;

    private void Start()
    {
        playerStatsScript = GetComponent<PlayerStatsScript>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Gives a value between -1 and 1
        horizontal = Input.GetAxisRaw("Horizontal"); // -1 is left
        vertical = Input.GetAxisRaw("Vertical"); // -1 is down

        if (horizontal == 1) //right
        {
            animator.SetInteger("Direction", 0);
        }
        if (horizontal == -1) // left
        {
            animator.SetInteger("Direction", 1);
        }
        if (vertical == 1) //up
        {
            animator.SetInteger("Direction", 2);
        }
        if (vertical == -1) //down
        {
            animator.SetInteger("Direction", 3);
        }

    }

    void FixedUpdate()
    {

        if (horizontal != 0 && vertical != 0) // Check for diagonal movement
        {
            // limit movement speed diagonally, so you move at 70% speed
            horizontal *= moveLimiter;
            vertical *= moveLimiter;
        }
      
        float currentMoveSpeed = moveSpeed;
        PlayerInventory inv = GetComponent<PlayerInventory>();
        if (inv != null) currentMoveSpeed *= inv.MoveSpeedMultiplier;

        body.linearVelocity = new Vector2(horizontal * currentMoveSpeed, vertical * currentMoveSpeed);

        if(playerStatsScript.playerHealth <= 0)
        {
            body.linearVelocity = Vector2.zero;
        }
        // animator.SetBool("IsWalking", isWalking = body.velocity != Vector2.zero);
    }
}
