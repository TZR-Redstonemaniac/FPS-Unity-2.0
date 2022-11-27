using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HookGun : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    #region References

    [Header("References")] 
    [SerializeField] private Player pm;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform gunTip;
    [SerializeField] private LayerMask whatIsHookable;
    [SerializeField] private Rope rope;
    [SerializeField] private GameObject validCrosshair;
    [SerializeField] private GameObject invalidCrosshair;

    #endregion

    #region Hooking

    [Header("Hooking")]
    [SerializeField] private float maxHookDistance;
    [SerializeField] private float hookDelay;
    [SerializeField] private float overshootYAxis;

    private Vector3 hookPoint;

    #endregion

    #region Cooldown

    [Header("Cooldown")]
    [SerializeField] private float hookCd;
    
    private float hookCdTimer;

    #endregion

    #region Input
    
    private bool hooking;

    #endregion

    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Update()
    {
        if (pm.fireLeft) StartHook();

        if (hookCdTimer > 0) hookCdTimer -= Time.deltaTime;

        rope.drawRope = hooking;
        rope.startPos = gunTip.position;

        if (Physics.Raycast(cam.position, cam.forward, out _, maxHookDistance, whatIsHookable))
        {
            validCrosshair.SetActive(true);
            invalidCrosshair.SetActive(false);
        }
        else
        {
            validCrosshair.SetActive(false);
            invalidCrosshair.SetActive(true);
        }
    }

    private void StartHook()
    {
        if(hookCdTimer > 0) return;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, maxHookDistance, whatIsHookable))
        {
            hookPoint = hit.point;
            
            hooking = true;
            pm.freeze = true;
            
            Invoke(nameof(ExecuteHook), hookDelay);
        }
        else
        {
            hookPoint = cam.position + cam.forward * maxHookDistance;
            
            Invoke(nameof(StopHook), hookDelay);
        }
        
        rope.endPos = hookPoint;
    }

    private void ExecuteHook()
    {
        pm.freeze = false;

        var lowestPoint = new Vector3(transform.position.x, transform.position.y - pm.playerHeight / 2, transform.position.z);

        var hookPointRelativeYPos = hookPoint.y - lowestPoint.y;
        var highestPointOnArc = hookPointRelativeYPos < 0 ? overshootYAxis : hookPointRelativeYPos + overshootYAxis;
        
        pm.JumpToPosition(hookPoint, highestPointOnArc);
        
        Invoke(nameof(StopHook), 1f);
    }

    public void StopHook()
    {
        pm.freeze = false;
        pm.state = Player.MovementState.air;

        hooking = false;

        hookCdTimer = hookCd;
        
    }
}
