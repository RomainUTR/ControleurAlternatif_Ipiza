using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using System.Collections;
using RomainUTR.SLToolbox;

public class AudioManager : MonoBehaviour
{
    [BoxGroup("Settings")]
    [TitleGroup("Settings/Audio")]
    [SerializeField] private int startingAudioObjectsCount;
    [TitleGroup("Settings/Audio")]
    [SerializeField, Range(0f, 1f)] float transitionFadeOutDelay;
    [TitleGroup("Settings/Audio")]
    [SerializeField, Range(0f, 1f)] float transitionFadeInDelay;

    [BoxGroup("References")]
    [TitleGroup("References/AudioMixer"), Required]
    [SerializeField] private AudioMixerGroup musicMixerGroup, sfxMusicGroup, uiMusicGroup, masterMusicGroup;

    private Transform playlistParent = null;
    private Transform soundParent = null;
    private Transform uiParent = null;
    private readonly Queue<AudioSource> soundsQueue = new();
    private readonly Queue<AudioSource> uiSoundsQueue = new();
    private List<AudioSource> audios = new();
    List<AudioSource> playlistAudios = new();
    int initialMusicCount;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupAudioParent();
        for (int i = 0; i < startingAudioObjectsCount; i++)
        {
            soundsQueue.Enqueue(CreateAudioSource(soundParent));
        }

        for (int i = 0; i < 5; i++)
        {
            uiSoundsQueue.Enqueue(CreateUIAudioSource(uiParent));
        }
    }

    void SetupAudioParent()
    {
        playlistParent = new GameObject("[PLAYLIST]").transform;
        playlistParent.parent = transform;

        soundParent = new GameObject("[SOUNDS]").transform;
        soundParent.parent = transform;

        uiParent = new GameObject("[UISOUND]").transform;
        uiParent.parent = transform;
    }

    void ClearAllAudio()
    {
        foreach (AudioSource audio in audios)
        {
            audio.Stop();
        }
    }

    public void PlayClipAt(SoundData sound, Vector3 position)
    {
        AudioSource audioSource;

        if (soundsQueue.Count <= 0)
        {
            audioSource = CreateAudioSource(soundParent);
        }
        else
        {
            audioSource = soundsQueue.Dequeue();
        }

        audioSource.transform.position = position;
        audioSource.clip = sound.clips.GetRandom();
        audioSource.volume = Mathf.Clamp(sound.volumeMultiplier, 0f, 1f);
        audioSource.spatialBlend = sound.spatialBlend;

        audioSource.Play();
        StartCoroutine(AddAudioSourceToQueue(audioSource, soundsQueue));
    }

    public void PlayUISound(SoundData sound)
    {
        AudioSource audioSource;

        if (uiSoundsQueue.Count <= 0)
        {
            audioSource = CreateUIAudioSource(uiParent);
        }
        else
        {
            audioSource = uiSoundsQueue.Dequeue();
        }

        audioSource.clip = sound.clips.GetRandom();
        audioSource.volume = Mathf.Clamp(sound.volumeMultiplier, 0f, 1f);
        audioSource.spatialBlend = 0f;

        audioSource.Play();
        StartCoroutine(AddAudioSourceToQueue(audioSource, uiSoundsQueue));
    }


    private IEnumerator AddAudioSourceToQueue(AudioSource current, Queue<AudioSource> targetQueue)
    {
        float cooldown = current.clip.length;
        float timer = 0f;

        while (timer < cooldown)
        {
            yield return null;
            timer += Time.deltaTime;

            if (!current.isPlaying)
            {
                current.UnPause();
            }
        }

        targetQueue.Enqueue(current);
    }

    private AudioSource CreateAudioSource(Transform parent)
    {
        AudioSource audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
        audioSource.transform.SetParent(parent);
        audioSource.outputAudioMixerGroup = sfxMusicGroup;
        audios.Add(audioSource);
        return audioSource;
    }

    private AudioSource CreateUIAudioSource(Transform parent)
    {
        AudioSource audioSource = new GameObject("UIAudioSource").AddComponent<AudioSource>();
        audioSource.transform.SetParent(uiParent);
        audioSource.outputAudioMixerGroup = uiMusicGroup;
        audios.Add(audioSource);

        return audioSource;
    }

    AudioSource CreatePlaylistAudioSource()
    {
        AudioSource audioSource = new GameObject("Playlist").AddComponent<AudioSource>();
        audioSource.transform.SetParent(playlistParent);
        audioSource.outputAudioMixerGroup = musicMixerGroup;
        return audioSource;
    }

    public void ChangeAmbianceMusic(MusicTrack[] playlists)
    {
        StartCoroutine(ChangeAmbianceMusicDelay(playlists));
    }

    IEnumerator ChangeAmbianceMusicDelay(MusicTrack[] musicDatas)
    {
        if (playlistAudios.Count > 0 && musicDatas.Length > 0)
        {
            if (playlistAudios[0] != null && musicDatas[0].music != null)
            {
                if (playlistAudios[0].clip == musicDatas[0].music.clip)
                {
                    yield break;
                }
            }
        }

        List<AudioSource> sourcesToFadeOut = new List<AudioSource>(playlistAudios);
        playlistAudios.Clear();

        foreach (AudioSource oldSource in sourcesToFadeOut)
        {
            if (oldSource == null) continue;

            oldSource.DOFade(0, transitionFadeOutDelay)
                .SetLink(oldSource.gameObject)
                .OnComplete(() =>
                {
                    if (oldSource != null) Destroy(oldSource.gameObject);
                });
        }

        yield return new WaitForSeconds(transitionFadeOutDelay);

        foreach (MusicTrack track in musicDatas)
        {
            if (track.music == null || track.music.clip == null) continue;

            AudioSource newSource = CreatePlaylistAudioSource();
            newSource.clip = track.music.clip;
            newSource.loop = track.music.isLooping;
            newSource.volume = 0f;
            newSource.Play();

            float finalVolume = (track.music.volumMultiplier > 0 ? track.music.volumMultiplier : 1f) * track.localVolume;

            newSource.DOFade(finalVolume, transitionFadeInDelay)
                .SetLink(newSource.gameObject);

            playlistAudios.Add(newSource);
        }
    }

    public void StopAmbianceMusic()
    {
        ChangeAmbianceMusic(new MusicTrack[0]);
    }
}