using UnityEngine;
using UnityEngine.InputSystem;

public class GameModeSwitcher : MonoBehaviour
{
    [SerializeField] private RSO_GameMode GameModeData;
    [SerializeField] private InputActionReference SwitchInput;

    [Header("References")]
    [SerializeField] private GameObject RTPizza;

    private void OnEnable() => SwitchInput.action.Enable();
    private void OnDisable() => SwitchInput.action.Disable();

    private void Update()
    {
        if (SwitchInput.action.WasPressedThisFrame())
        {
            GameModeData.CurrentMode = (GameModeData.CurrentMode == RSO_GameMode.GameMode.Rythm)
                ? RSO_GameMode.GameMode.Pizza
                : RSO_GameMode.GameMode.Rythm;

            Debug.Log($"Mode switché sur : {GameModeData.CurrentMode}");
        } 
    }

    
}