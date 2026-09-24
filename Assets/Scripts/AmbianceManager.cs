using UnityEngine;
using UnityEngine.UI;

public class AmbianceManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private SSO_AmbianceData AmbianceData;
    [SerializeField] private float LerpSpeed = 5f;

    [Header("Références Visuelles")]
    [SerializeField] private SpriteRenderer JaugeSprite;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private Gradient AmbianceGradient;

    private float _targetAmbiance = 50f;
    private Material _jaugeMat;

    private void OnEnable()
    {
        GameEvents.OnNoteHit += HandleNoteHit;
        GameEvents.OnNoteMiss += HandleNoteMiss;
        GameEvents.OnPizzaDelivered += HandlePizzaDelivered;
        GameEvents.OnPizzaFailed += HandlePizzaFailed;
    }

    private void OnDisable()
    {
        GameEvents.OnNoteHit -= HandleNoteHit;
        GameEvents.OnNoteMiss -= HandleNoteMiss;
        GameEvents.OnPizzaDelivered -= HandlePizzaDelivered;
        GameEvents.OnPizzaFailed -= HandlePizzaFailed;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        GameOverPanel.SetActive(false);

        if (JaugeSprite != null)
        {
            _jaugeMat = JaugeSprite.material;
        }
    }

    private void HandleNoteHit() => ModifyAmbiance(AmbianceData.NoteHitReward);
    private void HandleNoteMiss() => ModifyAmbiance(-AmbianceData.NoteMissPenalty);
    private void HandlePizzaDelivered() => ModifyAmbiance(AmbianceData.PizzaReward);
    private void HandlePizzaFailed() => ModifyAmbiance(-AmbianceData.PizzaPenalty);

    private void Update()
    {
        float targetFill = _targetAmbiance / 100f;

        if (JaugeSprite != null)
        {
            JaugeSprite.color = AmbianceGradient.Evaluate(targetFill);
        }

        if (_jaugeMat != null && _jaugeMat.HasProperty("_Fill"))
        {
            float currentFill = _jaugeMat.GetFloat("_Fill");
            float smoothFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * LerpSpeed);
            _jaugeMat.SetFloat("_Fill", smoothFill);
        }
    }

    private void ModifyAmbiance(float amount)
    {
        _targetAmbiance = Mathf.Clamp(_targetAmbiance + amount, 0f, 100f);

        if (_targetAmbiance <= 0f && !GameOverPanel.activeSelf)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}