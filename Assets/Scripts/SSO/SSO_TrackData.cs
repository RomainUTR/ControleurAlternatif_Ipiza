using UnityEngine;

[CreateAssetMenu(fileName = "SSO_TrackData", menuName = "Data/SSO/SSO_TrackData")]
public class SSO_TrackData : ScriptableObject
{
    public AudioClip TrackAudio;
    public float BPM = 120f;

    public float FirstBeatOffset = 0f;

    public float DifficultyTolerance = 0.2f;

    public bool DivideBPM = false;
}