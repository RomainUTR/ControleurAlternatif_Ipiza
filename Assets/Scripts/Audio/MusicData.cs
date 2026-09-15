using UnityEngine;

[CreateAssetMenu(fileName = "NewMusicData", menuName = "Audio/Music Data")]
public class MusicData : ScriptableObject
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volumMultiplier;
    public bool isLooping;
}