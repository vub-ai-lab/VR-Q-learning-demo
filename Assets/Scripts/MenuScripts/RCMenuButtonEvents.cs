using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class RCMenuButtonEvents : MonoBehaviour
{
    //Drag and drop input fields
    public VRTK_ControllerEvents menuRControllerEvents;
    public MenuManager manager;

    void OnEnable()
    {
        menuRControllerEvents.TriggerClicked += MenuRTriggerClicked;
    }

    void OnDisable()
    {
        menuRControllerEvents.TriggerClicked -= MenuRTriggerClicked;
    }

    //Code for when trigger is pressed
    private void MenuRTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {

    }

}
