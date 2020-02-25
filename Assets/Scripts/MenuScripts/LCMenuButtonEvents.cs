using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class LCMenuButtonEvents : MonoBehaviour
{
    //Drag and drop input fields
    public VRTK_ControllerEvents menuLControllerEvents;
    public MenuManager manager;

    void OnEnable()
    {
        menuLControllerEvents.TriggerClicked += MenuLTriggerClicked;
    }

    void OnDisable()
    {
        menuLControllerEvents.TriggerClicked -= MenuLTriggerClicked;
    }

    //Code for when trigger is pressed
    private void MenuLTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        manager.GoToPrevious();
    }
}
