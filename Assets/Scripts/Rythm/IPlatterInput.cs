using UnityEngine;

public interface IPlatterInput
{
    float CurrentInput { get; }
    float CurrentSpeed { get; }
    bool IsConsumed { get; }
    void ConsumeInput();
}
