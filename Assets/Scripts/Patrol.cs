using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Patrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTimeAtPoint = 1f;

    private Animator animator;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isWaiting || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position += direction * patrolSpeed * Time.deltaTime;

        // Hadapkan ke arah tujuan
        transform.LookAt(new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z));

        // Aktifkan animasi jalan
        animator.SetBool("isWalking", true);

        // Cek jika sudah sampai titik tujuan
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            StartCoroutine(WaitBeforeNextPoint());
        }
    }

    private IEnumerator WaitBeforeNextPoint()
    {
        animator.SetBool("isWalking", false); // Hentikan animasi saat menunggu
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtPoint);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    public void StopPatrol()
    {
        animator.SetBool("isWalking", false);
        enabled = false; // Nonaktifkan skrip ini
    }
}
