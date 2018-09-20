using UnityEngine;
using VRTK;

// This class controls the behaviour for the remote controllers
public class ButtonEvents : MonoBehaviour {
    public VRTK_ControllerEvents controllerEvents;
    public GameObject menu;
    public GridWorld gridWorld;

    bool menuState = false;

	// This will get called by Unity when the demo is loaded
    void OnEnable()
    {
        controllerEvents.GripReleased += ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed += ControllerEvents_ButtonTwoPressed;
    }

	// This will get called by Unity when the demo is closed.
    void OnDisable()
    {
        controllerEvents.GripReleased -= ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed -= ControllerEvents_ButtonTwoPressed;
    }

	// This is the code for the button on top of the touchpad.
    private void ControllerEvents_ButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        gridWorld.Reset();
    }

	// This is the code for both (either one of them needs to be pressed and released) grip buttons.
    private void ControllerEvents_GripReleased(object sender, ControllerInteractionEventArgs e)
    {
        menuState = !menuState;
        menu.SetActive(menuState);
    }
}
