using UnityEngine;

namespace Assets.Scripts
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private Texture2D cursorTexture;
        [SerializeField] private Vector2 clickPosition = Vector2.zero;

        void Start()
        {
            Cursor.SetCursor(cursorTexture, clickPosition, CursorMode.Auto);
        }
    }
}
