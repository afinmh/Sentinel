using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineBulletPathController : CinemachinePathController
{
    [SerializeField] LayerMask mask;

    public override bool CheckIfPathISClear(Transform target, float distance, Quaternion orientation)
    {
        if (Physics.BoxCast(target.TransformPoint(boxCollider.center),
            boxCollider.size / 2f, target.forward, out RaycastHit hit,
            orientation, distance, ~mask))
        {
            // Cek apakah objek yang terkena adalah musuh dengan ragdoll
            RagdollController ragdoll = hit.collider.GetComponentInParent<RagdollController>();
            if (ragdoll != null)
            {
                Debug.LogError("Blocked by enemy: " + hit.collider.gameObject.name);
                return false; // Ada musuh dengan ragdoll menghalangi
            }

            return true; // Abaikan semua objek lain
        }

        return true; // Tidak ada penghalang
    }
}
