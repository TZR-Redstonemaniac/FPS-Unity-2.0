using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookGun : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    #region References

    [Header("References")] 
    [SerializeField] private Transform cam;
    [SerializeField] private Transform gunTip;
    [SerializeField] private LayerMask whatIsHookable;
    [SerializeField] private Player pm;

    #endregion

    #region Hooking

    [Header("Hooking")]
    [SerializeField] private float maxHookDistance;
    [SerializeField] private float hookDelay;

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

    private void StartHook()
    {
        
    }

    private void ExecuteHook()
    {
        
    }

    private void StopHook()
    {
        
    } 
}
