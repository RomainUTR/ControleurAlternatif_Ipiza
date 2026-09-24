using UnityEngine;
using UnityEngine.InputSystem;

public class GameModeSwitcher : MonoBehaviour
{
    [SerializeField] private RSO_GameMode GameModeData;
    [SerializeField] private InputActionReference SwitchInput;

    [Header("References")]
    [SerializeField] private GameObject RTPizza;
    [SerializeField] private GameObject RTRythm;

    private void OnEnable() => SwitchInput.action.Enable();
    private void OnDisable() => SwitchInput.action.Disable();

    private void Start()
    {
        SwitchToRythmMode();
    }

    private void Update()
    {
        if (SwitchInput.action.WasPressedThisFrame())
        {
            SwitchGameMode();
        } 
    }

    void SwitchGameMode()
    {
        if (GameModeData.CurrentMode == RSO_GameMode.GameMode.Pizza)
        {
            SwitchToRythmMode();
        } else
        {
            SwitchToPizzaMode();
        }

        Debug.Log($"Mode switché sur : {GameModeData.CurrentMode}");
    }

    void SwitchToPizzaMode()
    {
        GameModeData.CurrentMode = RSO_GameMode.GameMode.Pizza;

        RTRythm.SetActive(false);
        RTPizza.SetActive(true);
    }

    void SwitchToRythmMode()
    {
        GameModeData.CurrentMode = RSO_GameMode.GameMode.Rythm;

        RTPizza.SetActive(false);
        RTRythm.SetActive(true);
    }
}