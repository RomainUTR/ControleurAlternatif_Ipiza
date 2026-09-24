using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameModeSwitcher : MonoBehaviour
{
    [SerializeField] private RSO_GameMode GameModeData;
    [SerializeField] private InputActionReference SwitchInput;

    [Header("References")]
    [SerializeField] private GameObject RTPizza;
    [SerializeField] private GameObject RTRythm;
    [SerializeField] private GameObject CommandsPanel;
    [SerializeField] private Slider AutoSwitchSlider;
    [SerializeField] private RSE_OnTutorialFinished OnTutorialFinished;

    [Header("Auto-Switcher")]
    [SerializeField, SuffixLabel("sec")] private float MinSwitchInterval;
    [SerializeField, SuffixLabel("sec")] private float MaxSwitchInterval;
    [ShowInInspector] public float SwitchProgress => _timer / _currentSwitchTarget;

    private float _currentSwitchTarget = 15f;
    private float _timer = 0f;

    private void OnEnable()
    {
        SwitchInput.action.Enable();
        OnTutorialFinished.OnEventRaised += HandleForcedSwitch;
    }
    private void OnDisable()
    {
        SwitchInput.action.Disable();
        OnTutorialFinished.OnEventRaised -= HandleForcedSwitch;
    }

    private void Start()
    {
        SwitchToRythmMode();
        SetNewRandomInterval();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (AutoSwitchSlider != null)
        {
            AutoSwitchSlider.value = SwitchProgress;
        }

        if (_timer >= _currentSwitchTarget)
        {
            SwitchGameMode();
            SetNewRandomInterval();
        }

        if (SwitchInput.action.WasPressedThisFrame())
        {
            SwitchGameMode();
            SetNewRandomInterval();
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
        CommandsPanel.SetActive(true);
    }

    void SwitchToRythmMode()
    {
        GameModeData.CurrentMode = RSO_GameMode.GameMode.Rythm;

        CommandsPanel.SetActive(false);
        RTPizza.SetActive(false);
        RTRythm.SetActive(true);
    }

    private void SetNewRandomInterval()
    {
        _currentSwitchTarget = Random.Range(MinSwitchInterval, MaxSwitchInterval);
        _timer = 0f;
    }

    private void HandleForcedSwitch()
    {
        Debug.Log("Tuto terminé, on passe de force à la pizza");
        SwitchGameMode();
        SetNewRandomInterval();
    }
}