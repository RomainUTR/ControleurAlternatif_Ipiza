using UnityEngine;
using Sirenix.OdinInspector;

public class GameSpeedDebugger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("La vitesse quand on maintient la touche d'accélération (ex: 3x plus vite)")]
    [SerializeField] private float FastForwardMultiplier = 3f;
    [Tooltip("La touche pour accélérer le temps")]
    [SerializeField] private KeyCode FastForwardKey = KeyCode.Tab;

    [Header("References")]
    [Tooltip("L'AudioSource principale qui joue la musique du jeu (il est dans RythmConductor)")]
    [SerializeField] private AudioSource MusicSource;

    private float _originalTimeScale;
    private float _originalAudioPitch;
    private bool _isFastForwarding = false;

    private void Start()
    {
        _originalTimeScale = Time.timeScale;
        if (MusicSource != null)
        {
            _originalAudioPitch = MusicSource.pitch;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f && !_isFastForwarding) return;

        if (Input.GetKeyDown(FastForwardKey))
        {
            StartFastForward();
        }
        else if (Input.GetKeyUp(FastForwardKey))
        {
            StopFastForward();
        }
    }

    [Button("Force Fast Forward")]
    private void StartFastForward()
    {
        _isFastForwarding = true;
        Time.timeScale = _originalTimeScale * FastForwardMultiplier;

        if (MusicSource != null)
        {
            MusicSource.pitch = _originalAudioPitch * FastForwardMultiplier;
        }
    }

    [Button("Force Normal Speed")]
    private void StopFastForward()
    {
        _isFastForwarding = false;
        Time.timeScale = _originalTimeScale;

        if (MusicSource != null)
        {
            MusicSource.pitch = _originalAudioPitch;
        }
    }
}