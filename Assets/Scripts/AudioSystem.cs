using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioSystem : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource mainSource; // Used for background music
    [SerializeField] AudioSource secondarySource; // Used for other clips that interrupt the bacground music, without resetting the main source
    [SerializeField] AudioSource effectsSource; // USed for small clips to be played over the background music

    [Header("Background Music Clips")]
    [SerializeField] AudioClip level1Music;
    [SerializeField] AudioClip level2Music;
    [SerializeField] AudioClip level3Music;

    [Header("Audio Controls")]
    [SerializeField] float fadeTime;

    [Header("")]
    [SerializeField] List<SoundEffect> soundEffects;

    public static AudioSystem instance;

    Dictionary<SoundEffectType, AudioClip> soundEffectDict;

    int currentLevelAudio = 0;

    bool isFading = false;
    bool interrupted = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        soundEffectDict = new();

        foreach (var effect in soundEffects)
        {
            soundEffectDict.Add(effect.type, effect.clip);
        }

        mainSource.loop = true;
        secondarySource.loop = false;
        effectsSource.loop = false;

        DontDestroyOnLoad(gameObject); // Keep alive between scenes for a smooth transition
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainSource.clip = level1Music;

        PlayMusicForLevel(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void PlayMusicForLevel(int level)
    {
        if (currentLevelAudio == level || interrupted) return;

        currentLevelAudio = level;

        if (mainSource.isPlaying) StartCoroutine(SwapAudio(fadeTime, mainSource, GetClipOfLevel(level)));
        else StartCoroutine(FadeAudio(fadeTime, mainSource, true, GetClipOfLevel(level)));
    }

    public void PlaySoundEffect(SoundEffectType type)
    {
        if (soundEffectDict.TryGetValue(type, out AudioClip clip))
        {
            effectsSource.clip = clip;
            effectsSource.volume = 1.0f;
            effectsSource.Play();
        }
    }

    public void Interrupt(AudioClip clip)
    {
        if (interrupted) return;

        StartCoroutine(InterruptAudio(0.6f, mainSource, effectsSource, clip));
    }

    AudioClip GetClipOfLevel(int level)
    {
        switch (level)
        {
            case 1:  return level1Music;
            case 2:  return level2Music;
            case 3:  return level3Music;
            default: return level1Music;
        }
    }

    // // COROUTINES // //
    IEnumerator InterruptAudio(float waitTime, AudioSource mainSource, AudioSource secondarySource, AudioClip clip)
    {
        while (interrupted) yield return null;
        interrupted = true;

        yield return FadeAudio(fadeTime, mainSource, false);

        yield return new WaitForSeconds(waitTime);

        secondarySource.clip = clip;
        secondarySource.volume = 1.0f;
        secondarySource.Play();

        while (secondarySource.isPlaying) yield return null;

        yield return new WaitForSeconds(waitTime);

        yield return FadeAudio(fadeTime, mainSource, true);

        interrupted = false;

        yield return null;
    }

    IEnumerator SwapAudio(float fadeTime, AudioSource source, AudioClip newClip)
    {
        while (isFading) yield return null;
        isFading = true;
        yield return FadeAudio(fadeTime, source, false);

        source.clip = newClip;

        yield return FadeAudio(fadeTime, source, true);
        isFading = false;

        yield return null;
    }

    IEnumerator FadeAudio(float fadeTime, AudioSource source, bool fadeIn, AudioClip newClip = null)
    {
        float startVolume = 0.0f;
        float endVolume = 1.0f;

        if (!fadeIn)
        {
            (startVolume, endVolume) = (endVolume, startVolume);
        }

        float currentTime = 0;

        if (fadeIn)
        {
            if (newClip != null) source.clip = newClip;
            source.Play();
        }

        while (currentTime < fadeTime)
        {
            source.volume = Mathf.Lerp(startVolume, endVolume, currentTime / fadeTime);
            currentTime += Time.deltaTime;

            yield return null;
        }

        source.volume = endVolume;

        if (!fadeIn) source.Pause();

        yield return null;
    }
}

[System.Serializable]
public enum SoundEffectType
{
    None,
    DoorOpen,
    ItemAcquired,
    PuzzleCompleted,
    MazeCompleted,
    InvalidAction,
    UI_BtnHover,
    UI_BtnPressed,
    UI_PanelMove
};

[System.Serializable]
public class SoundEffect
{
    public SoundEffectType type;
    public AudioClip clip;
}
