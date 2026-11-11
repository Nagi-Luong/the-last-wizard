using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalkScript : StateMachineBehaviour
{
    private Transform player;
    private Transform enemy;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private EnemyScript enemyScript;
    private BoxCollider2D boxCollider;
    //private int sortAbove = 11; //when enemy is above player
    //private int sortBelow = 9; //when enemy is below player

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemy = animator.GetComponent<Transform>();
        enemyScript = animator.GetComponentInParent<EnemyScript>();
        boxCollider = animator.GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sprite = animator.GetComponentInChildren<SpriteRenderer>();
        rb = animator.GetComponentInParent<Rigidbody2D>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemyScript != null && enemyScript.enemyHealth > 0)
        {
            LookAtPlayer();
            float currentSpeed = enemyScript.moveSpeed;
            float currentRange = enemyScript.attackRange;

            Vector2 target = new Vector2(player.position.x, player.position.y);
            Vector2 newPosition = Vector2.MoveTowards(rb.position, target, currentSpeed * Time.deltaTime);
            rb.MovePosition(newPosition);
       
            if (Vector2.Distance(player.position, rb.position) <= currentRange)
            {
                animator.SetTrigger("Attack");
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Attack");
    }

    public void LookAtPlayer()
    {
        //visual layering? for when player is with enemy either below or above
        /* if (enemy.position.y  > player.position.x)
         {
             sprite.sortingOrder = sortBelow;
         }
         if (enemy.position.y  < player.position.y)
         {
             sprite.sortingOrder = sortAbove;
         } */
        if (enemy.position.x > player.position.x)
        {
            sprite.flipX = true;
            boxCollider.offset = new Vector2(-0.1702045f, 0.041116f);
        }
        if (enemy.position.x < player.position.x)
        {
            sprite.flipX = false;
            boxCollider.offset = new Vector2(0.1702045f, 0.041116f);
        }

    }

}