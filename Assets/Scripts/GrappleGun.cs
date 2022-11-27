using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    [Header("References")]
    [SerializeField] private Player pm;
    [SerializeField] private Transform gunTip, cam, player;
    [SerializeField] private Rope rope;
    [SerializeField] private GameObject validCrosshair;
    [SerializeField] private GameObject invalidCrosshair;
    [SerializeField] private LayerMask whatIsGrappleable;
    
    [Header("Swinging")]
    [SerializeField] private float maxSwingDistance;
    [SerializeField] private float spring;
    [SerializeField] private float damper;
    [SerializeField] private float massScale;
    
    [Header("Air Control")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float horizontalThrustForce;
    [SerializeField] private float forwardThrustForce;
    [SerializeField] private float extendCableSpeed;

    private Vector3 swingPoint;
    private SpringJoint joint;
    private bool swinging;
    
    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Update()
    {
        if (pm.fireLeft && !swinging) StartSwing();
        if (!pm.fireLeft && swinging) EndSwing();

        rope.startPos = gunTip.position;
        
        if (Physics.Raycast(cam.position, cam.transform.forward, out _, maxSwingDistance, whatIsGrappleable))
        {
            validCrosshair.SetActive(true);
            invalidCrosshair.SetActive(false);
        }
        else
        {
            validCrosshair.SetActive(false);
            invalidCrosshair.SetActive(true);
        }

        pm.swinging = swinging;
        
        if (swinging) AirControl();
    }

    private void AirControl()
    {
        switch (pm.moveVector.x)
        {
            case > 0:
                rb.AddForce(player.right * horizontalThrustForce * Time.deltaTime);
                break;
            case < 0:
                rb.AddForce(-player.right * horizontalThrustForce * Time.deltaTime);
                break;
        }
        
        if (pm.moveVector.z > 0) rb.AddForce(player.forward * forwardThrustForce * Time.deltaTime);

        if (pm.jump)
        {
            var directionToPoint = swingPoint - player.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            var distanceToPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceToPoint * 1.2f;
            joint.minDistance = distanceToPoint * .25f;
        }
        if (pm.moveVector.z < 0)
        {
            var extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 1.2f;
            joint.minDistance = extendedDistanceFromPoint * .25f;
        }
    }

    private void StartSwing()
    {
        if (Physics.Raycast(cam.position, cam.transform.forward, out var hit, maxSwingDistance, whatIsGrappleable))
        {
            swinging = true;
            
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;
            joint.enableCollision = true;

            var distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.minDistance = distanceFromPoint * .25f;
            joint.maxDistance = distanceFromPoint * 1.2f;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;
            
            rope.drawRope = true;
            rope.startPos = gunTip.position;
            rope.endPos = swingPoint;
        }
    }

    private void EndSwing()
    {
        swinging = false;
        rope.drawRope = false;
        Destroy(joint);
    }
}
