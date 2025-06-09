using UnityEngine;

public class Alligner : MonoBehaviour
{
    public Terrain terrain;

    void Start()
    {
        foreach (Transform child in transform)
        {
            Vector3 pos = child.position;
            float terrainHeight = terrain.SampleHeight(pos);
            pos.y = terrainHeight;
            child.position = pos;
        }

        Debug.Log("Semua bangunan sudah disejajarkan dengan terrain.");
    }
}
