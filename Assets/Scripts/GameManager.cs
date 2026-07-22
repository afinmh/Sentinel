using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // ← Tambahkan ini di atas

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public class GameManager : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [SerializeField] private DifficultyLevel currentDifficulty;
    
    [Header("Zombie Settings")]
    [SerializeField] private TextMeshProUGUI zombieCounterText;

    [Header("UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject loseText;
    [SerializeField] private TMPro.TextMeshProUGUI winModeText;
    [SerializeField] private TMPro.TextMeshProUGUI loseModeText;

    [Header("Player & Camera")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject winPlayer;
    [SerializeField] private GameObject losePlayer;
    [SerializeField] private Camera winCamera;
    [SerializeField] private Transform winCameraPosition;
    [SerializeField] private ShootController shootController;

    [Header("Ammo Settings")]
    [SerializeField] public int maxAmmoReserve = 7;

    // Difficulty-based settings
    private Dictionary<DifficultyLevel, DifficultySettings> difficultySettings = new Dictionary<DifficultyLevel, DifficultySettings>()
    {
        { DifficultyLevel.Easy, new DifficultySettings { zombiesToActivate = 4, totalZombies = 10, description = "Easy locations" } },
        { DifficultyLevel.Medium, new DifficultySettings { zombiesToActivate = 4, totalZombies = 10, description = "Hidden locations" } },
        { DifficultyLevel.Hard, new DifficultySettings { zombiesToActivate = 5, totalZombies = 10, description = "Moving & hidden locations" } }
    };

    private List<GameObject> easyZombies = new List<GameObject>();
    private List<GameObject> mediumZombies = new List<GameObject>();
    private List<GameObject> hardZombies = new List<GameObject>();
    private List<GameObject> activeZombies = new List<GameObject>();
    
    private int zombiesLeft;
    private int zombiesKilled = 0;
    private int maxZombiesToActivate;
    private bool difficultySet = false;

    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public class DifficultySettings
    {
        public int zombiesToActivate;
        public int totalZombies;
        public string description;
    }

    private void Awake()
    {
        Instance = this;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (winCamera != null) winCamera.gameObject.SetActive(false);
        if (winPlayer != null) winPlayer.SetActive(false);
        if (losePlayer != null) losePlayer.SetActive(false);
        if (winText != null) winText.SetActive(false);
        if (loseText != null) loseText.SetActive(false);
        
        // Jangan update mode text di awal karena difficulty belum diset
        // UpdateModeText();
    }

void Start()
{
    InitializeZombiesByDifficulty();
    // Hanya aktifkan zombie jika difficulty sudah diset
    if (difficultySet)
    {
        ActivateZombiesForCurrentDifficulty();
    }
}

private void InitializeZombiesByDifficulty()
{
    // Cari zombie berdasarkan tag difficulty
    GameObject[] easy = GameObject.FindGameObjectsWithTag("EasyZombie");
    GameObject[] medium = GameObject.FindGameObjectsWithTag("MediumZombie");
    GameObject[] hard = GameObject.FindGameObjectsWithTag("HardZombie");

    easyZombies.AddRange(easy);
    mediumZombies.AddRange(medium);
    hardZombies.AddRange(hard);

    Debug.Log($"Found zombies - Easy: {easyZombies.Count}, Medium: {mediumZombies.Count}, Hard: {hardZombies.Count}");

    // Pastikan semua zombie inactive di awal
    DeactivateAllZombies();
}

private void DeactivateAllZombies()
{
    foreach (var zombie in easyZombies)
        if (zombie != null) zombie.SetActive(false);
    
    foreach (var zombie in mediumZombies)
        if (zombie != null) zombie.SetActive(false);
    
    foreach (var zombie in hardZombies)
        if (zombie != null) zombie.SetActive(false);
}

private void ActivateZombiesForCurrentDifficulty()
{
    var settings = difficultySettings[currentDifficulty];
    maxZombiesToActivate = settings.zombiesToActivate;
    
    List<GameObject> zombiesToUse = new List<GameObject>();
    
    switch (currentDifficulty)
    {
        case DifficultyLevel.Easy:
            zombiesToUse = easyZombies;
            break;
        case DifficultyLevel.Medium:
            zombiesToUse = mediumZombies;
            break;
        case DifficultyLevel.Hard:
            zombiesToUse = hardZombies;
            break;
    }

    if (zombiesToUse.Count < settings.totalZombies)
    {
        Debug.LogWarning($"Not enough {currentDifficulty} zombies found! Expected: {settings.totalZombies}, Found: {zombiesToUse.Count}");
    }

    // Shuffle dan aktifkan zombie sesuai difficulty
    Shuffle(zombiesToUse);
    activeZombies.Clear();

    int activatedCount = Mathf.Min(maxZombiesToActivate, zombiesToUse.Count);
    for (int i = 0; i < zombiesToUse.Count; i++)
    {
        bool shouldActivate = i < activatedCount;
        if (zombiesToUse[i] != null)
        {
            zombiesToUse[i].SetActive(shouldActivate);
            if (shouldActivate)
            {
                activeZombies.Add(zombiesToUse[i]);
            }
        }
    }

    zombiesLeft = activatedCount;
    UpdateZombieCounter();

    Debug.Log($"Difficulty: {currentDifficulty} - Activated {activatedCount} zombies out of {zombiesToUse.Count} available");
}

// Method untuk mengubah difficulty (dipanggil dari MenuController)
public void SetDifficulty(int difficultyIndex)
{
    currentDifficulty = (DifficultyLevel)difficultyIndex;
    difficultySet = true;
    Debug.Log($"Difficulty set to: {currentDifficulty}");
    UpdateModeText(); // Update mode text saat difficulty berubah
    ActivateZombiesForCurrentDifficulty(); // Aktifkan zombie setelah difficulty diset
}

public void SetDifficulty(DifficultyLevel difficulty)
{
    currentDifficulty = difficulty;
    difficultySet = true;
    Debug.Log($"Difficulty set to: {currentDifficulty}");
    UpdateModeText(); // Update mode text saat difficulty berubah
    ActivateZombiesForCurrentDifficulty(); // Aktifkan zombie setelah difficulty diset
}

// Update text mode berdasarkan difficulty yang dipilih
private void UpdateModeText()
{
    if (!difficultySet) return; // Jangan update jika difficulty belum diset
    
    string modeText = currentDifficulty.ToString().ToUpper();
    
    if (winModeText != null)
    {
        winModeText.text = modeText;
        Debug.Log($"Win mode text set to: {modeText}");
    }
    
    if (loseModeText != null)
    {
        loseModeText.text = modeText;
        Debug.Log($"Lose mode text set to: {modeText}");
    }
    
    Debug.Log($"Mode text updated to: {modeText}");
}


    private void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void OnZombieKilled()
    {
        zombiesKilled++;
        zombiesLeft--;
        UpdateZombieCounter();

        if (zombiesLeft <= 0)
        {
            Debug.Log("Semua zombie telah dikalahkan!");
            GameOver(true); // Menang
        }

        CheckGameOver();
    }

    public void UseOneAmmo()
    {
        maxAmmoReserve = Mathf.Max(0, maxAmmoReserve - 1);
        Debug.Log("Ammo left: " + maxAmmoReserve);

    }

    private void UpdateZombieCounter()
    {
        if (zombieCounterText != null)
            zombieCounterText.text = $" {zombiesLeft}/{maxZombiesToActivate}";
    }

    private void CheckGameOver()
    {
        // Lose hanya jika current ammo dan reserve kosong
        if (maxAmmoReserve <= 0 && zombiesLeft > 0 && GetCurrentAmmo() == 0)
        {
            GameOver(false); // Kalah
        }
    }

    // Helper untuk akses current ammo dari ShootController
    public int GetCurrentAmmo()
    {
        var shootController = FindObjectOfType<ShootController>();
        if (shootController != null)
        {
            var field = shootController.GetType().GetField("currentAmmo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                return (int)field.GetValue(shootController);
        }
        return 0;
    }

    public void GameOver(bool isWin)
    {
        Debug.Log(isWin ? "Menang!" : "Game Over! Out of ammo.");
        
        // Disable shooting controller immediately when game ends
        if (shootController != null)
        {
            shootController.enabled = false;
            Debug.Log("ShootController disabled");
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (isWin)
        {
            StartCoroutine(WinTextSequence());
        }
        else
        {
            StartCoroutine(LoseTextSequence());
        }
    }

    private System.Collections.IEnumerator WinTextSequence()
    {
        // Stop BGM dan play win song
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCurrentMusic();
            
        }
        
        if (winText != null) winText.SetActive(true);
        yield return new WaitForSeconds(10f);
        AudioManager.Instance.PlayWinSong();
        if (winText != null) winText.SetActive(false);
        
        // Ensure shooting controller is disabled during win sequence
        if (shootController != null)
            shootController.enabled = false;
            
        if (player != null)
            player.SetActive(false);
        if (winCamera != null)
        {
            winCamera.gameObject.SetActive(true);
            if (winCameraPosition != null)
            {
                winCamera.transform.position = winCameraPosition.position;
                winCamera.transform.rotation = winCameraPosition.rotation;
            }
        }
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
        if (winPlayer != null) winPlayer.SetActive(true);
        if (losePlayer != null) losePlayer.SetActive(false);
    }

    private System.Collections.IEnumerator LoseTextSequence()
    {
        // Stop BGM dan play lose song
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCurrentMusic();
            
        }
        
        if (loseText != null) loseText.SetActive(true);
        yield return new WaitForSeconds(10f);
        AudioManager.Instance.PlayLoseSong();
        if (loseText != null) loseText.SetActive(false);
        
        // Ensure shooting controller is disabled during lose sequence
        if (shootController != null)
            shootController.enabled = false;
            
        if (player != null)
            player.SetActive(false);
        if (winCamera != null)
        {
            winCamera.gameObject.SetActive(true);
            if (winCameraPosition != null)
            {
                winCamera.transform.position = winCameraPosition.position;
                winCamera.transform.rotation = winCameraPosition.rotation;
            }
        }
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);
        if (winPlayer != null) winPlayer.SetActive(false);
        if (losePlayer != null) losePlayer.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Normalisasi waktu
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload scene saat ini
    }

    // Getter untuk info difficulty
    public DifficultyLevel GetCurrentDifficulty()
    {
        return currentDifficulty;
    }

    public int GetZombiesLeftCount()
    {
        return zombiesLeft;
    }
}
