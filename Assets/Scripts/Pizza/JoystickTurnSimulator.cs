using UnityEngine;

public class JoystickTurnSimulator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float DeadZone = 0.5f;
    [SerializeField] private string HorizontalAxis = "Horizontal";
    [SerializeField] private string VerticalAxis = "Vertical";
    [SerializeField] private float OffsetAngle = -90f;

    [Header("References")]
    [SerializeField] private Transform PizzaTransform;
    [SerializeField] private RSE_OnTurnCompleted OnTurnCompleted;

    private float _previousAngle = 0f;
    private float _accumulatedDegrees = 0f;
    private bool _isTracking = false;

    private void Update()
    {
        float x = Input.GetAxis(HorizontalAxis);
        float y = Input.GetAxis(VerticalAxis);

        //Debug.Log($"Input brut - X: {x}, Y: {y} | Magnitude: {new Vector2(x, y).magnitude}");

        if (new Vector2(x, y).magnitude < DeadZone)
        {
            _isTracking = false;
            return;
        }

        float currentAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        if (PizzaTransform != null)
        {
            PizzaTransform.rotation = Quaternion.Euler(0f, 0f, currentAngle + OffsetAngle);
        }

        if (!_isTracking)
        {
            _previousAngle = currentAngle;
            _isTracking = true;
            return;
        }

        float delta = Mathf.DeltaAngle(_previousAngle, currentAngle);
        _accumulatedDegrees -= delta;
        _previousAngle = currentAngle;

        if (Mathf.Abs(_accumulatedDegrees) >= 360f)
        {
            int turns = (int)(_accumulatedDegrees / 360f);

            _accumulatedDegrees %= 360f;

            TriggerTurn(turns);
        }
    }

    private void TriggerTurn(int turns)
    {
        Debug.Log($"[Simulateur] Le joueur a fait {turns} tour(s) au joystick !");
        OnTurnCompleted?.Raise(turns);
    }
}
