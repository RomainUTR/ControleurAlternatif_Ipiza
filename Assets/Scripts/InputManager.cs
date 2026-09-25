using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [System.Serializable]
    public struct IngredientMapping
    {
        public InputActionReference InputAction;
        public SSO_Ingredient Ingredient;
    }

    [Header("Broadcaster Channel")]
    [SerializeField] private SSO_InputReader InputReader;

    [Header("Ingredient Mapping")]
    [SerializeField] private List<IngredientMapping> IngredientInputs = new List<IngredientMapping>();

    [Header("Global Actions")]
    [SerializeField] private InputActionReference OvenInput;
    [SerializeField] private InputActionReference ServeInput;
    [SerializeField] private InputActionReference SwitchInput;
    [SerializeField] private InputActionReference SpamInput;

    private void OnEnable()
    {
        if (OvenInput != null)
        {
            OvenInput.action.Enable();
            OvenInput.action.performed += OnOvenPerformed;
            OvenInput.action.canceled += OnOvenPerformed;
        }
        if (ServeInput != null) { ServeInput.action.Enable(); ServeInput.action.performed += OnServePerformed; }
        if (SwitchInput != null) { SwitchInput.action.Enable(); SwitchInput.action.performed += OnSwitchPerformed; }
        if (SpamInput != null) { SpamInput.action.Enable(); SpamInput.action.performed += OnSpamPerformed; }

        foreach (var mapping in IngredientInputs)
        {
            if (mapping.InputAction != null)
            {
                mapping.InputAction.action.Enable();
                mapping.InputAction.action.performed += ctx => InputReader.RaiseIngredientPressed(mapping.Ingredient);
            }
        }
    }

    private void OnDisable()
    {
        if (OvenInput != null)
        {
            OvenInput.action.Disable();
            OvenInput.action.performed -= OnOvenPerformed;
            OvenInput.action.canceled -= OnOvenPerformed;
        }
        if (ServeInput != null) { ServeInput.action.Disable(); ServeInput.action.performed -= OnServePerformed; }
        if (SwitchInput != null) { SwitchInput.action.Disable(); SwitchInput.action.performed -= OnSwitchPerformed; }
        if (SpamInput != null) { SpamInput.action.Disable(); SpamInput.action.performed -= OnSpamPerformed; }

        foreach (var mapping in IngredientInputs)
        {
            if (mapping.InputAction != null)
            {
                mapping.InputAction.action.Disable();
                mapping.InputAction.action.performed -= ctx => InputReader.RaiseIngredientPressed(mapping.Ingredient);
            }
        }
    }

    public void OnOvenPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            InputReader.RaiseOvenPressed();
        }

        if (ctx.canceled)
        {
            InputReader.RaiseOvenReleased();
        }
    }
    private void OnServePerformed(InputAction.CallbackContext ctx) => InputReader.RaiseServePressed();
    private void OnSwitchPerformed(InputAction.CallbackContext ctx) => InputReader.RaiseSwitchPressed();
    private void OnSpamPerformed(InputAction.CallbackContext ctx) => InputReader.RaiseSpamPressed();
}