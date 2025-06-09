using UnityEngine;
using System.Collections; // Tambahkan ini

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    [SerializeField] private AudioClip shootingClip;
    [SerializeField] private AudioClip trailClip; 
    [SerializeField] private AudioClip hitClip;       
    [SerializeField] private AudioClip emptyClip;     
    [SerializeField] private AudioClip reloadClip; 
    [SerializeField] private AudioClip mainMenuSong; // Tambahkan ini
    [SerializeField] private AudioClip gameBGM; // Tambahkan ini

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
        if (hitClip != null)
            audioSource.PlayOneShot(emptyClip);
    }

    public void PlayReloadSound()
    {
        if (hitClip != null)
            audioSource.PlayOneShot(reloadClip);
    }

    public void PlayMainMenuSong()
    {
        if (mainMenuSong != null)
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
        if (gameBGM != null)
        {
            audioSource.clip = gameBGM;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlayGameBGMWithDelay(float delaySeconds = 2f)
    {
        StartCoroutine(PlayGameBGMRoutine(delaySeconds));
    }

    private IEnumerator PlayGameBGMRoutine(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlayGameBGM();
    }
}
