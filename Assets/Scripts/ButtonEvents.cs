using UnityEngine;
using VRTK;

// This class controls the behaviour for the remote controllers
public class ButtonEvents : MonoBehaviour {
    public VRTK_ControllerEvents controllerEvents;
    public GameObject menu;
	public GameObject popUpMenu;
    public Agent agent;

    public static bool menuState = false;

	// This will get called by Unity when the demo is loaded
    void OnEnable()
    {
        controllerEvents.GripReleased += ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed += ControllerEvents_ButtonTwoPressed;
		controllerEvents.TriggerClicked += ControllerEvents_TriggerClicked;
		GridWorld.OnGoalReached += EnvEvents_GoalReached;

		// Trigger clicks do not work with keyboard events in the simulator.
		//#if DEBUG
		//controllerEvents.TriggerPressed += ControllerEvents_TriggerClicked;
		//#endif
    }

	// This will get called by Unity when the demo is closed.
    void OnDisable()
    {
        controllerEvents.GripReleased -= ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed -= ControllerEvents_ButtonTwoPressed;
		controllerEvents.TriggerClicked -= ControllerEvents_TriggerClicked;
		GridWorld.OnGoalReached -= EnvEvents_GoalReached;
		// Trigger clicks do not work with keyboard events in the simulator.
		//#if DEBUG
		//controllerEvents.TriggerPressed -= ControllerEvents_TriggerClicked;
		//#endif
    }

	// This is the code for the button on top of the touchpad.
    private void ControllerEvents_ButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        agent.ResetEpisode();
    }

	// This is the code for both (either one of them needs to be pressed and released) grip buttons.
    private void ControllerEvents_GripReleased(object sender, ControllerInteractionEventArgs e)
    {
        menuState = !menuState;
        menu.SetActive(menuState);
    }

	private void ControllerEvents_TriggerClicked(object sender, ControllerInteractionEventArgs e)
	{
		if (menuState)
			return;
		
		var direction = e.controllerReference.actual.transform.forward;

		var thresh = 0.8;

		if (direction.z > thresh)
			agent.MoveForward ();
		else if (direction.z < -thresh)
			agent.MoveBackward ();
		else if (direction.x < -thresh)
			agent.MoveLeft ();
		else if (direction.x > thresh)
			agent.MoveRight ();
	}

	private void EnvEvents_GoalReached(){
		//Lock controls and activate pop-up menu
		menuState = true;
		popUpMenu.SetActive (menuState);
	}

	public void SetMenuState(bool status){
		menuState = status;
	}
}
