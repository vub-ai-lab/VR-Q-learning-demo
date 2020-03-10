using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;



[Serializable]
public class Agent : MonoBehaviour
{


    // Algorithms
    public QlearningWtraces qlearningWtraces;
    public SARSA sarsa;
    private String algorithmType;
    private Algorithm algorithm;

    public float Learning_rate
    {
        get
        {
            return algorithm.Learning_rate;
        }

        set
        {
            algorithm.Learning_rate = value;
            Debug.Log("Set new learning rate.");
        }
    }

    public float Discount_factor
    {
        get
        {
            return algorithm.Discount_factor;
        }

        set
        {
            algorithm.Discount_factor = value;
            Debug.Log("Set new discount factor.");
        }
    }

    public float Trace_decay
    {
        get
        {
            return algorithm.Trace_decay;
        }

        set
        {
            algorithm.Trace_decay = value;
            Debug.Log("Set new trace decay.");
        }
    }

    /// <summary>
    /// Gets the current Estimate of the State Value
    /// </summary>
    /// <returns>The V-value of the current state.</returns>
    public float GetCurrentStateValue()
    {
        return GetStateValue(algorithm.LastState);
    }

    public float GetStateValue(Vector2Int state)
    {
        return algorithm.GetStateValue(state);
        ;
    }

    public float GetQval(Vector2Int state, Action action)
    {
        return algorithm.GetQval(state, action);
    }


    public float GetTraceValue(Vector2Int state, Action action)
    {
        return algorithm.GetTraceValue(state, action);
    }

    public float GetPickChance(Vector2Int state, Action selectedAction, List<Action> actions)
    {
        return algorithm.GetPickChance(state, selectedAction, actions);
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>

    // Handle player movement
    private bool Act(Action action)
    {
        GridWorld env = algorithm.env;

        float reward;
        try
        {
            reward = env.Step(action);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Action invalid");
            return false;
        }
        Debug.Log("Received reward: " + reward.ToString());
        Vector2Int nextState = env.getCurrentState();
        bool done = env.isTerminal();

        algorithm.UpdateValues(action, nextState, reward, done);

        // UpdateQTable(algorithm.LastState, action, nextState, reward, done);
        //algorithm.LastState = nextState;

        return true;
    }

    private void WalkUntilCrossing(Action chosen_action, Action crossing_action1, Action crossing_action2)
    {
        GridWorld env = algorithm.env;
        GridWorldGUI envGUI = algorithm.envGUI;
        Vector2Int prevState = algorithm.LastState;

        while (Act(chosen_action))
        {
            var actions = env.getActions(env.getCurrentState());
            if (actions.Contains(crossing_action1) ||
                actions.Contains(crossing_action2))
                break;
        }

        envGUI.moveAgentInGameWorld(prevState, algorithm.LastState);
    }


    public void MoveForward()
    {
        WalkUntilCrossing(Action.up, Action.left, Action.right);
    }

    public void MoveBackward()
    {
        WalkUntilCrossing(Action.down, Action.left, Action.right);
    }

    public void MoveLeft()
    {
        WalkUntilCrossing(Action.left, Action.up, Action.down);
    }

    public void MoveRight()
    {
        WalkUntilCrossing(Action.right, Action.up, Action.down);
    }

    void Start()
    {


        // Get the algorithm the user selected in the StartMenu
        algorithmType = UnityEngine.PlayerPrefs.GetString("Algorithm");
        //PlayerPrefs.DeleteAll();

        switch (algorithmType)
        {
            case "Qlearning":
                algorithm = qlearningWtraces;
                Debug.Log("LoadedQlearning");
                break;
            case "SARSA":
                algorithm = sarsa;
                Debug.Log("Loaded SARSA");
                break;
        }


        algorithm.Initialize();

        Debug.Log(algorithm.Test());
    }


    public void ClearMemory()
    {
        algorithm.ClearMemory();
    }

#if DEBUG
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            MoveForward();
        else if (Input.GetKeyDown(KeyCode.S))
            MoveBackward();
        else if (Input.GetKeyDown(KeyCode.A))
            MoveLeft();
        else if (Input.GetKeyDown(KeyCode.D))
            MoveRight();
        else if (Input.GetKeyDown(KeyCode.E))
            ResetEpisode();
    }
#endif


    public void RestartLearning()
    {
        algorithm.RestartLearning();
    }

    public void ResetEpisode()
    {
        algorithm.ResetEpisode();
    }
}


