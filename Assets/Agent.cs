using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Action = Enums.Action;
using VRTK;

[Serializable]
public class Agent : MonoBehaviour {
    // Algorithm parameters
    public float learning_rate; // The rate at which to update the value estimates given a reward.
    public float discount_factor; // Discount factor for calculating Q-target.

	// Environment
	public GridWorld env;

    // UI vars
    public Text Q_UpEstimText;
    public Text Q_DownEstimText;
    public Text Q_LeftEstimText;
    public Text Q_RightEstimText;

	private Text[] texts;
    private Button[] buttons;

	// Learning Memory\
	[HideInInspector]
	public bool learning = true;
	[HideInInspector]
	public Vector2Int lastState;
	private float [ , ][] q_table;   // The matrix containing the q-value estimates.
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
	public void UpdateQTable(Vector2Int state, Action action, Vector2Int nextState, float reward, bool terminal) 
	{
        if (terminal)
        {
			learning = false;
			q_table[state.x, state.y][(int)action] += learning_rate * (reward - q_table[state.x, state.y][(int)action]);
        }
        else
        {
			float q_max_nextState = q_table[nextState.x, nextState.y].Max();
			q_table[state.x,state.y][(int) action] += learning_rate * (reward + discount_factor * q_max_nextState - q_table[state.x,state.y][(int)action]);
        }
        
    }

    private IEnumerator WaitAndReset(float time)
    {
        // Pause
        yield return new WaitForSeconds(time);
        // Reset environment
        env.Reset();
    }
		
    private void Act(Action action)
    {
        float reward = env.Step(action);
		Debug.Log("Received reward: " + reward.ToString());
		Vector2Int nextState = env.getCurrentState();
		if (learning) {
			bool done = env.isTerminal();
			UpdateQTable(lastState, action, nextState, reward, done);
		}
        lastState = nextState;
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
		q_table = new float[env.gridSizeX, env.gridSizeY][];
		buttons = new Button[4] {GameObject.Find("ForwardButton").GetComponent<Button>(),
                                 GameObject.Find("BackwardButton").GetComponent<Button>(),
                                 GameObject.Find("LeftButton").GetComponent<Button>(),
                                 GameObject.Find("RightButton").GetComponent<Button>()};
		texts = new Text[4]{ Q_UpEstimText, Q_DownEstimText, Q_LeftEstimText, Q_RightEstimText };
		ClearUI (this,new DestinationMarkerEventArgs());
    }

	void Start () {
		lastState = env.getCurrentState();
		for(int x=0; x < env.gridSizeX; x++)
		{
			for (int y = 0; y< env.gridSizeY; y++)
			{
				q_table[x, y] = new float[actionSize];
			}
		}
		UpdateUI(this,new DestinationMarkerEventArgs());
	}

	// Because we use this method as a VRTK teleport event we need the given signature
	public void UpdateUI(object sender, DestinationMarkerEventArgs e)
    {
		// get available actions
		List<Action> actions = env.getActions();
		// set corresponding button active
		// set corresponding text active and update its value
		foreach(Action action in actions){
			buttons [(int) action].gameObject.SetActive(true);
			texts [(int) action].enabled = true;
			texts[(int) action].text = GetQval(lastState, action).ToString();
		}
    }

	// Because we use this method as a VRTK teleport event we need the given signature
	public void ClearUI(object sender, DestinationMarkerEventArgs e)
	{
		foreach (Button button in buttons) {
			button.gameObject.SetActive(false);
		}
		foreach (Text text in texts) {
			text.enabled = false;
		}
	}
}
