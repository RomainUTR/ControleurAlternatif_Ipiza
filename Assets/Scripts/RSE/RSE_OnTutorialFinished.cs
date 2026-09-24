using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "RSE_OnTutorialFinished", menuName = "Events/RSE_OnTutorialFinished")]
public class RSE_OnTutorialFinished : ScriptableObject
{
    public event UnityAction OnEventRaised;

    public void Raise()
    {
        OnEventRaised?.Invoke();
    }
}