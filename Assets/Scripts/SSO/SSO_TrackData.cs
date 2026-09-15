using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialNoteType
{
    Spam,
    Hold
}

[Serializable]
public struct SpecialEvent
{
    public SpecialNoteType Type;
    public float Timecode;
    public float Duration;
}

[CreateAssetMenu(fileName = "SSO_TrackData", menuName = "Data/SSO/SSO_TrackData")]
public class SSO_TrackData : ScriptableObject
{
    public AudioClip TrackAudio;
    public float BPM = 120f;

    public float FirstBeatOffset = 0f;

    public float DifficultyTolerance = 0.2f;

    public bool DivideBPM = false;

    [Header("Special Level Design"), InfoBox("Par ordre chronologique", InfoMessageType.Warning)]
    public List<SpecialEvent> SpecialEvents;
}