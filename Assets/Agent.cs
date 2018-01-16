using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Action = Enums.Action;

[Serializable]
public class Agent : MonoBehaviour {

    float [ , ][] q_table;   // The matrix containing the q-value estimates.
    
    // Algorithm parameters
    public float learning_rate = 0.5f; // The rate at which to update the value estimates given a reward.
    public float discount_factor = 1.0f; // Discount factor for calculating Q-target.

    // Misc 
    Vector2Int lastState;

    // UI vars
    public Text Q_UpEstimText;
    public Text Q_DownEstimText;
    public Text Q_LeftEstimText;
    public Text Q_RightEstimText;

	private Text[] texts;
    private Button[] buttons;

    // Environment
    private GridWorld env;
	private int actionSize = Enum.GetNames(typeof(Action)).Length;

    /// <summary>
    /// Gets the current Estimate of the State Value
    /// </summary>
    /// <returns>The V-value of the current state.</returns>
	public float GetCurrentStateValue()
    {
        return GetStateValue(lastState);
    }

    private float GetStateValue(Vector2Int state)
    {
		// Since the human user performs the action selection, 
		// we assume a uniform random policy
        float result = q_table[state.x, state.y].Average();
        return result;
    }

    public float GetQval(Vector2Int state, Action action)
    {
		return q_table[state.x, state.y][(int) action];
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>
    public void UpdateQTable(Action action, Vector2Int state, float reward, bool done) 
	{
        if (done)
        {
			q_table[lastState.x, lastState.y][(int)action] += learning_rate * (reward - q_table[lastState.x, lastState.y][(int)action]);  
        }
        else
        {
            float q_max_nextState = q_table[state.x, state.y].Max();
			q_table[lastState.x,lastState.y][(int) action] += learning_rate * (reward + discount_factor * q_max_nextState - q_table[lastState.x,lastState.y][(int)action]);
        }
        
    }

    private IEnumerator WaitAndReset(float time)
    {
        // Deactivate buttons
        DeActivateUIButtons();
        // Pause
        yield return new WaitForSeconds(time);
        // Reset environment
        env.Reset();
        // Activate buttons
        ActivateUIButtons();
    }



    private void Act(Action action)
    {
        float reward = env.Step(action);
        Debug.Log("Received reward: " + reward.ToString());
        Vector2Int nextState = env.getCurrentState();
        bool done = env.done;
        UpdateQTable(action, nextState, reward, done);
        lastState = nextState;
        UpdateUI();
    }

    public void MoveForward()
    {
		Act(Action.up);

    }

    public void MoveBackward()
    {
        Act(Action.down);

    }

    public void MoveLeft()
    {
		Act(Action.left);

    }

    public void MoveRight()
    {
		Act(Action.right);

    }

    void Awake()
    {
        env = GameObject.Find("GridWorld").GetComponent<GridWorld>();
		q_table = new float[env.gridSizeX, env.gridSizeY][];
		lastState = env.getCurrentState();
		buttons = new Button[4] {GameObject.Find("ForwardButton").GetComponent<Button>(),
                                 GameObject.Find("BackwardButton").GetComponent<Button>(),
                                 GameObject.Find("LeftButton").GetComponent<Button>(),
                                 GameObject.Find("RightButton").GetComponent<Button>()};
		texts = new Text[4]{ Q_UpEstimText, Q_DownEstimText, Q_LeftEstimText, Q_RightEstimText };
		ClearUI ();
    }
		
	void Start () {
		for(int x=0; x < env.gridSizeX; x++)
		{
			for (int y = 0; y< env.gridSizeY; y++)
			{
				q_table[x, y] = new float[4];
			}
		}
		UpdateUI();
	}

	/// <summary>
	/// Updates the UI.
	/// This function will activate the necessay UI elements, 
	/// depending on the available actions in the current state.
	/// </summary>
    public void UpdateUI()
    {
		// get available actions
		List<Action> actions = env.getActions(lastState);
		// set corresponding button active
		// set corresponding text active and update its value
		foreach(Action action in actions){
			buttons [(int) action].gameObject.SetActive(true);
			texts [(int) action].enabled = true;
			texts[(int) action].text = GetQval(lastState, action).ToString();
		}
    }

	public void ClearUI()
	{
		foreach (Button button in buttons) {
			button.gameObject.SetActive(false);
		}
		foreach (Text text in texts) {
			text.enabled = false;
		}
	}
		
    public void ActivateUIButtons() { SetInteractableButtons(true); }
    public void DeActivateUIButtons() { SetInteractableButtons(false); }
    private void SetInteractableButtons(bool value)
    {
        foreach(Button button in buttons)
        {
            button.interactable = value;
        }
    }
}
