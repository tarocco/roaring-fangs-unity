using RoaringFangs.Adapters.FluffyUnderware.Curvy;
using RoaringFangs.Attributes;
using RoaringFangs.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
#if UNITY_EDITOR
[CanEditMultipleObjects]
#endif
public class SplineConstraint : MonoBehaviour, ISerializationCallbackReceiver
{
    public enum ERotationConstraintMode
    {
        None,
        Tangent,
        Normal,
        Binormal
    }

    [SerializeField, AutoProperty("Spline")]
    private MonoBehaviour _SplineBehavior;

    public ICurvySpline Spline
    {
        get { return _SplineBehavior as ICurvySpline; }
        set { _SplineBehavior = value as MonoBehaviour; }
    }

    [SerializeField, AutoProperty]
    private float _PathAngle;

    public float PathAngle
    {
        get { return _PathAngle; }
        set { _PathAngle = value; }
    }

    [SerializeField, AutoProperty]
    private bool _MaintainDistance;

    public bool MaintainDistance
    {
        get { return _MaintainDistance; }
        set { _MaintainDistance = value; }
    }

    [SerializeField, AutoProperty]
    private float _Distance;

    public float Distance
    {
        get { return _Distance; }
        set { _Distance = value; }
    }

    [SerializeField, AutoProperty]
    private Vector3 _PositionOffset;

    public Vector3 PositionOffset
    {
        get { return _PositionOffset; }
        set { _PositionOffset = value; }
    }

    [SerializeField, AutoProperty]
    private ERotationConstraintMode _RotationConstraintMode;

    public ERotationConstraintMode RotationConstraintMode
    {
        get { return _RotationConstraintMode; }
        set { _RotationConstraintMode = value; }
    }

    [SerializeField, AutoProperty]
    private bool _AllowRoll;

    public bool AllowRoll
    {
        get { return _AllowRoll; }
        set { _AllowRoll = value; }
    }

    [SerializeField, AutoProperty]
    private Quaternion _RotationOffset;

    public Quaternion RotationOffset
    {
        get { return _RotationOffset; }
        set { _RotationOffset = value; }
    }

    private static void ApplyConstraint(
        Transform transform,
        ICurvySpline spline,
        float path_angle,
        ref float? distance,
        Vector3 position_offset,
        ERotationConstraintMode rotation_constraint_mode,
        bool allow_roll,
        Quaternion rotation_offset)
    {
        var relative_position = spline.transform.InverseTransformPoint(transform.position);
        var nearest_tf = spline.GetNearestPointTF(relative_position);
        var nearest_point = spline.Interpolate(nearest_tf);
        var position_to_point = relative_position - nearest_point;

        if (distance.HasValue)
            position_to_point = distance.Value * position_to_point.normalized;
        else
            distance = Vector3.Distance(nearest_point, relative_position);

        var tangent = spline.GetTangent(nearest_tf);
        var orientation = Quaternion.AngleAxis(path_angle, tangent) * spline.GetOrientationFast(nearest_tf);
        var normal = orientation * Vector3.up;

        var relative_constrained_position = Vector3.ProjectOnPlane(position_to_point, normal) + nearest_point;
        relative_constrained_position += orientation * position_offset;
        var constrained_position = spline.transform.TransformPoint(relative_constrained_position);
        transform.position = constrained_position;

        Vector3 up;
        if (allow_roll)
            up = transform.up;
        else
            up = Vector3.up;

        switch (rotation_constraint_mode)
        {
            default:
            case ERotationConstraintMode.None:
                break;

            case ERotationConstraintMode.Tangent:
                transform.rotation = Quaternion.LookRotation(tangent, up) * rotation_offset;
                break;

            case ERotationConstraintMode.Normal:
                transform.rotation = Quaternion.LookRotation(normal, up) * rotation_offset;
                break;

            case ERotationConstraintMode.Binormal:
                var binormal = Vector3.Cross(tangent, normal);
                transform.rotation = Quaternion.LookRotation(binormal, up) * rotation_offset;
                break;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (Spline != null && !Spline.Dirty)
        {
            float? distance = MaintainDistance ? Distance : (float?)null;
            ApplyConstraint(
                transform,
                Spline,
                PathAngle,
                ref distance,
                PositionOffset,
                RotationConstraintMode,
                AllowRoll,
                RotationOffset);
            if (distance.HasValue)
                Distance = distance.Value;
        }
    }

    public void OnBeforeSerialize()
    {
        EditorUtilities.OnBeforeSerializeAutoProperties(this);
    }

    public void OnAfterDeserialize()
    {
    }
}