using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_InputReader", menuName = "Data/SSO/SSO_InputReader")]
public class SSO_InputReader : ScriptableObject
{
    public event Action<SSO_Ingredient> OnIngredientPressedEvent;

    public event Action OnOvenPressedEvent;
    public event Action OnServePressedEvent;
    public event Action OnSwitchPressedEvent;
    public event Action OnSpamPressedEvent;

    public void RaiseIngredientPressed(SSO_Ingredient ingredient)
    {
        OnIngredientPressedEvent?.Invoke(ingredient);
    }

    public void RaiseOvenPressed() => OnOvenPressedEvent?.Invoke();
    public void RaiseServePressed() => OnServePressedEvent?.Invoke();
    public void RaiseSwitchPressed() => OnSwitchPressedEvent?.Invoke();
    public void RaiseSpamPressed() => OnSpamPressedEvent?.Invoke();
}