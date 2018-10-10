using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;

[Serializable]
public class Agent : MonoBehaviour {
    // Algorithm parameters
	// These are adjustable in a GUI menu
    [Range(0f, 1f)]
    private float learning_rate = 0.4f; // The rate at which to update the value estimates given a reward.
    [Range(0f, 1f)]
    private float discount_factor = 0.9f; // Discount factor for calculating Q-target.

    // Environment
    public GridWorld env;
	public GridWorldGUI envGUI;

	// Learning Memory
	private Vector2Int lastState;
	private Dictionary<Action,float>[ , ] q_table;   // The matrix containing the q-value estimates.
	//private int actionSize = Enum.GetNames(typeof(Action)).Length;
    
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
		return q_table[state.x, state.y].Values.Max();;
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
			// learning = false;
			q_table[state.x, state.y][action] += learning_rate * (reward - q_table[state.x, state.y][action]);
        }
        else
        {
			float q_max_nextState = q_table[nextState.x, nextState.y].Values.Max();
			q_table[state.x,state.y][action] += learning_rate * (reward + discount_factor * q_max_nextState - q_table[state.x,state.y][action]);
        }
        
    }
		
    private bool Act(Action action)
    {
		float reward;
		try {
        	reward = env.Step(action);
		} catch (InvalidOperationException e) {
			Debug.Log ("Action invalid");
			return false;
		}
		Debug.Log("Received reward: " + reward.ToString());
		Vector2Int nextState = env.getCurrentState();
		bool done = env.isTerminal();
		UpdateQTable(lastState, action, nextState, reward, done);
        lastState = nextState;
		return true;
    }

    public void MoveForward()
    {
		while (Act (Action.up)) {
			var actions = env.getActions (env.getCurrentState());
			if (actions.Contains (Action.left) ||
			    actions.Contains (Action.right))
				break;
		}

		// Change position of the agent in the VR world
		envGUI.moveAgentInGameWorld(lastState);
    }

    public void MoveBackward()
    {
		while (Act (Action.down)) {
			var actions = env.getActions (env.getCurrentState());
			if (actions.Contains (Action.left) ||
				actions.Contains (Action.right))
				break;
		}

		// Change position of the agent in the VR world
		envGUI.moveAgentInGameWorld(lastState);
    }

    public void MoveLeft()
    {
		while (Act (Action.left)) {
			var actions = env.getActions (env.getCurrentState());
			if (actions.Contains (Action.up) ||
				actions.Contains (Action.down))
				break;
		}

		// Change position of the agent in the VR world
		envGUI.moveAgentInGameWorld(lastState);
    }

    public void MoveRight()
    {
		while (Act (Action.right)) {
			var actions = env.getActions (env.getCurrentState());
			if (actions.Contains (Action.up) ||
				actions.Contains (Action.down))
				break;
		}

		// Change position of the agent in the VR world
		envGUI.moveAgentInGameWorld(lastState);
    }

    void Awake()
    {
		q_table = new Dictionary<Enums.Action, float>[env.gridSizeX, env.gridSizeY];

    }

	void Start()
    {
		lastState = env.getCurrentState();
        clearMemory();
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

	#if DEBUG
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
		else if (Input.GetKeyDown (KeyCode.Q))
			RestartLearning ();
		else if (Input.GetKeyDown (KeyCode.E))
			ResetEpisode ();
	}
	#endif

		
	public void RestartLearning()
	{
		clearMemory();
		ResetEpisode();
	}

	public void ResetEpisode()
	{
		env.ResetEpisode();
		lastState = env.getCurrentState ();
		envGUI.moveAgentInGameWorld (lastState, true);
	}
}
