using UnityEngine;

[CreateAssetMenu(fileName = "RSO_GameMode", menuName = "Data/RSO/RSO_GameMode")]
public class RSO_GameMode : ScriptableObject
{
    public enum GameMode { Rythm, Pizza }
    public GameMode CurrentMode = GameMode.Rythm;
}