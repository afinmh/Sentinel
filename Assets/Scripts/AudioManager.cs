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
    [SerializeField] private AudioClip winSong;
    [SerializeField] private AudioClip loseSong;

    [Range(0f, 1f)] [SerializeField] private float mainMenuVolume = 1f; // Volume tinggi
    [Range(0f, 1f)] [SerializeField] private float gameBGMVolume = 0.3f; // Volume rendah
    [Range(0f, 1f)] [SerializeField] private float winSongVolume = 0.8f; // Volume sedang-tinggi
    [Range(0f, 1f)] [SerializeField] private float loseSongVolume = 0.6f; // Volume sedang

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

    // Fungsi mute/unmute untuk dihubungkan ke button
    public void MuteAllSound()
    {
        AudioListener.volume = 0f;
        Debug.Log("[AudioManager] All sound muted.");
    }

    public void UnmuteAllSound()
    {
        AudioListener.volume = 1f;
        Debug.Log("[AudioManager] All sound unmuted.");
    }

    public void ToggleMute()
    {
        if (AudioListener.volume > 0f)
            MuteAllSound();
        else
            UnmuteAllSound();
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
        if (emptyClip != null)
            audioSource.PlayOneShot(emptyClip);
    }

    public void PlayReloadSound()
    {
        if (reloadClip != null)
            audioSource.PlayOneShot(reloadClip);
    }

    public void PlayMainMenuSong()
    {
        if (mainMenuSong != null)
        {
            audioSource.clip = mainMenuSong;
            audioSource.loop = true;
            audioSource.volume = mainMenuVolume;
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

    public void StopCurrentMusic()
    {
        if (audioSource != null)
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
            audioSource.volume = gameBGMVolume;
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

    public void PlayWinSong()
    {
        if (winSong != null)
        {
            audioSource.clip = winSong;
            audioSource.loop = false; // Win song tidak perlu loop
            audioSource.volume = winSongVolume;
            audioSource.Play();
            Debug.Log("[AudioManager] Playing win song.");
        }
    }

    public void PlayLoseSong()
    {
        if (loseSong != null)
        {
            audioSource.clip = loseSong;
            audioSource.loop = false; // Lose song tidak perlu loop
            audioSource.volume = loseSongVolume;
            audioSource.Play();
            Debug.Log("[AudioManager] Playing lose song.");
        }
    }
}
