using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "RSE_OnTurnCompleted", menuName = "Events/RSE_OnTurnCompleted")]
public class RSE_OnTurnCompleted : ScriptableObject
{
    public event UnityAction<int> OnEventRaised;

    public void Raise(int amount)
    {
        OnEventRaised?.Invoke(amount);
    }
}