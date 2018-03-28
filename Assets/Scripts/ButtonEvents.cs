using UnityEngine;
using VRTK;

public class ButtonEvents : MonoBehaviour {
    public VRTK_ControllerEvents controllerEvents;
    public GameObject menu;
    public GridWorld gridWorld;

    bool menuState = false;

    void OnEnable()
    {
        controllerEvents.GripReleased += ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed += ControllerEvents_ButtonTwoPressed;
    }

    void OnDisable()
    {
        controllerEvents.GripReleased -= ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed -= ControllerEvents_ButtonTwoPressed;
    }

    private void ControllerEvents_ButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        gridWorld.Reset();
    }

    private void ControllerEvents_GripReleased(object sender, ControllerInteractionEventArgs e)
    {
        menuState = !menuState;
        menu.SetActive(menuState);
    }
}
