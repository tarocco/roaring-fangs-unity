/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;

public class DeluxeCam : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    private Transform _FollowTarget;

    public Transform FollowTarget
    {
        get { return _FollowTarget; }
        set { _FollowTarget = value; }
    }

    [SerializeField]
    private Rigidbody _Rigidbody;

    public Rigidbody Rigidbody
    {
        get { return _Rigidbody; }
        set { _Rigidbody = value; }
    }

    [SerializeField]
    private AnimationCurve _HorizontalAlignmentCurve =
        AnimationCurve.Linear(0.0f, 0.5f, 1.0f, 0.5f);

    public AnimationCurve HorizontalAlignmentCurve
    {
        get { return _HorizontalAlignmentCurve; }
        set { _HorizontalAlignmentCurve = value; }
    }

    [SerializeField]
    private AnimationCurve _VerticalAlignmentCurve =
        AnimationCurve.Linear(0.0f, 0.5f, 1.0f, 0.5f);

    public AnimationCurve VerticalAlignmentCurve
    {
        get { return _VerticalAlignmentCurve; }
        set { _VerticalAlignmentCurve = value; }
    }

    [SerializeField]
    private Bounds _DeadZone = new Bounds(Vector3.zero, Vector3.one);

    public Bounds DeadZone
    {
        get { return _DeadZone; }
        set { _DeadZone = value; }
    }

    private void Start()
    {
    }

    private Vector3 CalculateNextPosition(
        Vector3 target_position,
        Vector3 alignment_position)
    {
        // TODO: axis-independent calculations
        var difference = target_position - alignment_position;
        var x_interpolant = HorizontalAlignmentCurve.Evaluate(Mathf.Abs(difference.x));
        var y_interpolant = VerticalAlignmentCurve.Evaluate(Mathf.Abs(difference.y));
        var next_point = new Vector3(
            Mathf.Lerp(alignment_position.x, target_position.x, x_interpolant),
            Mathf.Lerp(alignment_position.y, target_position.y, y_interpolant),
            alignment_position.z);
        return next_point;
    }

    private void FixedUpdate()
    {
        if (!FollowTarget)
            return;
        
        var next_position = CalculateNextPosition(FollowTarget.position, transform.position);

        // Direct approach
        //transform.position = next_position;

        // Velocity approach
        
        var delta = next_position - transform.position;
        var velocity = delta / Time.fixedDeltaTime;
        Rigidbody.velocity = velocity;
        
    }

    public void OnBeforeSerialize()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    public void OnAfterDeserialize()
    {
    }
}