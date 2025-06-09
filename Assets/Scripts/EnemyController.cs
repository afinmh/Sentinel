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

    public void StopPatrol()
    {
        canPatrol = false;
    }

    private IEnumerator WaitBeforeNextPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtPoint);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    public void OnEnemyShot(Vector3 shootDirection, Rigidbody shotRB)
    {
        StopAnimation();
        StopPatrol();

        ragdollController.EnableRagdoll();

        // Tambah gaya hanya ke titik yang terkena (lebih realistis)
        if (shotRB)
        {
            shotRB.WakeUp(); // Ini penting!
            shotRB.AddForce(shootDirection.normalized * 150f, ForceMode.Impulse); // Tambah force
        }

        // Tambah gaya ringan ke semua rigidbody agar ragdoll bereaksi lebih natural
        foreach (Rigidbody rb in ragdollController.GetRigidbodies())
        {
            rb.WakeUp();
            rb.AddForce(shootDirection * 30f, ForceMode.Impulse); // Nilai kecil agar tak berlebihan
        }

        AudioManager.Instance.PlayHitSound();

    // Panggil GameManager
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
