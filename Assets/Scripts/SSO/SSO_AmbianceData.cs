using UnityEngine;

[CreateAssetMenu(fileName = "SSO_AmbianceData", menuName = "Data/SSO/SSO_AmbianceData")]
public class SSO_AmbianceData : ScriptableObject
{
    public float NoteHitReward = 0.5f;
    public float NoteMissPenalty = 2.0f;
    public float PizzaReward = 25.0f;
    public float PizzaPenalty = 15.0f;
}