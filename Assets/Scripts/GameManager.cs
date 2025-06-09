using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Zombie Settings")]
    [SerializeField] private int maxZombiesToActivate = 5;
    [SerializeField] private TextMeshProUGUI zombieCounterText;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("Ammo Settings")]
    [SerializeField] public int maxAmmoReserve = 7;

    private List<GameObject> allZombies = new List<GameObject>();
    private int zombiesLeft;
    private int zombiesKilled = 0;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        gameOverText.gameObject.SetActive(false);
    }

    void Start()
    {
        // Ambil semua zombie di scene
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");

        allZombies.AddRange(zombies);
        Shuffle(allZombies); // Acak zombie

        // Aktifkan hanya sejumlah maxZombiesToActivate
        for (int i = 0; i < allZombies.Count; i++)
        {
            allZombies[i].SetActive(i < maxZombiesToActivate);
        }

        zombiesLeft = maxZombiesToActivate;
        UpdateZombieCounter();
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
            gameOverText.text = "Horee!!!";
            gameOverText.gameObject.SetActive(true);
        }

        CheckGameOver();
    }

    public void UseOneAmmo()
    {
        maxAmmoReserve = Mathf.Max(0, maxAmmoReserve - 1);
        Debug.Log("Ammo left: " + maxAmmoReserve);
        CheckGameOver();
    }


    private void UpdateZombieCounter()
    {
        if (zombieCounterText != null)
            zombieCounterText.text = $" {zombiesLeft}/{maxZombiesToActivate}";
    }

    private void CheckGameOver()
    {
        if (maxAmmoReserve <= 0 && zombiesLeft > 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Out of ammo.");
        gameOverText.text = "";
        gameOverText.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; 
    }
}
