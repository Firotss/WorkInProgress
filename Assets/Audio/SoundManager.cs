using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Assign clips here, or place .wav in Assets/Resources/Sounds/")]
    [SerializeField] private AudioClip playCard;
    [SerializeField] private AudioClip takeBackCard;
    [SerializeField] private AudioClip shuffle;
    [SerializeField] private AudioClip endTurn;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0f, 1f)]
    [SerializeField] private float backgroundMusicVolume = 0.6f;

    private AudioSource source;
    private AudioSource musicSource;
    private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();
    private static bool loggedMissing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        source = GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = 1f;
        source.mute = false;

        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = backgroundMusicVolume;
        musicSource.mute = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private AudioClip GetClip(string resourceName, AudioClip serialized)
    {
        if (serialized != null) return serialized;
        if (Cache.TryGetValue(resourceName, out AudioClip cached)) return cached;
        AudioClip loaded = Resources.Load<AudioClip>($"Sounds/{resourceName}");
        if (loaded == null)
            loaded = Resources.Load<AudioClip>(resourceName);
        if (loaded != null) Cache[resourceName] = loaded;
        return loaded;
    }

    private void Play(AudioClip clip, string actionName)
    {
        if (source == null) return;
        if (clip == null)
        {
            if (!loggedMissing)
            {
                Debug.LogWarning($"SoundManager: No clip for '{actionName}'. Assign in Inspector on GameManager > SoundManager, or add {actionName}.wav to Assets/Resources/Sounds/");
                loggedMissing = true;
            }
            return;
        }
        source.PlayOneShot(clip);
    }

    public void PlayCardPlaced()
    {
        Play(GetClip("Play_card", playCard), "Play_card");
    }

    public void PlayCardWithdrawn()
    {
        Play(GetClip("Take_back_card", takeBackCard), "Take_back_card");
    }

    public void PlayShuffle()
    {
        Play(GetClip("Shuffle", shuffle), "Shuffle");
    }

    public void PlayEndTurn()
    {
        Play(GetClip("End_Turn", endTurn), "End_Turn");
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource == null) return;
        AudioClip clip = backgroundMusic != null ? backgroundMusic : GetClip("Background_Music", null);
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: Background_Music.wav not found in Assets/Resources/Sounds/. Assign in Inspector or add the file.");
            return;
        }
        if (musicSource.isPlaying && musicSource.clip == clip)
            return;
        musicSource.clip = clip;
        musicSource.volume = backgroundMusicVolume;
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }
}
