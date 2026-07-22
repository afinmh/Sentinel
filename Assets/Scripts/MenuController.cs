using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject idlePlayer;
    [SerializeField] private GameObject winPlayer;
    [SerializeField] private GameObject losePlayer;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Camera mainMenuCamera;
    [SerializeField] private GameObject crosshairUI;

    [Header("Camera Transitions")]
    [SerializeField] private Transform mainMenuPosition;
    [SerializeField] private Transform settingPosition; // posisi kamera untuk setting
    [SerializeField] private Transform difficultyPosition; // posisi kamera untuk difficulty
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("UI Panels")]
    [SerializeField] private GameObject modeButtonsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject difficultyPanel;

    [Header("Custom Camera Path")]
    [SerializeField] private Transform[] modePathWaypoints; // Track ke mode selection

    private bool isTransitioning = false;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainMenuSong();

        if (canvas != null)
            canvas.SetActive(false);

        if (player != null)
            player.SetActive(false);

        if (idlePlayer != null)
            idlePlayer.SetActive(true);
        if (winPlayer != null)
            winPlayer.SetActive(false);
        if (losePlayer != null)
            losePlayer.SetActive(false);

        if (mainMenuCamera != null)
        {
            mainMenuCamera.transform.position = mainMenuPosition.position;
            mainMenuCamera.transform.rotation = mainMenuPosition.rotation;
            mainMenuCamera.gameObject.SetActive(true);
        }

        if (crosshairUI != null)
            crosshairUI.SetActive(true);

        if (modeButtonsPanel != null)
            modeButtonsPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (!isTransitioning)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            StartCoroutine(MoveCameraAlongPath(modePathWaypoints, () =>
            {
                if (modeButtonsPanel != null)
                    modeButtonsPanel.SetActive(true);
            }));
        }
    }

    public void GoToSetting()
    {
        if (!isTransitioning)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            StartCoroutine(MoveCameraTo(settingPosition, () =>
            {
                if (settingsPanel != null)
                    settingsPanel.SetActive(true);
            }));
        }
    }

    // Back dari MODE ke Main Menu lewat jalur kamera
    public void BackFromModeToMenu()
    {
        if (!isTransitioning)
        {
            if (modeButtonsPanel != null)
                modeButtonsPanel.SetActive(false);

            // Balik arah jalur mode
            Transform[] reversedPath = (Transform[])modePathWaypoints.Clone();
            System.Array.Reverse(reversedPath);

            StartCoroutine(MoveCameraAlongPath(reversedPath, () =>
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);
            }));
        }
    }

    // Back dari SETTINGS ke Main Menu langsung
    public void BackFromSettingToMenu()
    {
        if (!isTransitioning)
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            StartCoroutine(MoveCameraTo(mainMenuPosition, () =>
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);
            }));
        }
    }

    public void ChooseStandardMode()
    {
        ShowDifficultySelection();
    }

    public void ChooseArcadeMode()
    {
        ShowDifficultySelection();
    }

    private void ShowDifficultySelection()
    {
        if (!isTransitioning)
        {
            if (modeButtonsPanel != null)
                modeButtonsPanel.SetActive(false);

            StartCoroutine(MoveCameraTo(difficultyPosition, () =>
            {
                if (difficultyPanel != null)
                    difficultyPanel.SetActive(true);
            }));
        }
    }

    // Difficulty selection methods - langsung start game
    public void SelectEasyDifficulty()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(0); // Easy
            BeginGameplay();
        }
    }

    public void SelectMediumDifficulty()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(1); // Medium
            BeginGameplay();
        }
    }

    public void SelectHardDifficulty()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(2); // Hard
            BeginGameplay();
        }
    }

    public void BackFromDifficultyToMode()
    {
        if (!isTransitioning)
        {
            if (difficultyPanel != null)
                difficultyPanel.SetActive(false);

            // Kembali langsung ke posisi terakhir dari modePathWaypoints
            Transform targetPosition = (modePathWaypoints != null && modePathWaypoints.Length > 0) 
                ? modePathWaypoints[modePathWaypoints.Length - 1] 
                : mainMenuPosition;

            StartCoroutine(MoveCameraTo(targetPosition, () =>
            {
                if (modeButtonsPanel != null)
                    modeButtonsPanel.SetActive(true);
            }));
        }
    }

    private void BeginGameplay()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        modeButtonsPanel.SetActive(false);
        difficultyPanel.SetActive(false);

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

        if (idlePlayer != null)
            idlePlayer.SetActive(false);
    }

    private IEnumerator MoveCameraTo(Transform target, System.Action onComplete = null)
    {
        isTransitioning = true;

        float elapsed = 0f;
        Vector3 startPos = mainMenuCamera.transform.position;
        Quaternion startRot = mainMenuCamera.transform.rotation;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            mainMenuCamera.transform.position = Vector3.Lerp(startPos, target.position, t);
            mainMenuCamera.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        mainMenuCamera.transform.position = target.position;
        mainMenuCamera.transform.rotation = target.rotation;

        isTransitioning = false;
        onComplete?.Invoke();
    }

private IEnumerator MoveCameraAlongPath(Transform[] pathPoints, System.Action onComplete = null)
{
    isTransitioning = true;
    if (pathPoints == null || pathPoints.Length == 0)
    {
        isTransitioning = false;
        onComplete?.Invoke();
        yield break;
    }

    // Gabungkan semua segment menjadi satu lintasan panjang
    float totalDuration = transitionDuration * pathPoints.Length;
    float elapsed = 0f;
    Vector3 startPos = mainMenuCamera.transform.position;
    Quaternion startRot = mainMenuCamera.transform.rotation;

    // Buat array posisi dan rotasi
    Vector3[] positions = new Vector3[pathPoints.Length + 1];
    Quaternion[] rotations = new Quaternion[pathPoints.Length + 1];
    positions[0] = startPos;
    rotations[0] = startRot;
    for (int i = 0; i < pathPoints.Length; i++)
    {
        positions[i + 1] = pathPoints[i].position;
        rotations[i + 1] = pathPoints[i].rotation;
    }

    int segmentCount = pathPoints.Length;
    while (elapsed < totalDuration)
    {
        float t = elapsed / totalDuration;
        float pathT = t * segmentCount;
        int seg = Mathf.FloorToInt(pathT);
        float segT = pathT - seg;
        seg = Mathf.Clamp(seg, 0, segmentCount - 1);

        // Interpolasi posisi dan rotasi antar segment
        Vector3 pos = Vector3.Lerp(positions[seg], positions[seg + 1], segT);
        Quaternion rot = Quaternion.Slerp(rotations[seg], rotations[seg + 1], segT);
        mainMenuCamera.transform.position = pos;
        mainMenuCamera.transform.rotation = rot;

        elapsed += Time.unscaledDeltaTime;
        yield return null;
    }
    // Pastikan di akhir path
    mainMenuCamera.transform.position = positions[positions.Length - 1];
    mainMenuCamera.transform.rotation = rotations[rotations.Length - 1];

    isTransitioning = false;
    onComplete?.Invoke();
}

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}
