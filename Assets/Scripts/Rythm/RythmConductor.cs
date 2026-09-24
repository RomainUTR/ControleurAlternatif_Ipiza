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
    [SerializeField] private RSE_RefreshUI RefreshUI;
    [SerializeField] private RSO_Score Score;
    [SerializeField] private SSO_ScoreData ScoreData;

    public float CurrentTrackTime => (float)_currentTrackTime;

    private AudioSource _audioSource;

    // On pr�f�re double pour la pr�cision des d�cimales
    private double _trackStartDspTime;
    private double _currentTrackTime;

    private bool _isPlaying = false;
    private float _bpm;
    private float _secondsPerBeat;

    private int _currentDataNoteIndex = 0;
    private float _nextBonusTargetTime = 0f;
    private float _pauseProceduralUntil = 0f;

    private List<NoteBehavior> _activeNotes = new List<NoteBehavior>();

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

            float blockDuration = nextNote.Duration > 0f ? nextNote.Duration : 0.05f;
            _pauseProceduralUntil = Mathf.Max(_pauseProceduralUntil, targetTimeInSeconds + blockDuration);

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
        if (_currentDataNoteIndex >= trackData.TrackNotes.Count) return;

        if (_nextBonusTargetTime <= _pauseProceduralUntil + 0.01f)
        {
            _nextBonusTargetTime += _secondsPerBeat;
            return;
        }

        if (_currentDataNoteIndex < trackData.TrackNotes.Count)
        {
            NoteEvent nextNote = trackData.TrackNotes[_currentDataNoteIndex];
            float nextNoteTime = trackData.FirstBeatOffset + (nextNote.TargetBeat * _secondsPerBeat);

            if (Mathf.Abs(nextNoteTime - _nextBonusTargetTime) <= 0.1f)
            {
                _nextBonusTargetTime += _secondsPerBeat;
                return;
            }
        }

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
        bool allNotesPlayed = _currentDataNoteIndex >= trackData.TrackNotes.Count;
        bool audioFinished = _currentTrackTime >= _audioSource.clip.length;

        if ((allNotesPlayed || audioFinished) && _isPlaying)
        {
            _isPlaying = false;

            if (_audioSource.isPlaying) _audioSource.Stop();

            foreach (NoteBehavior remainNote in _activeNotes)
            {
                if (remainNote != null) Destroy(remainNote.gameObject);
            }
            _activeNotes.Clear();
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

        _activeNotes.Add(noteScript);
    }

    private void CheckPlayerInput()
    {
        for (int i = 0; i < _activeNotes.Count; i++)
        {
            NoteBehavior note = _activeNotes[i];
            note.EvaluateInput(PlayerPlatter, trackData.DifficultyTolerance, (float)_currentTrackTime);

            if (note.CurrentState == NoteState.Hit)
            {
                //Debug.LogWarning("HIT !");
                PlayerPlatter.ConsumeInput();
                note.TryToScoring();

                Score.RuntimeMultiplier += ScoreData.MultiplierByComboUnit;
                Score.RuntimeCombo++;

                RefreshUI.Raise();

                Destroy(note.gameObject);
                _activeNotes.RemoveAt(i);

                i--;
            }
            else if (note.CurrentState == NoteState.Miss)
            {
                //Debug.LogError("MISS !");

                if (note.CurrentType != NoteType.Bonus)
                {
                    Score.RuntimeMultiplier = Score.InitialMultiplierValue;
                    Score.RuntimeCombo = Score.InitialComboValue;
                }

                RefreshUI.Raise();

                note.TriggerMissFeedback((float)CurrentTrackTime);
                _activeNotes.RemoveAt(i);

                i--;
            }
        }
    }
}