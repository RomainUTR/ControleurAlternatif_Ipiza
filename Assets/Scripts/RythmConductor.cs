using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using RomainUTR.SLToolbox;

[RequireComponent(typeof(AudioSource))]
public class RythmConductor : MonoBehaviour
{
    [Header("Settings")]
    public float lookAheadTime = 2f;
    [SerializeField, Range(0f, 1f)] private float BonusSpawnRate;

    [Header("References")]
    [SerializeField, InlineEditor] private SSO_TrackData trackData;
    [SerializeField] private KeyboardPlatterController PlayerPlatter;
    [SerializeField] private Transform ValidationZone;
    [SerializeField] private Transform NotesParent;
    [SerializeField] private GameObject ScratchPrefab;
    [SerializeField] private GameObject BonusPrefab;
    // public SoundData SFXTest;

    public float CurrentTrackTime => (float)_currentTrackTime;

    private AudioSource _audioSource;
    private double _trackStartDspTime;
    private double _currentTrackTime;
    private bool _isPlaying = false;

    [ReadOnly] public float _bpm;
    private float _secondsPerBeat;
    private int _nextNoteIndex = 0;

    private Queue<NoteBehavior> _activeNotesQueue = new Queue<NoteBehavior>();
    private Queue<SpecialEvent> _specialEventsQueue = new Queue<SpecialEvent>();
    private float _pauseProceduralUntil = 0f;

    private void Start()
    {
        if (trackData == null || trackData.TrackAudio == null) return;

        if (trackData.SpecialEvents != null)
        {
            foreach (var specialEvent in trackData.SpecialEvents)
            {
                _specialEventsQueue.Enqueue(specialEvent);
            }
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = trackData.TrackAudio;

        _bpm = trackData.DivideBPM ? trackData.BPM / 2 : trackData.BPM;

        _secondsPerBeat = (60f / _bpm) / 2f;

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

        if (_specialEventsQueue.Count > 0)
        {
            SpecialEvent nextEvent = _specialEventsQueue.Peek();
            float eventSpawnTime = nextEvent.Timecode - lookAheadTime;

            if ((float)_currentTrackTime >= eventSpawnTime)
            {
                _pauseProceduralUntil = nextEvent.Timecode + nextEvent.Duration;
                Debug.LogWarning($"Apparition d'une note spéciale : {nextEvent.Type} ! Pause du procédural jusqu'à {_pauseProceduralUntil}s.");
                _specialEventsQueue.Dequeue();
            }
        }

        while ((float)_currentTrackTime > _pauseProceduralUntil && (float)_currentTrackTime >= nextNoteSpawnTime && nextNoteSpawnTime <= _audioSource.clip.length)
        {
            Debug.Log(nextNoteSpawnTime);

            if (_nextNoteIndex % 2 == 0)
            {
                float scratchDirection = ((_nextNoteIndex / 2) % 2 == 0) ? 1f : -1f;
                SpawnNote(ScratchPrefab, nextNoteTargetTime, scratchDirection);
            }
            else
            {
                if (Random.value < BonusSpawnRate)
                {
                    SpawnNote(BonusPrefab, nextNoteTargetTime, 0f);
                }
            }

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

    private void SpawnNote(GameObject prefabToSpawn, float targetTime, float direction)
    {
        GameObject newNote = Instantiate(prefabToSpawn, transform.position, Quaternion.identity, NotesParent);

        NoteBehavior noteScript = newNote.GetComponent<NoteBehavior>();
        if (noteScript != null)
        {
            float spawnTime = targetTime - lookAheadTime;
            noteScript.Initialize(transform.position, ValidationZone.position, spawnTime, targetTime, direction, this);
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
            // AudioManager.Instance.PlayClipAt(SFXTest, transform.position);
            Destroy(missedNote.gameObject);
            Debug.LogError("MISS !");
            return;
        }

        if (Mathf.Abs(timeDifference) <= trackData.DifficultyTolerance)
        {
            if (currentNote.EvaluateInput(PlayerPlatter, trackData.DifficultyTolerance))
            {
                NoteBehavior hitNote = _activeNotesQueue.Dequeue();
                Destroy(hitNote.gameObject);
                Debug.LogWarning("HIT ! Action validée !");
            }
        }
    }
}