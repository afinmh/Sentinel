using UnityEngine;
using UnityEditor;

public class SnapChildrenBottomToTerrain : EditorWindow
{
    private GameObject parentObject;
    private Terrain terrain;

    [MenuItem("Tools/Snap Bottom of Children to Terrain")]
    public static void ShowWindow()
    {
        GetWindow<SnapChildrenBottomToTerrain>("Snap Children Bottom");
    }

    void OnGUI()
    {
        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object (City)", parentObject, typeof(GameObject), true);
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        if (GUILayout.Button("Snap All Children Bottom to Terrain"))
        {
            if (parentObject == null || terrain == null)
            {
                Debug.LogError("Parent object atau terrain belum dipilih.");
                return;
            }

            int count = 0;

            foreach (Transform child in parentObject.transform)
            {
                Renderer renderer = child.GetComponentInChildren<Renderer>();
                if (renderer == null)
                {
                    Debug.LogWarning($"Objek '{child.name}' tidak memiliki Renderer, dilewati.");
                    continue;
                }

                Vector3 worldPos = child.position;
                float terrainY = terrain.SampleHeight(worldPos) + terrain.GetPosition().y;

                float bottomY = renderer.bounds.min.y;
                float deltaY = (terrainY +0.1f) - bottomY;

                // Pindahkan objek agar bagian bawahnya tepat di terrain
                child.position += new Vector3(0, deltaY, 0);
                count++;
            }

            Debug.Log($"Snap selesai. {count} objek telah diselaraskan ke terrain.");
        }
    }
}
