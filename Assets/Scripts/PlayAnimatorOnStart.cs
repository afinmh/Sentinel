using UnityEngine;

public class PlayAnimatorOnStart : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if(animator != null)
        {
            animator.Play("CaAnim");
        }
        else
        {
            Debug.LogWarning("Animator tidak ditemukan!");
        }
    }
}
