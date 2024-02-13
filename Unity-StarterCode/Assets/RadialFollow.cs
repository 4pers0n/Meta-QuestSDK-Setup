using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

/// <summary>
/// A script that updates the transform of the gameobject being attached by this,
/// so that it follows the view of your main camera.
/// </summary>
public class RadialFollow : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Main Camera to be used. Default to Camera.main")]
    private Transform _camera;

    private Transform _trackedObject;

    [SerializeField]
    [Tooltip("Min distance from camera to this gameobject")]
    private float _minDistance = 1f;

    [SerializeField]
    [Tooltip("Max distance from camera to this gameobject")]
    private float _maxDistance = 2f;

    [SerializeField]
    [Tooltip("The gameobject will stay at least this far away from the center of view")]
    private float _minViewDegrees = 0f;

    [SerializeField]
    [Tooltip("The gameobject will stay at most this far away from the center of view")]
    private float _maxViewDegrees = 30f;

    private const float POSITION_FUZZY_CORRECTION = 7f;
    private const float POSITION_VIEWDEGREE_SMOOTH_FACTOR = 0.0006f;
    private const float POSITION_LERP_SMOOTH_FACTOR = 3f;
    private const float ROTATION_LERP_SMOOTH_FACTOR = 3f;


    private void Start()
    {
        if (_camera == null) _camera = Camera.main.transform;
        _trackedObject = transform;
    }

    private void FixedUpdate()
    {
        // Solve for the new position of the tracked gameobject
        // We want to make sure it doesn't move if it's within the view degrees
        // We solve system of equations to find the new location of it by using the
        // maxViewDegrees if it's out of bounds
        Vector3 toObject = (_trackedObject.position - _camera.position).normalized;
        Vector3 toForward = _camera.forward.normalized;
        float angleBetween = Vector3.Angle(toForward, toObject);
        Vector3 newTargetInBounds = _trackedObject.position - _camera.position;

        if (angleBetween > (_maxViewDegrees + POSITION_FUZZY_CORRECTION))
        {
            newTargetInBounds = (toObject
                + POSITION_VIEWDEGREE_SMOOTH_FACTOR
                * (angleBetween - _maxViewDegrees)
                * (angleBetween - _maxViewDegrees)
                * (toForward - toObject)).normalized *
                Vector3.Magnitude(newTargetInBounds);
        }

        if (angleBetween < (_minViewDegrees - POSITION_FUZZY_CORRECTION))
        {
            newTargetInBounds = (toObject
                + -POSITION_VIEWDEGREE_SMOOTH_FACTOR
                * (_minViewDegrees - angleBetween)
                * (_minViewDegrees - angleBetween)
                * (toForward - toObject)).normalized *
                Vector3.Magnitude(newTargetInBounds);
        }

        // Correct its distance after the View Degree calculation
        float newObjectToCameraDistance = Vector3.Magnitude(newTargetInBounds);

        if (newObjectToCameraDistance < _minDistance)
            newObjectToCameraDistance = _minDistance;
        if (newObjectToCameraDistance > _maxDistance)
            newObjectToCameraDistance = _maxDistance;

        newTargetInBounds = newTargetInBounds.normalized * newObjectToCameraDistance;

        // Update the position and rotation with new values
        _trackedObject.SetPositionAndRotation(
            Vector3.Lerp(_trackedObject.position, newTargetInBounds + _camera.position, 
            POSITION_LERP_SMOOTH_FACTOR * Time.fixedDeltaTime), 
            Quaternion.Lerp(_trackedObject.rotation, Quaternion.LookRotation(_trackedObject.position - _camera.position, Vector3.up),
            ROTATION_LERP_SMOOTH_FACTOR * Time.fixedDeltaTime));
    }
}
