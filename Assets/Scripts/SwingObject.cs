using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingObject : MonoBehaviour
{
    [Header("Swing settings")]
    [SerializeField] private Vector3 swingAxis;
    [SerializeField] private float swingDegree = 10;
    [SerializeField] private float swingSpeed = 1;

    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingDegree;

        transform.localRotation = startRotation * Quaternion.AngleAxis(angle, swingAxis.normalized);
    }
}
