using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Rigidbody[] _rigidbodies;
    private Animator animator;

    private Rigidbody[] rigidbodies
    {
        get
        {
            if (_rigidbodies == null)
                _rigidbodies = GetComponentsInChildren<Rigidbody>();
            return _rigidbodies;
        }
    }

    private bool isRagdollEnabled = false;
    public bool IsRagdollEnabled => isRagdollEnabled;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        DisableRagdoll();
    }

    private void DisableRagdoll()
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        if (animator != null)
            animator.enabled = true;

        isRagdollEnabled = false;
    }

    public void EnableRagdoll()
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.WakeUp(); // Pastikan rigidbodies aktif
        }

        if (animator != null)
            animator.enabled = false;

        isRagdollEnabled = true;
    }

    private void Update()
    {
        DebugRagdoll();
    }

    private void DebugRagdoll()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnableRagdoll();
        }
    }

    public Rigidbody[] GetRigidbodies()
    {
        return rigidbodies;
    }

}
