using UnityEngine;
using VRTK;

// This class controls the behaviour for the remote controllers
public class ButtonEvents : MonoBehaviour {
    public VRTK_ControllerEvents controllerEvents;
    public GameObject menu;
	public GameObject popUpMenu;
    public GameObject HoleMenu;
    public Agent agent;

    public static bool disabled = false;
    public static bool menuInitialized = false;

	// This will get called by Unity when the demo is loaded
    void OnEnable()
    {
        controllerEvents.GripReleased += ControllerEvents_GripReleased;
        controllerEvents.ButtonTwoPressed += ControllerEvents_ButtonTwoPressed;
		controllerEvents.TriggerClicked += ControllerEvents_TriggerClicked;
		GridWorld.OnGoalReached += EnvEvents_GoalReached;
        GridWorld.OnFellInHole += EnvEvents_FellInHole;

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
        GridWorld.OnFellInHole -= EnvEvents_FellInHole;
        // Reset booleans
        disabled = false;
        menuInitialized = false;
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
		disabled = !disabled;
        menu.SetActive(disabled);
        if (!menuInitialized)
        {
            agent.InitializeMenu();
            menuInitialized = true;
        }

 

    }

	private void ControllerEvents_TriggerClicked(object sender, ControllerInteractionEventArgs e)
	{
		if (disabled)
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
		disabled = true;
		popUpMenu.SetActive (disabled);
	}

    private void EnvEvents_FellInHole()
    {
        //Lock controls and activate pop-up menu
        disabled = true;
        HoleMenu.SetActive(disabled);
    }

	public void SetDisabled(bool status){
		disabled = status;
	}
}
