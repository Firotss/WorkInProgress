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

    private AudioSource source;
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
}
