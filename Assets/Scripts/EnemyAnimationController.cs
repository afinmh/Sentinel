using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void DisableAnimator()
    {
        animator.enabled = false;
    }

    public void SetWalking(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking); // Pastikan ada parameter "isWalking" di Animator
    }

        public void ForcePlayWalk()
    {
        // Langsung paksa ganti ke animasi Walk (nama state harus sesuai di Animator)
        animator.Play("walk", 0, 0f); // Layer 0, time = 0 (dari awal)
    }
}
