using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class SceneMusic : MonoBehaviour
{
    [ListDrawerSettings(ShowIndexLabels = false)]
    [SerializeField] private MusicTrack[] playlist;

    [Title("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopPlaylist = true;

    private Coroutine _playlistCoroutine;

    private void Start()
    {
        if (playOnStart && AudioManager.Instance != null)
        {
            TriggerMusic();
        }
    }

    public void TriggerMusic()
    {
        if (AudioManager.Instance != null && playlist.Length > 0)
        {
            if (_playlistCoroutine != null)
            {
                StopCoroutine(_playlistCoroutine);
            }

            _playlistCoroutine = StartCoroutine(PlayPlaylistSequentially());
        }
    }

    private IEnumerator PlayPlaylistSequentially()
    {
        int currentIndex = 0;

        while (true)
        {
            MusicTrack currentTrack = playlist[currentIndex];

            AudioManager.Instance.ChangeAmbianceMusic(new MusicTrack[] { currentTrack });

            yield return new WaitForSeconds(currentTrack.music.clip.length);

            currentIndex++;

            if (currentIndex >= playlist.Length)
            {
                if (loopPlaylist)
                {
                    currentIndex = 0;
                }
                else
                {
                    break;
                }
            }
        }
    }
}