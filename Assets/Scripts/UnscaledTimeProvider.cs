using UnityEngine;

public class UnscaledTimeProvider : MonoBehaviour
{
    private static readonly int UnscaledTimeID = Shader.PropertyToID("_UnscaledTime");

    void Update()
    {
        Shader.SetGlobalFloat(UnscaledTimeID, Time.unscaledTime);
    }
}