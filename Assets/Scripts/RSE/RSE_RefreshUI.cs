using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "RSE_RefreshUI", menuName = "Events/RSE_RefreshUI")]
public class RSE_RefreshUI : ScriptableObject
{
    public event UnityAction OnEventRaised;

    public void Raise()
    {
        OnEventRaised?.Invoke();
    }
}