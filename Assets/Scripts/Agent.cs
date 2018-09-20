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
    [Range(0f, 1f)]
    private float learning_rate = 0.4f; // The rate at which to update the value estimates given a reward.
    [Range(0f, 1f)]
    private float discount_factor = 0.9f; // Discount factor for calculating Q-target.


    // Environment
    public GridWorld env;

    // UI vars
    public Text Q_UpEstimText;
    public Text Q_DownEstimText;
    public Text Q_LeftEstimText;
    public Text Q_RightEstimText;

	private Text[] texts;
    private Button[] buttons;

	// Learning Memory
	[HideInInspector]
	public bool learning = true;
	[HideInInspector]
	public Vector2Int lastState;
	private Dictionary<Action,float>[ , ] q_table;   // The matrix containing the q-value estimates.
	private int actionSize = Enum.GetNames(typeof(Action)).Length;
    
    public float Learning_rate
    {
        get
        {
            return learning_rate;
        }

        set
        {
            learning_rate = value;
            Debug.Log("Set new learning rate");
        }
    }
    
    public float Discount_factor
    {
        get
        {
            return discount_factor;
        }

        set
        {
            discount_factor = value;
            Debug.Log("Set new discount factor");
        }
    }

    /// <summary>
    /// Gets the current Estimate of the State Value
    /// </summary>
    /// <returns>The V-value of the current state.</returns>
    public float GetCurrentStateValue()
    {
        return GetStateValue(lastState);
    }

    public float GetStateValue(Vector2Int state)
    {
		// we assume a greedy policy
        float result = q_table[state.x, state.y].Values.Max();
        return result;
    }

    public float GetQval(Vector2Int state, Action action)
    {
		return q_table[state.x, state.y][action];
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
			q_table[state.x, state.y][action] += learning_rate * (reward - q_table[state.x, state.y][action]);
        }
        else
        {
			float q_max_nextState = q_table[nextState.x, nextState.y].Values.Max();
			q_table[state.x,state.y][action] += learning_rate * (reward + discount_factor * q_max_nextState - q_table[state.x,state.y][action]);
        }
        
    }
		
    private void Act(Action action)
    {
		float reward;
		try {
        	reward = env.Step(action);
		} catch (InvalidOperationException e) {
			Debug.Log ("Action invalid");
			return;
		}

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
		q_table = new Dictionary<Enums.Action, float>[env.gridSizeX, env.gridSizeY];
		buttons = new Button[4] {GameObject.Find("ForwardButton").GetComponent<Button>(),
                                 GameObject.Find("BackwardButton").GetComponent<Button>(),
                                 GameObject.Find("LeftButton").GetComponent<Button>(),
                                 GameObject.Find("RightButton").GetComponent<Button>()};
		texts = new Text[4]{ Q_UpEstimText, Q_DownEstimText, Q_LeftEstimText, Q_RightEstimText };
		ClearUI (this,new DestinationMarkerEventArgs());
    }

	void Start()
    {
		lastState = env.getCurrentState();
        clearMemory();
		UpdateUI(this,new DestinationMarkerEventArgs());
	}

    public void clearMemory()
    {
        for (int x = 0; x < env.gridSizeX; x++)
        {
            for (int y = 0; y < env.gridSizeY; y++)
            {
                List<Action> actions = env.getActions(new Vector2Int(x, y));
                Dictionary<Action, float> dict = new Dictionary<Enums.Action, float>();

				// FIXME: Change GridWorld to use 2D dictionary of nodes rather than a list of lists
				if (actions.Count == 0)
					dict.Add (Action.down, 0f);
				else {
					foreach (Action a in actions)
						dict.Add (a, 0f);
				}
                
	            q_table[x, y] = dict;
            }
        }
    }

	public void Update()
	{
		if (Input.GetKeyDown (KeyCode.W))
			MoveForward ();
		else if (Input.GetKeyDown (KeyCode.S))
			MoveBackward ();
		else if (Input.GetKeyDown (KeyCode.A))
			MoveLeft ();
		else if (Input.GetKeyDown (KeyCode.D))
			MoveRight ();
	}

	// Because we use this method as a VRTK teleport event we need the given signature
	public void UpdateUI(object sender, DestinationMarkerEventArgs e)
    {
		// get available actions
		List<Action> actions = env.getActions(env.getCurrentState());
		// set corresponding button active
		// set corresponding text active and update its value
		foreach(Action action in actions){
			buttons [(int) action].gameObject.SetActive(true);
			texts [(int) action].enabled = true;
			texts[(int) action].text = GetQval(env.getCurrentState(), action).ToString("n2");
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
