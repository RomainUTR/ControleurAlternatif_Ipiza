using Sirenix.OdinInspector;
using UnityEngine;

public class KeyboardPlatterController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float Acceleration = 3f;
    [SerializeField] private float Friction = 2f;
    [SerializeField] private float MaxSpeed = 1f;

    //[Header("References")]
    //[Header("Input")]
    //[Header("Output")]

    [ReadOnly]
    public float CurrentSpeed = 0f;

    public bool IsConsumed {  get; private set; }

    private float _lastInput = 0f;

    private void Update()
    {
        float input = Input.GetAxisRaw("Vertical");

        if (input != _lastInput)
        {
            IsConsumed = false;

            if (input != 0f && Mathf.Sign(input) != Mathf.Sign(CurrentSpeed))
            {
                CurrentSpeed = 0f;
            }
        }

        _lastInput = input;

        if (input != 0f)
        {
            CurrentSpeed += input * Acceleration * Time.deltaTime;
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