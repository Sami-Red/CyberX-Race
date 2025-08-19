using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;


    public AudioClip buttonSound;
    public AudioClip gameMusic;
    public AudioClip countdownBeep;
    public AudioClip countdownGo;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        PlayMusic();
    }


    // Button click sound
    public void PlayButtonSound()
    {
        PlaySound(buttonSound);
    }

    // General-purpose menu or event sound
    public void PlayMusic()
    {
        PlaySound(gameMusic);
    }

    // Countdown beep
    public void PlayCountdownBeep()
    {
        PlaySound(countdownBeep);
    }

    // Final "Go!" sound in countdown
    public void PlayCountdownGo()
    {
        PlaySound(countdownGo);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
