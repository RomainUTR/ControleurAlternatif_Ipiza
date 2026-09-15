using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[RequireComponent(typeof(AudioSource))]
public class RythmConductor : MonoBehaviour
{
    [Header("Settings")]
    public float lookAheadTime = 2f;
    public GameObject notePrefab;

    [Header("References")]
    [SerializeField, InlineEditor] private SSO_TrackData trackData;
    [SerializeField] private KeyboardPlatterController PlayerPlatter;
    [SerializeField] private Transform ValidationZone;
    [SerializeField] private Transform NotesParent;

    public float CurrentTrackTime => (float)_currentTrackTime;

    private AudioSource _audioSource;
    private double _trackStartDspTime;
    private double _currentTrackTime;
    private bool _isPlaying = false;

    [ReadOnly] public float _bpm;
    private float _secondsPerBeat;
    private int _nextNoteIndex = 0;

    private Queue<NoteBehavior> _activeNotesQueue = new Queue<NoteBehavior>();

    private void Start()
    {
        if (trackData == null || trackData.TrackAudio == null) return;

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = trackData.TrackAudio;

        _bpm = trackData.DivideBPM ? trackData.BPM / 2 : trackData.BPM;

        _secondsPerBeat = 60f / _bpm;

        _trackStartDspTime = AudioSettings.dspTime + lookAheadTime;
        _audioSource.PlayScheduled(_trackStartDspTime);

        _isPlaying = true;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _currentTrackTime = AudioSettings.dspTime - _trackStartDspTime;

        float nextNoteTargetTime = trackData.FirstBeatOffset + (_nextNoteIndex * _secondsPerBeat);
        float nextNoteSpawnTime = nextNoteTargetTime - lookAheadTime;

        while ((float)_currentTrackTime >= nextNoteSpawnTime && nextNoteSpawnTime <= _audioSource.clip.length)
        {
            SpawnNote(nextNoteTargetTime);
            _nextNoteIndex++;

            nextNoteTargetTime = trackData.FirstBeatOffset + (_nextNoteIndex * _secondsPerBeat);
            nextNoteSpawnTime = nextNoteTargetTime - lookAheadTime;
        }

        CheckPlayerInput();

        if (!_audioSource.isPlaying && _currentTrackTime > 0)
        {
            _isPlaying = false;

            while(_activeNotesQueue.Count > 0)
            {
                NoteBehavior remainNote = _activeNotesQueue.Dequeue();

                if (remainNote != null)
                {
                    Destroy(remainNote.gameObject);
                }
            }

            Debug.Log("Fin de piste");
        }
    }

    private void SpawnNote(float targetTime)
    {
        GameObject newNote = Instantiate(notePrefab, transform.position, Quaternion.identity, NotesParent);

        NoteBehavior noteScript = newNote.GetComponent<NoteBehavior>();
        if (noteScript != null)
        {
            float spawnTime = targetTime - lookAheadTime;
            float scratchDirection = (_nextNoteIndex % 2 == 0) ? 1f : -1f;
            noteScript.Initialize(transform.position, ValidationZone.position, spawnTime, targetTime, scratchDirection, this);
        }

        _activeNotesQueue.Enqueue(noteScript);
    }

    private void CheckPlayerInput()
    {
        if (_activeNotesQueue.Count == 0) return;

        NoteBehavior currentNote = _activeNotesQueue.Peek();
        float timeDifference = (float)_currentTrackTime - currentNote.TargetTime;

        if (timeDifference > trackData.DifficultyTolerance)
        {
            NoteBehavior missedNote = _activeNotesQueue.Dequeue();
            Destroy(missedNote.gameObject);
            Debug.LogError("MISS !");
            return;
        }

        if (Mathf.Abs(timeDifference) <= trackData.DifficultyTolerance)
        {
            if (Mathf.Abs(PlayerPlatter.CurrentSpeed) > 0.1f && Mathf.Sign(PlayerPlatter.CurrentSpeed) == Mathf.Sign(currentNote.Direction))
            {
                float speedDifference = Mathf.Abs(1f - Mathf.Abs(PlayerPlatter.CurrentSpeed));

                if (speedDifference <= trackData.DifficultyTolerance)
                {
                    NoteBehavior hitNote = _activeNotesQueue.Dequeue();
                    Destroy(hitNote.gameObject);
                    Debug.LogWarning($"HIT ! Scratch {(currentNote.Direction > 0 ? "HAUT" : "BAS")} parfait !");
                }
            }
        }
    }
}