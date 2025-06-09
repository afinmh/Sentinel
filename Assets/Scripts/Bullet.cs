using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float raycastRadius = 0.05f;

    private Transform hitTransform;
    private bool isEnemyShot;
    private float shootingForce;
    private Vector3 direction;
    private Vector3 hitPoint;
    private Vector3 lastPosition;

    public void Launch(float shootingForce, Transform hitTransform, Vector3 hitPoint)
    {
        direction = (hitPoint - transform.position).normalized;
        isEnemyShot = false;
        this.hitTransform = hitTransform;
        this.shootingForce = shootingForce;
        this.hitPoint = hitPoint;
        lastPosition = transform.position;
    }

    private void Update()
    {
        RaycastAndMove();
        Rotate();
    }

    private void RaycastAndMove()
    {
        Vector3 moveVector = direction * shootingForce * Time.deltaTime;
        Vector3 newPosition = transform.position + moveVector;

        // Raycast dari posisi lama ke posisi baru
        if (Physics.SphereCast(transform.position, raycastRadius, direction, out RaycastHit hit, moveVector.magnitude))
        {
            if (!isEnemyShot && hit.transform == hitTransform)
            {
                EnemyController enemy = hit.transform.GetComponentInParent<EnemyController>();
                if (enemy)
                {
                    ShootEnemy(hit.transform, enemy);
                }
            }

            // Hancurkan peluru saat tabrak
            Destroy(gameObject);
            return;
        }

        // Pindahkan posisi jika tidak kena apa-apa
        transform.position = newPosition;
        lastPosition = transform.position;
    }

    private void Rotate()
    {
        if (visualTransform != null)
            visualTransform.Rotate(Vector3.forward, 1200 * Time.deltaTime, Space.Self);
    }

    private void ShootEnemy(Transform hitTransform, EnemyController enemy)
    {
        isEnemyShot = true;
        Rigidbody shotRB = hitTransform.GetComponent<Rigidbody>();
        enemy.OnEnemyShot(direction, shotRB);
    }

    public float GetBulletSpeed() => shootingForce;
    internal Transform GetHitEnemyTransform() => hitTransform;
}