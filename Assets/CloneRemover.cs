using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable SuggestBaseTypeForParameter

public class CloneRemover : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    [SerializeField] private string[] cloneNames;
    [SerializeField] private float deleteDelay;

    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Update()
    {
        foreach (var i in cloneNames)
        {
            var o = GameObject.Find(i + "(Clone)");
            if (o is not null)
            {
                o.name = o.name.Remove(o.name.Length - 7);
                DeleteObject(o);
            }
        }
    }

    private void DeleteObject(GameObject obj)
    {
        Destroy(obj, deleteDelay);
    }
}
