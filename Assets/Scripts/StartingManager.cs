using RomainUTR.SLToolbox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartingManager : MonoBehaviour
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] private SceneReference MainScene;
    
    [Header("Input")]
    [SerializeField] private InputActionReference StartInput;
    //[Header("Output")]

    private void OnEnable()
    {
        if (StartInput != null)
        {
            StartInput.action.Enable();
            StartInput.action.performed += HandleStartingGame;
        }
    }

    private void OnDisable()
    {
        if (StartInput != null)
        {
            StartInput.action.performed -= HandleStartingGame;
            StartInput.action.Disable();
        }
    }

    private void HandleStartingGame(InputAction.CallbackContext ctx)
    {
        SceneManager.LoadScene(MainScene);
    }
}