using Sirenix.OdinInspector;
using UnityEngine;

public class KeyboardPlatterController : MonoBehaviour, IPlatterInput
{
    [Header("Settings")]
    [SerializeField] private float Acceleration = 3f;
    [SerializeField] private float Friction = 2f;
    [SerializeField] private float MaxSpeed = 1f;

    //[Header("References")]
    //[Header("Input")]
    //[Header("Output")]

    [ReadOnly, ShowInInspector]
    public float CurrentSpeed { get; private set; } = 0f;

    public bool IsConsumed {  get; private set; }
    public float CurrentInput {  get; private set; }

    private float _lastInput = 0f;

    private void Update()
    {
        CurrentInput = Input.GetAxisRaw("Vertical") * -1f;

        if (CurrentInput != _lastInput)
        {
            IsConsumed = false;

            if (CurrentInput != 0f && Mathf.Sign(CurrentInput) != Mathf.Sign(CurrentSpeed))
            {
                CurrentSpeed = 0f;
            }
        }

        _lastInput = CurrentInput;

        if (CurrentInput != 0f)
        {
            CurrentSpeed += CurrentInput * Acceleration * Time.deltaTime;
        }
        else
        {
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, Friction * Time.deltaTime);
        }

        CurrentSpeed = Mathf.Clamp(CurrentSpeed, -MaxSpeed, MaxSpeed);
    }

    public void ConsumeInput()
    {
        IsConsumed = true;
    }
}