using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType
{
    Scratch,
    Bonus,
    Spam,
    Hold
}

[Serializable]
public struct NoteEvent
{
    public NoteType Type;

    [InfoBox("Numéro du temps musical (ex: 0, 1, 2, 3...)")]
    public float TargetBeat;

    public float Duration;

    [InfoBox("1 = Haut, -1 = Bas")]
    public float Direction;
    public int RequiredHits;
}

[CreateAssetMenu(fileName = "SSO_TrackData", menuName = "Data/SSO/SSO_TrackData")]
public class SSO_TrackData : ScriptableObject
{
    public AudioClip TrackAudio;
    public float BPM = 120f;
    public float FirstBeatOffset = 0f;
    public float DifficultyTolerance = 0.2f;
    public bool DivideBPM = false;

    [Header("Level Design"), InfoBox("Par ordre chronologique", InfoMessageType.Warning)]
    public List<NoteEvent> TrackNotes = new List<NoteEvent>();


    [Button("Générer la base (ATTENTION : Écrase la liste !)", ButtonSizes.Large)]
    [GUIColor(1f, 0.4f, 0.4f)]
    private void GenerateBaseTrack()
    {
        if (TrackAudio == null)
        {
            Debug.LogError("Impossible de générer : Ajoute d'abord un AudioClip !");
            return;
        }

        TrackNotes.Clear();

        float clipLength = TrackAudio.length;
        float actualBpm = DivideBPM ? BPM / 2f : BPM;
        float secondsPerBeat = 60f / actualBpm;

        float currentTime = FirstBeatOffset;
        int currentBeatIndex = 0;

        while (currentTime < clipLength)
        {
            NoteEvent baseNote = new NoteEvent
            {
                Type = NoteType.Scratch,
                TargetBeat = currentBeatIndex,
                Direction = (currentBeatIndex % 2 == 0) ? 1f : -1f
            };

            TrackNotes.Add(baseNote);

            currentBeatIndex++;
            currentTime += secondsPerBeat;
        }

        Debug.Log($"Génération terminée : {TrackNotes.Count} notes créées !");
    }
}