using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "RSE_RequestScoring", menuName = "Events/RSE_RequestScoring")]
public class RSE_RequestScoring : ScriptableObject
{
    public event UnityAction<int> OnEventRaised;

    public void Raise(int amount)
    {
        OnEventRaised?.Invoke(amount);
    }
}