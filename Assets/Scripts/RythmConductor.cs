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
    [SerializeField] private GameObject HoldPrefab;
    [SerializeField] private GameObject SpamPrefab;

    public float CurrentTrackTime => (float)_currentTrackTime;

    private AudioSource _audioSource;

    // On préfère double pour la précision des décimales
    private double _trackStartDspTime;
    private double _currentTrackTime;

    private bool _isPlaying = false;
    private float _bpm;
    private float _secondsPerBeat;

    private int _currentDataNoteIndex = 0;
    private float _nextBonusTargetTime = 0f;

    private Queue<NoteBehavior> _activeNotesQueue = new Queue<NoteBehavior>();

    private void Start()
    {
        if (trackData == null || trackData.TrackAudio == null) return;

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = trackData.TrackAudio;
        _bpm = trackData.DivideBPM ? trackData.BPM / 2f : trackData.BPM;
        _secondsPerBeat = 60f / _bpm;

        _nextBonusTargetTime = trackData.FirstBeatOffset + (_secondsPerBeat / 2f);

        _trackStartDspTime = AudioSettings.dspTime + lookAheadTime;
        _audioSource.PlayScheduled(_trackStartDspTime);

        _isPlaying = true;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _currentTrackTime = AudioSettings.dspTime - _trackStartDspTime;

        ProcessDataNotes();
        ProcessProceduralBonuses();
        CheckPlayerInput();
        CheckTrackEnd();
    }

    private void ProcessDataNotes()
    {
        while (_currentDataNoteIndex < trackData.TrackNotes.Count)
        {
            NoteEvent nextNote = trackData.TrackNotes[_currentDataNoteIndex];
            float targetTimeInSeconds = trackData.FirstBeatOffset + (nextNote.TargetBeat * _secondsPerBeat);
            float spawnTime = targetTimeInSeconds - lookAheadTime;

            if ((float)_currentTrackTime < spawnTime) break;

            switch (nextNote.Type)
            {
                case NoteType.Scratch:
                    SpawnNote(ScratchPrefab, targetTimeInSeconds, nextNote.Direction);
                    break;
                case NoteType.Bonus:
                    SpawnNote(BonusPrefab, targetTimeInSeconds, 0f);
                    break;
                case NoteType.Hold:
                    SpawnNote(HoldPrefab, targetTimeInSeconds, nextNote.Direction, nextNote.Duration);
                    break;
                case NoteType.Spam:
                    SpawnNote(SpamPrefab, targetTimeInSeconds, nextNote.Direction, nextNote.Duration, nextNote.RequiredHits);
                    break;
            }

            _currentDataNoteIndex++;
        }
    }

    private void ProcessProceduralBonuses()
    {
        if ((float)_currentTrackTime >= _nextBonusTargetTime - lookAheadTime)
        {
            if (Random.value < BonusSpawnRate)
            {
                SpawnNote(BonusPrefab, _nextBonusTargetTime, 0f);
            }

            _nextBonusTargetTime += _secondsPerBeat;
        }
    }

    private void CheckTrackEnd()
    {
        if (_currentTrackTime >= _audioSource.clip.length && _isPlaying)
        {
            _isPlaying = false;

            while (_activeNotesQueue.Count > 0)
            {
                NoteBehavior remainNote = _activeNotesQueue.Dequeue();
                if (remainNote != null) Destroy(remainNote.gameObject);
            }
            Debug.Log("Fin de piste");
        }
    }

    private void SpawnNote(GameObject prefabToSpawn, float targetTime, float direction, float duration = 0f, int requiredHits = 0)
    {
        GameObject newNote = Instantiate(prefabToSpawn, transform.position, Quaternion.identity, NotesParent);

        NoteBehavior noteScript = newNote.GetComponent<NoteBehavior>();
        if (noteScript != null)
        {
            float spawnTime = targetTime - lookAheadTime;
            noteScript.Initialize(transform.position, ValidationZone.position, spawnTime, targetTime, direction, this, duration, requiredHits);
        }

        _activeNotesQueue.Enqueue(noteScript);
    }

    private void CheckPlayerInput()
    {
        if (_activeNotesQueue.Count == 0) return;

        NoteBehavior currentNote = _activeNotesQueue.Peek();

        currentNote.EvaluateInput(PlayerPlatter, trackData.DifficultyTolerance, (float)_currentTrackTime);

        switch (currentNote.CurrentState)
        {
            case NoteState.Hit:
                NoteBehavior hitNote = _activeNotesQueue.Dequeue();
                Destroy(hitNote.gameObject);
                Debug.LogWarning("HIT !");
                break;

            case NoteState.Miss:
                NoteBehavior missedNote = _activeNotesQueue.Dequeue();
                currentNote.TriggerMissFeedback((float)CurrentTrackTime);
                Debug.LogError("MISS !");
                break;

            case NoteState.Ongoing:
                break;
        }
    }
}