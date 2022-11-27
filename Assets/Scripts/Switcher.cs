using System;
using UnityEngine;

public class Switcher : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    [SerializeField] private int selected;
    [SerializeField] private Player pm;

    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Start()
    {
        Select();
    }

    private void Update()
    {
        var prevSelected = selected;
        
        if (pm.scrollUp)
        {
            if (selected >= transform.childCount - 1) selected = 0;
            else selected++;
        }
        
        if (pm.scrollDown)
        {
            if (selected <= 0) selected = transform.childCount - 1;
            else selected--;
        }

        if (prevSelected != selected)
        {
            Select();
        }
    }

    private void Select()
    {
        var i = 0;
        foreach (Transform selectedObject in transform)
        {
            selectedObject.gameObject.SetActive(i == selected);

            i++;
        }
    }
}
