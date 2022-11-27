using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    [SerializeField] private int quality;
    [SerializeField] private float damper;
    [SerializeField] private float strength;
    [SerializeField] private float velocity;
    [SerializeField] private float waveCount;
    [SerializeField] private float waveHeight;
    [SerializeField] private float speed;

    [SerializeField] private AnimationCurve affectCurve;

    [HideInInspector] public bool drawRope;
    [HideInInspector] public Vector3 startPos;
    [HideInInspector] public Vector3 endPos;
    [HideInInspector] public LineRenderer lr;
    
    private Spring spring;
    private Vector3 endPosLerp;
    
    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();
    }
    
    private void DrawRope()
    {
        if (!drawRope)
        {
            endPosLerp = startPos;
            spring.Reset();
            lr.enabled = false;
            return;
        }

        if (lr.enabled == false)
        {
            spring.SetVelocity(velocity);
            lr.enabled = true;
            lr.positionCount = quality + 1;
        }
        
        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var finalPoint = endPos;
        var startPosition = startPos;
        var up = Quaternion.LookRotation((finalPoint - startPosition).normalized) * Vector3.up;
        
        endPosLerp = Vector3.Lerp(endPosLerp, finalPoint, Time.deltaTime * speed);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);
            
            lr.SetPosition(i, Vector3.Lerp(startPosition, endPosLerp, delta) + offset);
        }
    }
}
