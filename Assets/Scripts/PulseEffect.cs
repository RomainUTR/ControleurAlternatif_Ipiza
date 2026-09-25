using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float amplitude = 0.15f;

    private Vector3 _startScale;

    void Start()
    {
        _startScale = transform.localScale;
    }

    void Update()
    {
        float variation = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localScale = _startScale + new Vector3(variation, variation, variation);
    }
}