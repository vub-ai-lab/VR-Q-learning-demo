using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using VRTK;
using Action = GridWorld.Action;

[Serializable]
public class Agent : MonoBehaviour {
    // Algorithm parameters
	// These are adjustable in a GUI menu
    [Range(0f, 1f)]
    private float learning_rate = 0.4f; // The rate at which to update the value estimates given a reward.
    [Range(0f, 1f)]
    private float discount_factor = 0.9f; // Discount factor for calculating Q-target.
	[Range(0f, 1f)]
	private float trace_decay = 0.8f; // Factor Lambda to decrease eligibility traces

    // Environment
    public GridWorld env;
	public GridWorldGUI envGUI;
    public PythonAgent py;

	// Learning Memory
	private int lastState;
	private Dictionary<Action,float>[] q_table;   // The matrix containing the q-value estimates.
	private Dictionary<Action,float>[] traces;  // Matrix containing the eligibility traces
    
    public float Learning_rate
    {
        get
        {
            return learning_rate;
        }

        set
        {
            learning_rate = value;
            Debug.Log("Set new learning rate.");
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
            Debug.Log("Set new discount factor.");
        }
    }

	public float Trace_decay
	{
		get
		{
			return trace_decay;
		}

		set
		{
			trace_decay = value;
			Debug.Log("Set new trace decay.");
		}
	}

	void Start()
	{
		// py.initializeQLearning (env.stateSpace, 4);
		q_table = new Dictionary<Action, float>[env.stateSpace];
		traces = new Dictionary<Action, float>[env.stateSpace];
		ClearMemory();
		lastState = env.getCurrentState();
		// FIXME VRTK teleporter would be preferred here but headset happens not to be enable when this is executed
		transform.localPosition = envGUI.getStatePosition(lastState);
	}

    /// <summary>
    /// Gets the current Estimate of the State Value
    /// </summary>
    /// <returns>The V-value of the current state.</returns>
    public float GetCurrentStateValue()
    {
        return GetStateValue(lastState);
    }

    public float GetStateValue(int state)
    {
		// we assume a greedy policy
		try {
			return q_table[state].Values.Max();
		} catch (InvalidOperationException e) {
			return 0f;
		}
    }

    public float GetQval(int state, Action action)
    {
        // var local = q_table[state][action];
        // var remote = py.GetQval(state, (int)action);
        // Debug.Log("Local: " + local + "; Remote: " + remote);

		return q_table[state][action];
    }


	public float GetTraceValue(int state, Action action)
	{
		return traces[state][action];
	}

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>
	public void UpdateQTable(int state, Action action, int nextState, float reward, bool terminal) 
	{
		float blop = terminal ? 0 : discount_factor * q_table [nextState].Values.Max ();
		float delta = reward + blop - q_table [state] [action];
		traces [state] [action] = 1;
		for (int x = 0; x < env.stateSpace; x++) {
			foreach (Action a in new List<Action>(q_table[x].Keys)) {
				q_table [x] [a] += learning_rate * delta * traces [x] [a];
				traces [x] [a] *= discount_factor * trace_decay;
			}
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
		var nextState = env.getCurrentState();
		bool done = env.isTerminal();
		UpdateQTable(lastState, action, nextState, reward, done);

        //if (env.isTerminal())
        //    py.UpdateQTable(lastState, (int)action, lastState, reward);
        //else
        //    py.UpdateQTable(lastState, (int)action, nextState, reward);


        lastState = nextState;
		return true;
    }

	private void WalkUntilCrossing(Action chosen_action, Action crossing_action1, Action crossing_action2)
	{
		int prevState = lastState;
		while (Act (chosen_action)) {
			var actions = env.getActions (env.getCurrentState());
			if (actions.Contains (crossing_action1) ||
				actions.Contains (crossing_action2))
				break;
		}

		envGUI.moveAgentInGameWorld(prevState,lastState);
	}


    public void MoveForward()
    {
		WalkUntilCrossing (Action.up, Action.left, Action.right);
    }

    public void MoveBackward()
    {
		WalkUntilCrossing (Action.down, Action.left, Action.right);
    }

    public void MoveLeft()
    {
		WalkUntilCrossing (Action.left, Action.up, Action.down);
    }

    public void MoveRight()
    {
		WalkUntilCrossing (Action.right, Action.up, Action.down);
    }

	private void ClearQtable()
	{
		for (int x = 0; x < env.stateSpace; x++)
		{
			List<Action> actions = env.getActions(x);
			Dictionary<Action, float> dict = new Dictionary<Action, float>();
	
			foreach (Action a in actions)
				dict.Add (a, 0f);
				
			q_table[x] = dict;
		}
	}

	private void ClearTraces()
	{
		for (int x = 0; x < env.stateSpace; x++)
		{
			List<Action> actions = env.getActions (x);
			Dictionary<Action, float> dict = new Dictionary<Action, float>();
	
			foreach (Action a in actions)
				dict.Add (a, 0f);

			traces[x] = dict;
		}
	}

    public void ClearMemory()
    {
		ClearQtable ();
		ClearTraces ();
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
		ClearMemory();
		ResetEpisode();
	}

	public void ResetEpisode()
	{
		ClearTraces ();
		envGUI.ResetEpisode();
		lastState = env.getCurrentState ();
		envGUI.moveAgentInGameWorld (lastState, lastState, true);
	}
}
