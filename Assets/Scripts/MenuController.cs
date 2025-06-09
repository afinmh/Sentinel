using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Camera mainMenuCamera;
    [SerializeField] private GameObject crosshairUI; // Tambahkan ini, drag crosshair UI dari Canvas

    private Animator animator;

    private void Start()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Confined; // Biarkan cursor bebas bergerak di menu
        Cursor.visible = false; // Sembunyikan cursor OS

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainMenuSong();

        if (canvas != null)
            canvas.SetActive(false);

        if (player != null)
            player.SetActive(false);

        if (mainMenuCamera != null)
        {
            mainMenuCamera.gameObject.SetActive(true);
            animator = mainMenuCamera.GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.Play("CaAnim");
            }
            else
            {
                Debug.LogWarning("Animator tidak ditemukan di mainMenuCamera!");
            }
        }
        else
        {
            Debug.LogWarning("mainMenuCamera belum diset di inspector!");
        }

        if (crosshairUI != null)
            crosshairUI.SetActive(true); // Tampilkan crosshair di menu
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMainMenuSong();
            AudioManager.Instance.PlayGameBGMWithDelay(0.5f);
        }

        if (canvas != null)
            canvas.SetActive(true);

        if (mainMenuCamera != null)
            mainMenuCamera.gameObject.SetActive(false);

        if (player != null)
            player.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}
