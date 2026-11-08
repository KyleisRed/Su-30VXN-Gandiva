using UnityEngine;

public class CanardSurface : MonoBehaviour
{
    [SerializeField] private Aircraft aircraft;
    [SerializeField] private Transform surface;
    [SerializeField] private AeroPart criticalPart;
    [SerializeField] private float DisableThreshold;

    [SerializeField] private float maxAngle = 30f;
    [SerializeField] private float minAngle = -30f;

    [Tooltip("X: alpha Y: response deflection")]
    [SerializeField] private AnimationCurve responseCurve = AnimationCurve.Linear(-30f, -30f, 30f, 30f);

    [Tooltip("Max rotation degrees per second.")]
    [Min(0f)]
    [SerializeField] private float responseSpeed = 90f;

    [Tooltip("Override alpha response while wheelbrakes when on the ground (gripen style)")]
    [SerializeField] private bool brakeFlare;

    [Tooltip("Brake deflection angle in degrees")]
    [SerializeField] private float brakeAngle = 30f;

    private LandingGear[] landingGear;
    private ControlInputs inputs;
    private Quaternion restingRotation;
    private float currentAngle;
    bool isFunctional = true;
    //aryx wuz here
    private void Awake()
    {
        if (aircraft == null || responseCurve == null || responseCurve.length == 0)
        {
            Debug.LogError("CanardSurface requires Aircraft and non-empty response curve.", this);
            enabled = false;
            return;
        }

        criticalPart.onPartDetached += Detach;
        criticalPart.onApplyDamage += Damage;

        restingRotation = surface.localRotation;
        inputs = aircraft.GetInputs();
        landingGear = aircraft.GetComponentsInChildren<LandingGear>(true);
    }

    private void LateUpdate()
    {
        if (aircraft == null || surface == null || !isFunctional)
        {
            return;
        }

        Vector3 localVelocity = aircraft.cockpit.transform.InverseTransformDirection(aircraft.CockpitRB().velocity - aircraft.GetWindVelocity());
        float alpha = localVelocity.y * localVelocity.y + localVelocity.z * localVelocity.z > 0.01f ? -Mathf.Atan2(localVelocity.y, localVelocity.z) * Mathf.Rad2Deg : 0f;
        float targetAngle = brakeFlare && inputs != null && inputs.brake > 0f && aircraft.radarAlt < 1f ? brakeAngle : responseCurve.Evaluate(alpha);
        targetAngle = Mathf.Clamp(targetAngle, Mathf.Min(minAngle, maxAngle), Mathf.Max(minAngle, maxAngle));
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Mathf.Max(0f, responseSpeed) * Time.deltaTime);
        surface.localRotation = restingRotation * Quaternion.AngleAxis(currentAngle, Vector3.right);
    }
    private void Damage(UnitPart.OnApplyDamage dmg)
    {
        if (criticalPart.hitPoints < DisableThreshold)
            Disable();
    }
    private void Detach(UnitPart part)
    {
        Disable();
    }
    private void Disable()
    {
        if (!isFunctional) return;

        isFunctional = false;
        criticalPart.onPartDetached -= Detach;
        criticalPart.onApplyDamage -= Damage;
    }
}