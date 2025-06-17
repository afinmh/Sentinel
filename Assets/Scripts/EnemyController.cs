using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAnimationController))]
[RequireComponent(typeof(RagdollController))]
public class EnemyController : MonoBehaviour
{
    private EnemyAnimationController animationController;
    private RagdollController ragdollController;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTimeAtPoint = 1f;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private bool canPatrol = true;
    private GameManager gameManager;

    private void Awake()
    {
        animationController = GetComponent<EnemyAnimationController>();
        ragdollController = GetComponent<RagdollController>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        if (canPatrol && patrolPoints.Length > 0)
        {
            animationController.SetWalking(true);        // Set parameter animator
            animationController.ForcePlayWalk();         // Paksa langsung ke state "Walk"
        }
    }

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (!canPatrol || ragdollController.IsRagdollEnabled) return;

        if (!isWaiting && patrolPoints.Length > 0)
        {
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            transform.position += direction * patrolSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z));

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
            {
                StartCoroutine(WaitBeforeNextPoint());
            }
        }
    }

    private IEnumerator WaitBeforeNextPoint()
    {
        isWaiting = true;
        animationController.SetWalking(false); // Set ke idle
        yield return new WaitForSeconds(waitTimeAtPoint);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
        animationController.SetWalking(true); // Jalan lagi
        animationController.ForcePlayWalk();  // Paksa langsung ke animasi jalan
    }

    public void StopPatrol()
    {
        canPatrol = false;
    }

    public void OnEnemyShot(Vector3 shootDirection, Rigidbody shotRB)
    {
        StopAnimation();
        StopPatrol();
        ragdollController.EnableRagdoll();

        if (shotRB)
        {
            shotRB.WakeUp();
            shotRB.AddForce(shootDirection.normalized * 150f, ForceMode.Impulse);
        }

        foreach (Rigidbody rb in ragdollController.GetRigidbodies())
        {
            rb.WakeUp();
            rb.AddForce(shootDirection * 30f, ForceMode.Impulse);
        }

        AudioManager.Instance.PlayHitSound();

        if (gameManager != null)
        {
            gameManager.OnZombieKilled();
        }
    }

    public void StopAnimation()
    {
        animationController.DisableAnimator();
    }
}
