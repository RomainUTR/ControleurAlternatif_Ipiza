using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class RythmConductor : MonoBehaviour
{
    [Header("Settings")]
    public float lookAheadTime = 2f;
    public GameObject notePrefab;

    [Header("References")]
    [SerializeField] private SSO_TrackData trackData;
    [SerializeField] private KeyboardPlatterController PlayerPlatter;

    private AudioSource _audioSource;
    private double _trackStartDspTime;
    private double _currentTrackTime;
    private bool _isPlaying = false;

    private float _secondsPerBeat;
    private int _nextNoteIndex = 0;

    private Queue<float> _activeNotesQueue = new Queue<float>();

    private void Start()
    {
        if (trackData == null || trackData.TrackAudio == null) return;

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = trackData.TrackAudio;

        _secondsPerBeat = 60f / trackData.BPM;

        _trackStartDspTime = AudioSettings.dspTime;
        _audioSource.Play();
        _isPlaying = true;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _currentTrackTime = AudioSettings.dspTime - _trackStartDspTime;

        float nextNoteTargetTime = trackData.FirstBeatOffset + (_nextNoteIndex * _secondsPerBeat);
        float nextNoteSpawnTime = nextNoteTargetTime - lookAheadTime;

        if ((float)_currentTrackTime >= nextNoteSpawnTime)
        {
            SpawnNote(nextNoteTargetTime);
            _nextNoteIndex++;
        }

        CheckPlayerInput();
    }

    private void SpawnNote(float targetTime)
    {
        GameObject newNote = Instantiate(notePrefab, transform.position, Quaternion.identity);

        _activeNotesQueue.Enqueue(targetTime);
    }

    private void CheckPlayerInput()
    {
        if (_activeNotesQueue.Count == 0) return;

        float nextExpectedHitTime = _activeNotesQueue.Peek();
        float timeDifference = (float)_currentTrackTime - nextExpectedHitTime;

        if (timeDifference > trackData.DifficultyTolerance)
        {
            _activeNotesQueue.Dequeue();
            Debug.LogError("MISS ! Trop tard, la note est passée.");
            return;
        }

        if (Mathf.Abs(timeDifference) <= trackData.DifficultyTolerance)
        {
            float speedDifference = Mathf.Abs(1f - PlayerPlatter.CurrentSpeed);

            if (speedDifference <= trackData.DifficultyTolerance)
            {
                _activeNotesQueue.Dequeue();
                Debug.LogWarning("HIT ! Scratch parfait dans le bon tempo !");
            }
        }
    }
}