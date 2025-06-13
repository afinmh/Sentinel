using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    [SerializeField] private AudioClip shootingClip;
    [SerializeField] private AudioClip trailClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip emptyClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip mainMenuSong;
    [SerializeField] private AudioClip gameBGM;

    private bool isMusicOn = true; // Musik ON by default

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        PlayMainMenuSong();
    }

    // ======== Sound Effects =========
    public void PlayShootingSound()
    {
        if (shootingClip != null)
            audioSource.PlayOneShot(shootingClip);
    }

    public void PlayTrailSound()
    {
        if (trailClip != null)
            audioSource.PlayOneShot(trailClip);
    }

    public void PlayHitSound()
    {
        if (hitClip != null)
            audioSource.PlayOneShot(hitClip);
    }

    public void PlayBulletEmpty()
    {
        if (emptyClip != null)
            audioSource.PlayOneShot(emptyClip);
    }

    public void PlayReloadSound()
    {
        if (reloadClip != null)
            audioSource.PlayOneShot(reloadClip);
    }

    // ======== BGM =========
    public void PlayMainMenuSong()
    {
        if (mainMenuSong != null && isMusicOn)
        {
            audioSource.clip = mainMenuSong;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopMainMenuSong()
    {
        if (audioSource != null && audioSource.clip == mainMenuSong)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    public void PlayGameBGM()
    {
        if (gameBGM != null && isMusicOn)
        {
            audioSource.clip = gameBGM;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlayGameBGMWithDelay(float delaySeconds = 2f)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("AudioManager GameObject is not active, cannot start coroutine.");
            return;
        }
        StartCoroutine(PlayGameBGMRoutine(delaySeconds));
    }


    private IEnumerator PlayGameBGMRoutine(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlayGameBGM();
    }

    // ======== Toggle Music =========
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;

        if (isMusicOn)
        {
            // Lanjutkan lagu sesuai mode
            if (audioSource.clip == mainMenuSong)
                PlayMainMenuSong();
            else if (audioSource.clip == gameBGM)
                PlayGameBGM();
        }
        else
        {
            audioSource.Stop();
        }

        Debug.Log("Music toggled: " + (isMusicOn ? "ON" : "OFF"));
    }
}
